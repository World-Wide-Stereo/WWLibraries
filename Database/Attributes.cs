using System;

namespace ww.Tables
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EqualityComparisonAttribute : Attribute { }
}
