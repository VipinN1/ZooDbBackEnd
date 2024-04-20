namespace BackEnd.Models
{
    public class EmployeeProfile
    {
            public string lastName { get; set; }
            public string firstName { get; set; }
            public DateTime hireDate { get; set; }
            public DateTime dob { get; set; }
            public decimal salary { get; set; }
            public string email { get; set; }
    }
}