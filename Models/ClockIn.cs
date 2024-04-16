namespace BackEnd.Models
{
    public class ClockIn
    {
        public int empId { get; set; }
        public DateTime clockIn { get; set; }
        public DateTime clockOut { get; set; }
        public float totalHours { get; set; }
        
    }
}