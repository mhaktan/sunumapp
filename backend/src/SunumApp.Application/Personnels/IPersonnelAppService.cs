using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using SunumApp.Personnels.Dto;

namespace SunumApp.Personnels
{
    public interface IPersonnelAppService : IAsyncCrudAppService<
        PersonnelDto,
        long,
        PagedPersonnelResultRequestDto,
        CreatePersonnelDto,
        PersonnelDto>
    {
    }
}
