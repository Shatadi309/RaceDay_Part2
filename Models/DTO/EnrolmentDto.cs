namespace RaceDay_Part2.Models.DTO
{//start of namespace
    public class EnrolmentDto
    {//start of class
            public int EventCategoryID { get; set; }
            public decimal PaymentAmount { get; set; }
            public string PaymentStatus { get; set; } = "Pending";
        }//end of class
    }//end of namespace

