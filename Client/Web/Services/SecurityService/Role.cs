namespace Home.Services.SecurityService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [Serializable]
    public enum Role : byte
    {
        Employee = 1,
        Manager,
        Administrator,
    }
}
