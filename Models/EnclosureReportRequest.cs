namespace BackEnd.Models
{
    public class EnclosureReportRequest
    {
        // Enclosure Name filter
        public string EnclosureName { get; set; }

        // Enclosure Type filter
        public string EnclosureType { get; set; }

        // Date range filter
        public DateTime? DateRangeStart { get; set; }
        public DateTime? DateRangeEnd { get; set; }

        // Time range filter
        public TimeSpan? TimeRangeStart { get; set; }
        public TimeSpan? TimeRangeEnd { get; set; }
    }
}