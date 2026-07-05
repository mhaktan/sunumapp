using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace SunumApp.Entities
{
    [Table("Aircrafts")]
    public class Aircraft : FullAuditedEntity<long>
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

        public Status Status { get; set; }

        public virtual ICollection<SnagReport> SnagReports { get; set; }

    }
}