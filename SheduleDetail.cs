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
        public string  SDDT_RUNTIME { get; set; }   
        public string SDDT_TITLE { get; set; }
        public string  SDDT_SCDYDATE { get; set; }  
        public  string SDDT_BRDTIME { get; set; }
        
        public bool Now_Playing { get; set; }

       public TimeSpan actualElapsed {  get; set; } 
        public int runtimeMilliSec { get; set; }    
        //public Stopwatch BroadCatsStarted { get; set; }
    }
}
