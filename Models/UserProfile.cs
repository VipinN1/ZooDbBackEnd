namespace BackEnd.Models
{
    public class UserProfile
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }

        public string address { get; set; }

        public string zipCode { get; set; }

        public DateTime formattedDate { get; set; }
    }
}