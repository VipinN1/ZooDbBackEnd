namespace BackEnd.Models
{
    public class FilterModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string TransactionType { get; set; } // "all", "ticket", "donation"
    }
}