using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Net.Mail;
using Abp.Runtime.Session;
using Abp.UI;
using SunumApp.Approvals.Dto;
using SunumApp.Entities;
using SunumApp.Flows;

namespace SunumApp.Approvals
{
    public interface IApprovalAppService : IApplicationService
    {
        Task SubmitForApprovalAsync(SubmitApprovalInput input);
        Task<ApprovalRecordDto> ProcessApprovalAsync(ProcessApprovalInput input);
        Task<List<ApprovalRecordDto>> GetApprovalHistoryAsync(string entityType, string entityId);
        Task<List<StatusChangeLogDto>> GetStatusChangeLogsAsync(string entityType, string entityId);
        Task<List<PendingApprovalDto>> GetMyPendingApprovalsAsync();
    }

    public class ApprovalAppService : ApplicationService, IApprovalAppService
    {
        private readonly IRepository<ApprovalRecord, Guid> _approvalRepo;
        private readonly IRepository<StatusChangeLog, long> _statusChangeLogRepo;
        private readonly IRepository<AppUser, long> _userRepo;
        private readonly IRepository<AppRole, long> _roleRepo;
        private readonly IRepository<UserRole, long> _userRoleRepo;
        private readonly IEmailSender _emailSender;
        private readonly IFlowEngine _flowEngine;
        private readonly IIocResolver _iocResolver;

        public ApprovalAppService(
            IRepository<ApprovalRecord, Guid> approvalRepo,
            IRepository<StatusChangeLog, long> statusChangeLogRepo,
            IRepository<AppUser, long> userRepo,
            IRepository<AppRole, long> roleRepo,
            IRepository<UserRole, long> userRoleRepo,
            IEmailSender emailSender,
            IFlowEngine flowEngine,
            IIocResolver iocResolver)
        {
            _approvalRepo = approvalRepo;
            _statusChangeLogRepo = statusChangeLogRepo;
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _userRoleRepo = userRoleRepo;
            _emailSender = emailSender;
            _flowEngine = flowEngine;
            _iocResolver = iocResolver;
        }

        // Helper: returns user ids that belong to the named role (case-insensitive).
        private List<long> GetUserIdsInRole(string roleName)
        {
            if (string.IsNullOrEmpty(roleName)) return new List<long>();
            var role = _roleRepo.GetAll().FirstOrDefault(r => r.Name.ToLower() == roleName.ToLower());
            if (role == null) return new List<long>();
            return _userRoleRepo.GetAll().Where(ur => ur.RoleId == role.Id).Select(ur => ur.UserId).ToList();
        }

        // Helper: role names assigned to a user (used to expand "My Tasks" with role-broadcast records).
        private List<string> GetUserRoleNames(long userId)
        {
            return _userRoleRepo.GetAll()
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.Name)
                .Where(n => n != null)
                .ToList();
        }

