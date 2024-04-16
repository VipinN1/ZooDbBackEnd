namespace BackEnd.Models
{
    public class Enclosure
    {
        public int enclosureID { get; set; }
        public string enclosureName { get; set; }
        public string enclosureType { get; set; }
        public DateTime builtDate { get; set; }
        public TimeSpan cleaningScheduleStart { get; set; }
        public TimeSpan cleaningScheduleEnd { get; set; }
    }
}