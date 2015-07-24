namespace Home.Services.SsisService
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web;

    public class JobParameter
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public JobParameterType Type { get; set; }

        [Required]
        public string Value { get; set; }
    }
}