namespace Home.Services.SsisService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    [Serializable]
    public enum JobParameterType : byte
    {
        Boolean = 1,
        Byte,
        DateTime,
        Decimal,
        Double,
        Int16,
        Int32,
        Int64,
        SByte,
        Single,
        String,
        UInt32,
        UInt64
    }
}