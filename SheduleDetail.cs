using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apc_Sample
{
    public class ScheduleDetail


    {
        public string SDDT_RUNTIME { get; set; }
        public string SDDT_TITLE { get; set; }
        public string SDDT_SCDYDATE { get; set; }
        public string SDDT_BRDTIME { get; set; }

        public bool Now_Playing { get; set; }

        public TimeSpan actualElapsed { get; set; }
        public int runtimeMilliSec { get; set; }

        public string FilePath { get; set; } = @"C:\Users\kimgu\OneDrive\바탕 화면\AudioServer자료\오디오데이터\audioam\20241201120000_녹음12.wav";
        public DateTime StartTime
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SDDT_BRDTIME) || SDDT_BRDTIME.Length < 6)
                {
                    return DateTime.MinValue;
                }
                return DateTime.Today
                    .AddHours(int.Parse(SDDT_BRDTIME.Substring(0, 2)))
                .AddMinutes(int.Parse(SDDT_BRDTIME.Substring(2, 2)))
                .AddSeconds(int.Parse(SDDT_BRDTIME.Substring(4, 2)));
            }
        }

        //public Stopwatch BroadCatsStarted { get; set; }
    }
}
