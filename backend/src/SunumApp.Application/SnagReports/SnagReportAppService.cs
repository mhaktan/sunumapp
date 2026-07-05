using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using SunumApp.Entities;
using SunumApp.SnagReports.Dto;
using SunumApp.Authorization;
using SunumApp.Flows;

namespace SunumApp.SnagReports
{
    public class SnagReportAppService : AsyncCrudAppService<
        SnagReport,
        SnagReportDto,
        long,
        PagedSnagReportResultRequestDto,
        CreateSnagReportDto,
        SnagReportDto>,
        ISnagReportAppService
    {
        private readonly IRepository<StatusChangeLog, long> _statusChangeLogRepo;
        private readonly IRepository<ApprovalRecord, Guid> _approvalRepo;
        private readonly IFlowEngine _flowEngine;

        public SnagReportAppService(IRepository<SnagReport, long> repository, IFlowEngine flowEngine, IRepository<StatusChangeLog, long> statusChangeLogRepo, IRepository<ApprovalRecord, Guid> approvalRepo)
            : base(repository)
        {
            _flowEngine = flowEngine;
            _statusChangeLogRepo = statusChangeLogRepo;
            _approvalRepo = approvalRepo;
            // Claim-based authorization (JwtPermissionChecker reads JWT "permission" claims)
            GetPermissionName = PermissionNames.SnagReport_Read;
            GetAllPermissionName = PermissionNames.SnagReport_Read;
            CreatePermissionName = PermissionNames.SnagReport_Create;
            UpdatePermissionName = PermissionNames.SnagReport_Update;
            DeletePermissionName = PermissionNames.SnagReport_Delete;
        }

        protected override IQueryable<SnagReport> CreateFilteredQuery(PagedSnagReportResultRequestDto input)
        {
            return Repository.GetAll()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.Id.ToString().Contains(input.Keyword) ||
                    (x.ReportNumber != null && x.ReportNumber.Contains(input.Keyword)) ||
                    (x.AtaChapter != null && x.AtaChapter.Contains(input.Keyword)) ||
                    (x.Title != null && x.Title.Contains(input.Keyword)) ||
                    (x.Description != null && x.Description.Contains(input.Keyword)) ||
                    (x.ActionDescription != null && x.ActionDescription.Contains(input.Keyword)) ||
                    (x.RevisionNote != null && x.RevisionNote.Contains(input.Keyword)))
                .WhereIf(!input.ReportNumber.IsNullOrWhiteSpace(), x => x.ReportNumber != null && x.ReportNumber.Contains(input.ReportNumber))
                .WhereIf(!input.AtaChapter.IsNullOrWhiteSpace(), x => x.AtaChapter != null && x.AtaChapter.Contains(input.AtaChapter))
                .WhereIf(!input.Title.IsNullOrWhiteSpace(), x => x.Title != null && x.Title.Contains(input.Title))
                .WhereIf(!input.Description.IsNullOrWhiteSpace(), x => x.Description != null && x.Description.Contains(input.Description))
                .WhereIf(!input.ActionDescription.IsNullOrWhiteSpace(), x => x.ActionDescription != null && x.ActionDescription.Contains(input.ActionDescription))
                .WhereIf(!input.RevisionNote.IsNullOrWhiteSpace(), x => x.RevisionNote != null && x.RevisionNote.Contains(input.RevisionNote))
                .WhereIf(input.Severity.HasValue, x => x.Severity == (Severity)input.Severity.Value)
                .WhereIf(input.DetectedAt.HasValue, x => x.DetectedAt == input.DetectedAt.Value)
                .WhereIf(input.Status.HasValue, x => x.Status == (Status)input.Status.Value)
                .WhereIf(input.CertifyingStaffId.HasValue, x => x.CertifyingStaffId == input.CertifyingStaffId.Value)
                .WhereIf(input.AircraftId.HasValue, x => x.AircraftId == input.AircraftId.Value)
                .WhereIf(input.PersonnelId.HasValue, x => x.PersonnelId == input.PersonnelId.Value);
        }

        public override async Task<SnagReportDto> CreateAsync(CreateSnagReportDto input)
        {
            var result = await base.CreateAsync(input);
            await _flowEngine.TriggerAsync("on-create", "SnagReport", result);

            // Frontend creates records with status pre-set without going through ChangeStatusAsync,
            // so mirror on-field-change here whenever the initial status isn't the default. Otherwise
            // status-driven flows (e.g. approval) never fire on plain Create.
            if (result.Status != (int)Status.Open)
                await _flowEngine.TriggerAsync("on-field-change", "SnagReport", result);
            return result;
        }

