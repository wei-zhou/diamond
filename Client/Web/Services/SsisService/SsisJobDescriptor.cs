namespace Home.Services.SsisService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public class SsisJobDescriptor
    {
        public string JobName { get; set; }
        public string EnvironmentName { get; set; }
        public IEnumerable<string> PackageNames { get; set; }
    }
}