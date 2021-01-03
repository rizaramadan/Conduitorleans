using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conduit.Features.Users.Outputs
{
    public class RegisterUserOutput
    {
        public User User { get; set; }

        public RegisterUserOutput(
            string username, 
            string email, 
            string bio, 
            string image, 
            string token
        ) 
        {
            User = new User 
            { 
                Username = username, 
                Email    = email, 
                Bio      = bio, 
                Image    = image, 
                Token    = token 
            };
        }
    }

    public class User
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }
        public string Token { get; set; }
    }
}
