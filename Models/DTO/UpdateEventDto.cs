namespace RaceDay_Part2.Models.DTO
{
    public class UpdateEventDto
    {


            public string EventName { get; set; } = string.Empty;
            public string? EventDescription { get; set; }
            public DateTime EventDate { get; set; }
            public string Location { get; set; } = string.Empty;
            public decimal EntryFee { get; set; }
        }
    }

