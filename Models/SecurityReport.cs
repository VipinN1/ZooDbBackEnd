namespace BackEnd.Models
{
    public class SecurityReport
    {
        public int logID { get; set; }
        public int empID { get; set; }
        public int empId { get; set; }
        public DateTime date { get; set; }
        public TimeSpan time { get; set; }
        public string eventDescription { get; set; }
        public string location { get; set; }
        public string severityLevel { get; set; }
    }
}