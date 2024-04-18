namespace BackEnd.Models
{
    public class VetRecords
    {
        public string animalID { get; set; }
        public string animalName { get; set; }
        public string animalSpecies { get; set; }
        public DateTime animalDoB { get; set; }
        public float weight { get; set; }
        public float height { get; set; }
        public string medications { get; set; }
        public string diagnosis { get; set;}

        
    }
}