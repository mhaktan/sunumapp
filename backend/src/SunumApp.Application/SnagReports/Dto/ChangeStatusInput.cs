using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SunumApp.SnagReports.Dto
{
    public class ChangeStatusInput
    {
        [Required]
        public string Action { get; set; }

        /// <summary>
        /// Additional data required by some transitions (e.g. revision reason).
        /// </summary>
        public Dictionary<string, string> ActionData { get; set; }
    }
}
