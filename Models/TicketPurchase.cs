namespace BackEnd.Models
{
    public class TicketPurchase
    {
        public int adultTickets { get; set; }
        public int childTickets { get; set;}
        public int seniorTickets { get; set; }
        public int infantTickets { get; set; }
        public DateTime formattedDate { get; set; }
        public double totalCost { get; set; }
        public int customerId { get; set; }
    }
}