using System;
using System.ComponentModel.DataAnnotations;
using Abp.AutoMapper;

namespace SunumApp.SnagReports.Dto
{
    [AutoMapTo(typeof(Entities.SnagReport))]
    public class CreateSnagReportDto
    {
        [Required]
        [MaxLength(50)]
        public string ReportNumber { get; set; }

        [Required]
        [MaxLength(20)]
        public string AtaChapter { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; }

        public int Severity { get; set; }

        public DateTime DetectedAt { get; set; }

        [MaxLength(2000)]
        public string ActionDescription { get; set; }

        [MaxLength(1000)]
        public string RevisionNote { get; set; }

        public int Status { get; set; }

        public long? CertifyingStaffId { get; set; }

        public long AircraftId { get; set; }

        public long PersonnelId { get; set; }

    }
}