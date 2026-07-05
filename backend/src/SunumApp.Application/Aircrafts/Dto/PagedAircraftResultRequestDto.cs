using System;
using Abp.Application.Services.Dto;

namespace SunumApp.Aircrafts.Dto
{
    public class PagedAircraftResultRequestDto : PagedAndSortedResultRequestDto
    {
        public string Keyword { get; set; }
        public string Registration { get; set; }
        public string AircraftType { get; set; }
        public string Model { get; set; }
        public int? Status { get; set; }
    }
}
