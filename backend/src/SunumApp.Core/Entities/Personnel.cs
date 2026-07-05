using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace SunumApp.Entities
{
    [Table("Personnels")]
    public class Personnel : FullAuditedEntity<long>
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

        public Role Role { get; set; }

        [MaxLength(100)]
        public string LicenseNumber { get; set; }

        public virtual ICollection<SnagReport> SnagReports { get; set; }

    }
}