using System;

namespace HttpServerDemo
{
    internal class Schedule
    {
        public TimeSpan Interval { get; set; }

        public DayOfWeek Day { get; set; }
        public DateTime StartTime { get; set; }

        public string Minute { get; set; }

        public string Hour { get; set; }

        public DateTime EndTime { get; set; }
    }
}