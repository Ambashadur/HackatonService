using System;

namespace HackatonService.Domain
{
    public class ProcessEntity
    {
        public long Id { get; set; }
        
        public string Name { get; set; }
        
        public string MachineName { get; set; }
        
        public DateTime StartTime { get; set; }
        
        public DateTime ExitTime { get; set; }
        
        public string MainWindowTitle { get; set; }
        
        public TimeSpan TotalProcessorTime { get; set; }
    }
}