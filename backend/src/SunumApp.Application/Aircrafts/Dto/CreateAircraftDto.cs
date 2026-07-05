using System;
using System.ComponentModel.DataAnnotations;
using Abp.AutoMapper;

namespace SunumApp.Aircrafts.Dto
{
    [AutoMapTo(typeof(Entities.Aircraft))]
    public class CreateAircraftDto
    {
        [Required]
        [MaxLength(20)]
        public string Registration { get; set; }

        [Required]
        [MaxLength(100)]
        public string AircraftType { get; set; }

        [Required]
        [MaxLength(100)]
        public string Model { get; set; }

        public int Status { get; set; }

    }
}