using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulingPractice.Core
{
    public enum JobStateEnum : int
    {
        CREATE = 0,
        LOCK = 1,
        COMPLETE = 2
    }
}