        public async Task SubmitForApprovalAsync(SubmitApprovalInput input)
        {
            // Cancel any existing pending approvals for this entity
            var existing = _approvalRepo.GetAll()
                .Where(a => a.EntityType == input.EntityType && a.EntityId == input.EntityId && a.Status == "Pending")
                .ToList();
            foreach (var record in existing)
            {
                record.Status = "Cancelled";
                await _approvalRepo.UpdateAsync(record);
            }

            // Exactly one of AssigneeUserId / AssigneeRole must be set
            var hasUser = input.AssigneeUserId.HasValue && input.AssigneeUserId.Value > 0;
            var hasRole = !string.IsNullOrWhiteSpace(input.AssigneeRole);
            if (!hasUser && !hasRole)
                throw new UserFriendlyException("Approval submission requires either AssigneeUserId or AssigneeRole.");

            // Create first approval step
            var approvalRecord = new ApprovalRecord
            {
                EntityType = input.EntityType,
                EntityId = input.EntityId,
                FlowId = input.FlowId,
                NodeId = input.NodeId,
                StepIndex = 0,
                StepName = string.IsNullOrEmpty(input.StepName) ? "Step 1" : input.StepName,
                AssigneeUserId = hasUser ? input.AssigneeUserId : null,
                AssigneeRole = hasRole ? input.AssigneeRole : null,
                Status = "Pending"
            };

            await _approvalRepo.InsertAsync(approvalRecord);

            // Send notification email — failure here MUST NOT block record creation.
            // Role-based assignment fans the email out to every member of the role.
            if (!string.IsNullOrEmpty(input.EmailSubject))
            {
                var recipients = new List<AppUser>();
                try
                {
                    if (hasUser)
                    {
                        var u = await _userRepo.FirstOrDefaultAsync(input.AssigneeUserId.Value);
                        if (u != null) recipients.Add(u);
                    }
                    else if (hasRole)
                    {
                        var userIds = GetUserIdsInRole(input.AssigneeRole);
                        if (userIds.Count > 0)
                            recipients = _userRepo.GetAll().Where(u => userIds.Contains(u.Id)).ToList();
                    }

                    var body = string.IsNullOrEmpty(input.EmailBody)
                        ? $"<p>You have a pending approval: {input.EntityType} #{input.EntityId}</p>"
                        : input.EmailBody;

                    foreach (var r in recipients)
                    {
                        if (!string.IsNullOrEmpty(r.EmailAddress))
                            await _emailSender.SendAsync(r.EmailAddress, input.EmailSubject, body, true);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Approval notification email failed: {ex.Message}");
                }
            }
        }

        public async Task<ApprovalRecordDto> ProcessApprovalAsync(ProcessApprovalInput input)
        {
            var record = await _approvalRepo.GetAsync(input.ApprovalRecordId);

            if (record.Status != "Pending")
                throw new UserFriendlyException("This approval has already been processed.");

            // Authorization: caller must be the specifically assigned user OR a member of the
            // assigned role. Broadcast-style role assignments are first-to-act.
            var currentUserId = AbpSession.GetUserId();
            var canProcess = (record.AssigneeUserId.HasValue && record.AssigneeUserId.Value == currentUserId)
                             || (!string.IsNullOrEmpty(record.AssigneeRole)
                                 && GetUserRoleNames(currentUserId).Any(r => r.Equals(record.AssigneeRole, StringComparison.OrdinalIgnoreCase)));
            if (!canProcess)
                throw new UserFriendlyException("You are not assigned to this approval step.");

            record.ActionTaken = input.Action;
            record.Comment = input.Comment;
            record.ActionDate = DateTime.UtcNow;

            // Look up the approval node config so we can drive next-step + assignee from the flow
            // definition rather than relying on the client to send NextAssigneeUserId.
            var flow = _flowEngine.GetFlowById(record.FlowId);
            var node = flow?.Nodes?.FirstOrDefault(n => n.Id == record.NodeId);
            var steps = node?.Approval?.Steps ?? new List<FlowApprovalStep>();

            if (input.Action == "Approve")
            {
                var nextIdx = record.StepIndex + 1;
                var hasNextStep = nextIdx < steps.Count;

                if (hasNextStep)
                {
                    var nextStep = steps[nextIdx];
                    var (nextUserId, nextRole) = await ResolveAssigneeFromEntityAsync(record.EntityType, record.EntityId, nextStep);
                    if (nextUserId <= 0 && string.IsNullOrEmpty(nextRole))
                        throw new UserFriendlyException(
                            $"Next step '{nextStep.Name}' assignee could not be resolved (assigneeType={nextStep.AssigneeType}, value={nextStep.AssigneeValue}).");

                    record.Status = "Approved";
                    record.NextAssigneeUserId = nextUserId > 0 ? (long?)nextUserId : null;
                    await _approvalRepo.UpdateAsync(record);

                    await _approvalRepo.InsertAsync(new ApprovalRecord
                    {
                        EntityType = record.EntityType,
                        EntityId = record.EntityId,
                        FlowId = record.FlowId,
                        NodeId = record.NodeId,
                        StepIndex = nextIdx,
                        StepName = nextStep.Name ?? $"Step {nextIdx + 1}",
                        AssigneeUserId = nextUserId > 0 ? (long?)nextUserId : null,
                        AssigneeRole = !string.IsNullOrEmpty(nextRole) ? nextRole : null,
                        Status = "Pending"
                    });
                }
                else
                {
                    record.Status = "Approved";
                    await _approvalRepo.UpdateAsync(record);
                }

                // Drive the entity's state machine forward — its ChangeStatusAsync owns the transition map.
                await TryChangeEntityStatusAsync(record.EntityType, record.EntityId, "Approve", input.Comment);
            }
            else if (input.Action == "Revise")
            {
                record.Status = "Revised";
                await _approvalRepo.UpdateAsync(record);

                // Sends the entity back to its creator (revisionAssignee="creator" on the flow node) by
                // moving the entity status to Revision; the entity list filtered by Revision becomes the
                // creator's revise inbox until they resubmit.
                await TryChangeEntityStatusAsync(record.EntityType, record.EntityId, "Revise", input.Comment);
            }

            return ObjectMapper.Map<ApprovalRecordDto>(record);
        }

        // Resolve the assignee for an approval step. Returns (userId, roleName).
        //   - assigneeType="fixed": numeric user id baked into the flow definition
        //   - assigneeType="role":  role name; broadcast to all members
        //   - assigneeType="field": look up the named property on the entity. The value may be
        //                           numeric (user id) or a string (role name) — auto-detected.
        private async Task<(long userId, string roleName)> ResolveAssigneeFromEntityAsync(string entityType, string entityIdStr, FlowApprovalStep step)
        {
            if (step == null) return (0, null);
            if (step.AssigneeType == "fixed" && long.TryParse(step.AssigneeValue, out var fixedId))
                return (fixedId, null);
            if (step.AssigneeType == "role" && !string.IsNullOrWhiteSpace(step.AssigneeValue))
                return (0, step.AssigneeValue);
            if (step.AssigneeType != "field" || string.IsNullOrWhiteSpace(step.AssigneeValue))
                return (0, null);
            if (!long.TryParse(entityIdStr, out var entityId)) return (0, null);

            var entityClrType = ResolveEntityClrType(entityType);
            if (entityClrType == null) return (0, null);

            // IRepository<TEntity, long> via reflection — keeps ApprovalAppService entity-agnostic.
            var repoIface = typeof(IRepository<,>).MakeGenericType(entityClrType, typeof(long));
            using (var disposable = _iocResolver.ResolveAsDisposable(repoIface))
            {
                var repo = disposable.Object;
                var firstOrDefault = repo.GetType().GetMethod("FirstOrDefaultAsync", new[] { typeof(long) });
                if (firstOrDefault == null) return (0, null);
                var task = (Task)firstOrDefault.Invoke(repo, new object[] { entityId });
                await task.ConfigureAwait(false);
                var entity = task.GetType().GetProperty("Result")?.GetValue(task);
                if (entity == null) return (0, null);

                var prop = entity.GetType().GetProperties()
                    .FirstOrDefault(p => string.Equals(p.Name, step.AssigneeValue, StringComparison.OrdinalIgnoreCase));
                var raw = prop?.GetValue(entity);
                if (raw == null) return (0, null);
                var s = raw.ToString();
                if (long.TryParse(s, out var assignee)) return (assignee, null);
                if (!string.IsNullOrWhiteSpace(s)) return (0, s);
                return (0, null);
            }
        }

        // Call I{EntityType}AppService.ChangeStatusAsync(id, ChangeStatusInput) by reflection.
        // Approval-driven transitions piggy-back on the entity's existing transition map so we don't
        // duplicate state-machine logic in two places.
        private async Task TryChangeEntityStatusAsync(string entityType, string entityIdStr, string action, string comment)
        {
            if (!long.TryParse(entityIdStr, out var entityId)) return;
            try
            {
                var rootNs = GetType().Namespace?.Split('.')[0] ?? "SunumApp";
                var pluralNs = $"{rootNs}.{entityType}s";
                var ifaceTypeName = $"{pluralNs}.I{entityType}AppService, {rootNs}.Application";
                var iface = Type.GetType(ifaceTypeName);
                if (iface == null) { Logger.Warn($"No app service interface for entity '{entityType}' (looked for {ifaceTypeName})"); return; }

                var method = iface.GetMethod("ChangeStatusAsync");
                if (method == null) { Logger.Warn($"{iface.Name} has no ChangeStatusAsync method; skipping status update."); return; }

                var changeStatusInputType = Type.GetType($"{pluralNs}.Dto.ChangeStatusInput, {rootNs}.Application");
                if (changeStatusInputType == null) { Logger.Warn($"ChangeStatusInput type missing for '{entityType}'"); return; }

                var changeInput = Activator.CreateInstance(changeStatusInputType);
                changeStatusInputType.GetProperty("Action")?.SetValue(changeInput, action);
                var actionData = new Dictionary<string, string>
                {
                    ["comment"] = comment ?? string.Empty,
                    ["revisionNote"] = comment ?? string.Empty,
                };
                changeStatusInputType.GetProperty("ActionData")?.SetValue(changeInput, actionData);

                using (var disposable = _iocResolver.ResolveAsDisposable(iface))
                {
                    var task = (Task)method.Invoke(disposable.Object, new[] { entityId, changeInput });
                    await task.ConfigureAwait(false);
                }
            }
            catch (TargetInvocationException tie)
            {
                Logger.Error($"ChangeStatusAsync invoke failed for {entityType}#{entityIdStr}: {tie.InnerException?.Message}", tie.InnerException);
                if (tie.InnerException is UserFriendlyException ufe) throw ufe;
                throw new UserFriendlyException("Status update failed: " + (tie.InnerException?.Message ?? tie.Message));
            }
        }

        private Type ResolveEntityClrType(string entityType)
        {
            var rootNs = GetType().Namespace?.Split('.')[0] ?? "SunumApp";
            return Type.GetType($"{rootNs}.Entities.{entityType}, {rootNs}.Core");
        }

        public async Task<List<ApprovalRecordDto>> GetApprovalHistoryAsync(string entityType, string entityId)
        {
            var records = _approvalRepo.GetAll()
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .OrderBy(a => a.CreationTime)
                .ToList();

            return ObjectMapper.Map<List<ApprovalRecordDto>>(records);
        }

        public async Task<List<StatusChangeLogDto>> GetStatusChangeLogsAsync(string entityType, string entityId)
        {
            var records = _statusChangeLogRepo.GetAll()
                .Where(r => r.EntityType == entityType && r.EntityId == entityId)
                .OrderBy(r => r.CreationTime)
                .ToList();

            return ObjectMapper.Map<List<StatusChangeLogDto>>(records);
        }

        public async Task<List<PendingApprovalDto>> GetMyPendingApprovalsAsync()
        {
            var userId = AbpSession.GetUserId();
            var userRoles = GetUserRoleNames(userId);
            // Pending = user is the named assignee OR the record is broadcast to a role this user belongs to.
            var records = _approvalRepo.GetAll()
                .Where(a => a.Status == "Pending"
                            && ((a.AssigneeUserId.HasValue && a.AssigneeUserId.Value == userId)
                                || (a.AssigneeRole != null && userRoles.Contains(a.AssigneeRole))))
                .OrderByDescending(a => a.CreationTime)
                .ToList();

            return records.Select(r => new PendingApprovalDto
            {
                ApprovalRecordId = r.Id,
                EntityType = r.EntityType,
                EntityId = r.EntityId,
                StepName = r.StepName,
                CreationTime = r.CreationTime,
                AvailableActions = new List<string> { "Approve", "Revise" }
            }).ToList();
        }
    }
}
