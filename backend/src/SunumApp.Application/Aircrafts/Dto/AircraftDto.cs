using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace SunumApp.Aircrafts.Dto
{
    [AutoMapFrom(typeof(Entities.Aircraft))]
    public class AircraftDto : EntityDto<long>
    {
        public string Registration { get; set; }

        public string AircraftType { get; set; }

        public string Model { get; set; }

        public int Status { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime? LastModificationTime { get; set; }

    }
}