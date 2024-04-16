namespace BackEnd.Models
{
    public class AnimalReportData
    {
        public int animalID{ get; set; }
        public int enclosureID { get; set; }
        public string name { get; set; }
        public string species { get; set; }
        public char gender { get; set; }
        public DateTime DoB { get; set; }
        public bool endangered { get; set; }
        public DateTime DoA { get; set; }
        public string origin { get; set; }
        public string LifeStage { get; set; }


        // Add other properties as needed
    }
}