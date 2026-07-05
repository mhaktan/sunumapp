using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace SunumApp.Entities
{
    // State Machine: status — Open → InProgress → PendingCRS → Closed
    // Initial: Open | Transitions: Open→InProgress[Acknowledge], InProgress→PendingCRS[Submit], PendingCRS→Closed[Approve], PendingCRS→InProgress[Revise]
    [Table("SnagReports")]
    public class SnagReport : FullAuditedEntity<long>
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

        public Severity Severity { get; set; }

        public DateTime DetectedAt { get; set; }

        [MaxLength(2000)]
        public string ActionDescription { get; set; }

        [MaxLength(1000)]
        public string RevisionNote { get; set; }

        public Status Status { get; set; }

        public long? CertifyingStaffId { get; set; }

        public long AircraftId { get; set; }

        [ForeignKey(nameof(AircraftId))]
        public virtual Aircraft Aircraft { get; set; }

        public long PersonnelId { get; set; }

        [ForeignKey(nameof(PersonnelId))]
        public virtual Personnel Personnel { get; set; }

    }
}