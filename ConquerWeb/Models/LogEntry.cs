using System;

namespace ConquerWeb.Models
{
    public class LogEntry
    {
        public int Id { get; set; }
        public string LogData { get; set; }
        public DateTime Timestamp { get; set; }
    }
}