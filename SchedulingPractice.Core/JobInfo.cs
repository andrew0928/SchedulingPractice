using System;

namespace SchedulingPractice.Core
{
    public class JobInfo
    {
        public int Id { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime RunAt { get; set; }
        public DateTime? ExecuteAt { get; set; }

        public int State { get; set; }
    }

    public enum JobStateEnum : int
    {
        CREATE = 0,
        LOCK = 1,
        COMPLETE = 2
    }

    public static class JobSettings
    {
        /// <summary>
        /// 最低準備時間 (資料建立 ~ 預計執行時間)
        /// </summary>
        public static TimeSpan MinPrepareTime = TimeSpan.FromSeconds(10);

        /// <summary>
        /// 容許最高延遲執行時間 (預計執行 ~ 實際執行時間)
        /// </summary>
        public static TimeSpan MaxDelayTime = TimeSpan.FromSeconds(30);
    }
}
