using BCrypt.Net;

namespace RaceDay_Part2.Helpers
{//start of namespace
    public class PasswordHasher
    {//start of class
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool Verify(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }//end of class
}//end of namespace