using System;
using Abp.Application.Services.Dto;

namespace SunumApp.Personnels.Dto
{
    public class PagedPersonnelResultRequestDto : PagedAndSortedResultRequestDto
    {
        public string Keyword { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmployeeNumber { get; set; }
        public int? Role { get; set; }
        public string LicenseNumber { get; set; }
    }
}
