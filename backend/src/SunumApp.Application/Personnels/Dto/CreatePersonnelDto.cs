using System;
using System.ComponentModel.DataAnnotations;
using Abp.AutoMapper;

namespace SunumApp.Personnels.Dto
{
    [AutoMapTo(typeof(Entities.Personnel))]
    public class CreatePersonnelDto
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(50)]
        public string EmployeeNumber { get; set; }

        public int Role { get; set; }

        [MaxLength(100)]
        public string LicenseNumber { get; set; }

    }
}