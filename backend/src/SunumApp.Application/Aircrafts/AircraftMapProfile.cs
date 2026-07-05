using AutoMapper;
using SunumApp.Entities;
using SunumApp.Aircrafts.Dto;

namespace SunumApp.Aircrafts
{
    public class AircraftMapProfile : Profile
    {
        public AircraftMapProfile()
        {
            CreateMap<Aircraft, AircraftDto>();
            CreateMap<CreateAircraftDto, Aircraft>();
            CreateMap<AircraftDto, Aircraft>();
        }
    }
}
