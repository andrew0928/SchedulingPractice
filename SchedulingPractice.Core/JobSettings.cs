using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulingPractice.Core
{
    public static class JobSettings
    {
        /// <summary>
        /// 最低準備時間 (資料建立 ~ 預計執行時間)
        /// </summary>
        public static readonly TimeSpan MinPrepareTime = TimeSpan.FromSeconds(10);

        /// <summary>
        /// 容許最高延遲執行時間 (預計執行 ~ 實際執行時間)
        /// </summary>
        public static readonly TimeSpan MaxDelayTime = TimeSpan.FromSeconds(30);
    }

}
