using System.Data.SqlClient;
namespace RaceDay_Part2.Models
{//start of namespace
    public class Event
    {//start of class
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string? EventDescription { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal EntryFee { get; set; }
        public int UserID { get; set; }
    }//end of class


}//end of namespace
