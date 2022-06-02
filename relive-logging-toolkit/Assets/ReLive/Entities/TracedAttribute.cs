using System;

namespace ReLive.Entities
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TracedAttribute : Attribute
    {
        public TracedAttribute()
        {
        }
    }
}