        public override async Task<SnagReportDto> UpdateAsync(SnagReportDto input)
        {
            // State machine: validate status transition + log
            var existing = await Repository.GetAsync(input.Id);
            var statusChanged = (int)existing.Status != input.Status;
            if (statusChanged)
            {
                var fromStatus = existing.Status.ToString();
                var toStatus = ((Status)input.Status).ToString();
                ValidateStatusTransition(existing.Status, (Status)input.Status);

                // Log status change
                await _statusChangeLogRepo.InsertAsync(new StatusChangeLog
                {
                    EntityType = "SnagReport",
                    EntityId = input.Id.ToString(),
                    FromStatus = fromStatus,
                    ToStatus = toStatus,
                    Action = "Update",
                    ChangedByUserId = AbpSession.UserId
                });
            }

            var result = await base.UpdateAsync(input);
            await _flowEngine.TriggerAsync("on-update", "SnagReport", result);

            // Frontend updates status via plain UpdateAsync (not ChangeStatusAsync) — fire
            // on-field-change so status-driven flows pick up the transition.
            if (statusChanged)
                await _flowEngine.TriggerAsync("on-field-change", "SnagReport", result);
            return result;
        }

        public override async Task DeleteAsync(EntityDto<long> input)
        {
            await base.DeleteAsync(input);
            await _flowEngine.TriggerAsync("on-delete", "SnagReport", new { Id = input.Id });
        }

        [Abp.Authorization.AbpAuthorize(PermissionNames.SnagReport_Update)]
        public async Task<SnagReportDto> ChangeStatusAsync(long id, ChangeStatusInput input)
        {
            var entity = await Repository.GetAsync(id);
            var currentStatus = entity.Status.ToString();

            // Find valid transition
            var transitions = new (string From, string To, string Action, bool Readonly)[]
            {
            ("Open", "InProgress", "Acknowledge", false),
            ("InProgress", "PendingCRS", "Submit", false),
            ("PendingCRS", "Closed", "Approve", true),
            ("PendingCRS", "InProgress", "Revise", false)
            };

            var transition = transitions.FirstOrDefault(t =>
                (t.From == "*" || t.From == currentStatus) && t.Action == input.Action);

            if (transition == default)
                throw new Abp.UI.UserFriendlyException($"Invalid action '{input.Action}' from status '{currentStatus}'");

            // Validate required fields per transition
            if (input.Action == "Submit" && (input.ActionData == null || !input.ActionData.ContainsKey("actionDescription") || string.IsNullOrWhiteSpace(input.ActionData["actionDescription"]) || !input.ActionData.ContainsKey("certifyingStaffId") || string.IsNullOrWhiteSpace(input.ActionData["certifyingStaffId"])))
                throw new Abp.UI.UserFriendlyException("Submit requires: actionDescription, certifyingStaffId");
            if (input.Action == "Revise" && (input.ActionData == null || !input.ActionData.ContainsKey("revisionNote") || string.IsNullOrWhiteSpace(input.ActionData["revisionNote"])))
                throw new Abp.UI.UserFriendlyException("Revise requires: revisionNote");

            var fromStatus = currentStatus;

            // Apply new status
            entity.Status = (Status)Enum.Parse(typeof(Status), transition.To);
            await Repository.UpdateAsync(entity);
            await CurrentUnitOfWork.SaveChangesAsync();

            // Cancel pending ApprovalRecords when the entity is cancelled — otherwise the records
            // sit forever in approvers' inboxes pointing to a cancelled request.
            if (input.Action == "Cancel")
            {
                var pending = _approvalRepo.GetAll()
                    .Where(a => a.EntityType == "SnagReport" && a.EntityId == id.ToString() && a.Status == "Pending")
                    .ToList();
                foreach (var pendingRec in pending)
                {
                    pendingRec.Status = "Cancelled";
                    pendingRec.ActionTaken = "Cancel";
                    pendingRec.ActionDate = DateTime.UtcNow;
                    pendingRec.Comment = "Entity cancelled by submitter.";
                    await _approvalRepo.UpdateAsync(pendingRec);
                }
            }

            // Log status change
            await _statusChangeLogRepo.InsertAsync(new Entities.StatusChangeLog
            {
                EntityType = "SnagReport",
                EntityId = id.ToString(),
                FromStatus = fromStatus,
                ToStatus = transition.To,
                Action = input.Action,
                Comment = input.ActionData != null && input.ActionData.ContainsKey("comment") ? input.ActionData["comment"] : null,
                ChangedByUserId = AbpSession.UserId
            });

            var result = MapToEntityDto(entity);

            // Trigger flow: on-status-change (always)
            await _flowEngine.TriggerAsync("on-field-change", "SnagReport", result);

            // Trigger named flow events
            if (input.Action == "Submit")
                await _flowEngine.TriggerAsync("submit-for-approval", "SnagReport", result);
            return result;
        }

        private void ValidateStatusTransition(Status from, Status to)
        {
            var allowed = new (string From, string To)[]
            {
                ("Open", "InProgress"),
                ("InProgress", "PendingCRS"),
                ("PendingCRS", "Closed"),
                ("PendingCRS", "InProgress")
            };

            var isValid = allowed.Any(t =>
                (t.From == "*" || t.From == from.ToString()) &&
                t.To == to.ToString());

            if (!isValid)
                throw new Abp.UI.UserFriendlyException($"Invalid status transition from {from} to {to}");
        }
    }
}
