using System;
using Abp.Application.Services.Dto;

namespace SunumApp.SnagReports.Dto
{
    public class PagedSnagReportResultRequestDto : PagedAndSortedResultRequestDto
    {
        public string Keyword { get; set; }
        public long? AircraftId { get; set; }
        public long? PersonnelId { get; set; }
        public string ReportNumber { get; set; }
        public string AtaChapter { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? Severity { get; set; }
        public DateTime? DetectedAt { get; set; }
        public string ActionDescription { get; set; }
        public string RevisionNote { get; set; }
        public int? Status { get; set; }
        public long? CertifyingStaffId { get; set; }
    }
}
