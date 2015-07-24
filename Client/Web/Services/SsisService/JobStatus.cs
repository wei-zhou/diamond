namespace Home.Services.SsisService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    [Serializable]
    public enum JobStatus : byte
    {
        Running = 1,
        Completed
    }
}