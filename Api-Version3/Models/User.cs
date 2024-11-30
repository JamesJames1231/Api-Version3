using System.Diagnostics.Metrics;
using System.Xml.Linq;

namespace Api_Version3.Models
{
    public class User
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string HashedPassword { get; set; } = "";
        public string Username { get; set; } = "";
        public byte[] Salt { get; set; }
    }

    public class CreateUser
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";

        public bool ReadyStatus()
        {
            return (Name != "" && Email != "" && Username != "" && Password != "") ? true : false;
        }
    }

    public class LoginUser 
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";

        public bool ReadyStatus()
        {
            return (Username != "" && Password != "") ? true : false;
        }
    }


    public class LoggedInUser
    {
        public int ID { get; set; } = -1;
        public string Name { get; set; } = "";
        public string Username { get; set; } = "";
        public string? Password { get; set; } = "";
        public bool ReadyStatus()
        {
            return (ID != -1 && Username != "" && Password != "") ? true : false;
        }
    }

    public class UpdateDetails
    {
        public int ID { get; set; } = -1;
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string NewName { get; set; } = "";
        public string NewEmail { get; set; } = "";
        public string NewPassword { get; set; } = "";

        public bool ReadyStatus()
        {
            return (ID != -1 && Username != "" && Password != "") ? true : false;
        }
        public int UpdateCounter()
        {
            int counter = 0;
            if (NewName != "") { counter++; }
            if (NewEmail != "") { counter++; }
            if (NewPassword != "") { counter++; }
            return counter;
        }
    }
}
