using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using SunumApp.Entities;
using SunumApp.Personnels.Dto;
using SunumApp.Authorization;
using SunumApp.Flows;

namespace SunumApp.Personnels
{
    public class PersonnelAppService : AsyncCrudAppService<
        Personnel,
        PersonnelDto,
        long,
        PagedPersonnelResultRequestDto,
        CreatePersonnelDto,
        PersonnelDto>,
        IPersonnelAppService
    {
        private readonly IFlowEngine _flowEngine;

        public PersonnelAppService(IRepository<Personnel, long> repository, IFlowEngine flowEngine)
            : base(repository)
        {
            _flowEngine = flowEngine;
            // Claim-based authorization (JwtPermissionChecker reads JWT "permission" claims)
            GetPermissionName = PermissionNames.Personnel_Read;
            GetAllPermissionName = PermissionNames.Personnel_Read;
            CreatePermissionName = PermissionNames.Personnel_Create;
            UpdatePermissionName = PermissionNames.Personnel_Update;
            DeletePermissionName = PermissionNames.Personnel_Delete;
        }

        protected override IQueryable<Personnel> CreateFilteredQuery(PagedPersonnelResultRequestDto input)
        {
            return Repository.GetAll()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x =>
                    x.Id.ToString().Contains(input.Keyword) ||
                    (x.FirstName != null && x.FirstName.Contains(input.Keyword)) ||
                    (x.LastName != null && x.LastName.Contains(input.Keyword)) ||
                    (x.EmployeeNumber != null && x.EmployeeNumber.Contains(input.Keyword)) ||
                    (x.LicenseNumber != null && x.LicenseNumber.Contains(input.Keyword)))
                .WhereIf(!input.FirstName.IsNullOrWhiteSpace(), x => x.FirstName != null && x.FirstName.Contains(input.FirstName))
                .WhereIf(!input.LastName.IsNullOrWhiteSpace(), x => x.LastName != null && x.LastName.Contains(input.LastName))
                .WhereIf(!input.EmployeeNumber.IsNullOrWhiteSpace(), x => x.EmployeeNumber != null && x.EmployeeNumber.Contains(input.EmployeeNumber))
                .WhereIf(!input.LicenseNumber.IsNullOrWhiteSpace(), x => x.LicenseNumber != null && x.LicenseNumber.Contains(input.LicenseNumber))
                .WhereIf(input.Role.HasValue, x => x.Role == (Role)input.Role.Value);
        }

        public override async Task<PersonnelDto> CreateAsync(CreatePersonnelDto input)
        {
            var result = await base.CreateAsync(input);
            await _flowEngine.TriggerAsync("on-create", "Personnel", result);
            return result;
        }

        public override async Task<PersonnelDto> UpdateAsync(PersonnelDto input)
        {
            var result = await base.UpdateAsync(input);
            await _flowEngine.TriggerAsync("on-update", "Personnel", result);
            return result;
        }

        public override async Task DeleteAsync(EntityDto<long> input)
        {
            await base.DeleteAsync(input);
            await _flowEngine.TriggerAsync("on-delete", "Personnel", new { Id = input.Id });
        }
    }
}
