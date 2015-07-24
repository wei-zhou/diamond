namespace Home.Mvc.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    [Serializable]
    public enum ProductFileUploadType : byte
    {
        Append = 1,
        Replace,
        Update
    }
}