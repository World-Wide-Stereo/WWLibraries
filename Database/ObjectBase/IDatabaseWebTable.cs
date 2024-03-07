using System;
using System.Collections.Generic;
using System.Reflection;

namespace ww.Tables
{
    public interface IDatabaseWebTable
    {
        Exception DeleteFromWeb(bool storeErrors = true);
        Exception UpdateWeb(bool storeErrors = true);

        IEnumerable<MemberInfo> GetKeyMembers(bool useAllKeys = false);
    }
}
