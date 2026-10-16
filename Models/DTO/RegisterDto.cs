namespace RaceDay_Part2.Models.DTO
{//start of namespace
    public class RegisterDto
    {//start of class
    
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = string.Empty; // Organizer | Participant
    }//end of class 
}//end of namespace

