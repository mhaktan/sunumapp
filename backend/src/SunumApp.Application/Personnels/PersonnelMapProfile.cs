using AutoMapper;
using SunumApp.Entities;
using SunumApp.Personnels.Dto;

namespace SunumApp.Personnels
{
    public class PersonnelMapProfile : Profile
    {
        public PersonnelMapProfile()
        {
            CreateMap<Personnel, PersonnelDto>();
            CreateMap<CreatePersonnelDto, Personnel>();
            CreateMap<PersonnelDto, Personnel>();
        }
    }
}
