using System;

namespace SchedulingPractice.Core
{
    public class JobInfo
    {
        public int Id { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime RunAt { get; set; }
        public DateTime? ExecuteAt { get; set; }
        public DateTime? LockAt { get; set; }

        public int State { get; set; }
    }

}
