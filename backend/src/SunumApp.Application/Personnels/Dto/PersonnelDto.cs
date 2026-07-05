using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace SunumApp.Personnels.Dto
{
    [AutoMapFrom(typeof(Entities.Personnel))]
    public class PersonnelDto : EntityDto<long>
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string EmployeeNumber { get; set; }

        public int Role { get; set; }

        public string LicenseNumber { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime? LastModificationTime { get; set; }

    }
}