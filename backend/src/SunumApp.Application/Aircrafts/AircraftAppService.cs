using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using SunumApp.Entities;
using SunumApp.Aircrafts.Dto;
using SunumApp.Authorization;
using SunumApp.Flows;

namespace SunumApp.Aircrafts
{
    public class AircraftAppService : AsyncCrudAppService<
        Aircraft,
        AircraftDto,
        long,
        PagedAircraftResultRequestDto,
        CreateAircraftDto,
        AircraftDto>,
        IAircraftAppService
    {
        private readonly IFlowEngine _flowEngine;

        public AircraftAppService(IRepository<Aircraft, long> repository, IFlowEngine flowEngine)
            : base(repository)
        {
            _flowEngine = flowEngine;
            // Claim-based authorization (JwtPermissionChecker reads JWT "permission" claims)
            GetPermissionName = PermissionNames.Aircraft_Read;
            GetAllPermissionName = PermissionNames.Aircraft_Read;
            CreatePermissionName = PermissionNames.Aircraft_Create;
            UpdatePermissionName = PermissionNames.Aircraft_Update;
            DeletePermissionName = PermissionNames.Aircraft_Delete;
        }

        protected override IQueryable<Aircraft> CreateFilteredQuery(PagedAircraftResultRequestDto input)
        {
            return Repository.GetAll()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.Id.ToString().Contains(input.Keyword) ||
                    (x.Registration != null && x.Registration.Contains(input.Keyword)) ||
                    (x.AircraftType != null && x.AircraftType.Contains(input.Keyword)) ||
                    (x.Model != null && x.Model.Contains(input.Keyword)))
                .WhereIf(!input.Registration.IsNullOrWhiteSpace(), x => x.Registration != null && x.Registration.Contains(input.Registration))
                .WhereIf(!input.AircraftType.IsNullOrWhiteSpace(), x => x.AircraftType != null && x.AircraftType.Contains(input.AircraftType))
                .WhereIf(!input.Model.IsNullOrWhiteSpace(), x => x.Model != null && x.Model.Contains(input.Model))
                .WhereIf(input.Status.HasValue, x => x.Status == (Status)input.Status.Value);
        }

        public override async Task<AircraftDto> CreateAsync(CreateAircraftDto input)
        {
            var result = await base.CreateAsync(input);
            await _flowEngine.TriggerAsync("on-create", "Aircraft", result);
            return result;
        }

        public override async Task<AircraftDto> UpdateAsync(AircraftDto input)
        {
            var result = await base.UpdateAsync(input);
            await _flowEngine.TriggerAsync("on-update", "Aircraft", result);
            return result;
        }

        public override async Task DeleteAsync(EntityDto<long> input)
        {
            await base.DeleteAsync(input);
            await _flowEngine.TriggerAsync("on-delete", "Aircraft", new { Id = input.Id });
        }
    }
}
