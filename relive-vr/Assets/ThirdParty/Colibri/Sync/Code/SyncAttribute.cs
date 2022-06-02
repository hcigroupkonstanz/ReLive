using System;

namespace HCIKonstanz.Colibri.Sync
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SyncAttribute : Attribute
    {
        public SyncAttribute()
        {
        }
    }
}
