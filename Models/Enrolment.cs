namespace RaceDay_Part2.Models
{//start of namespace
    public class Enrolment
    {//start of class

     
        public int EnrolmentID { get; set; }
        public int UserID { get; set; }
        public int EventCategoryID { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal PaymentAmount { get; set; }
    }//end of class
}//end of namespace

