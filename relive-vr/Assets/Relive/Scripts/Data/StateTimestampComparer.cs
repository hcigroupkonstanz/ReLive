using System.Collections.Generic;
using Relive.Data;

namespace Relive.Data
{
    public class StateTimestampComparer : IComparer<State>
    {
        public int Compare(State x, State y)
        {
            if (x.timestamp == y.timestamp)
            {
                return 0;
            }
            else if (x.timestamp > y.timestamp)
            {
                return 1;
            }
            else
            {
                return -1;
            }

        }
    }
}
