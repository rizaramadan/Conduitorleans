using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conduit.Models.Outputs
{
    public class RegisterUserOutput
    {
        public User user { get; set; }

        public RegisterUserOutput(
            string username, 
            string email, 
            string bio, 
            string image, 
            string token
        ) 
        {
            user = new User 
            { 
                username = username, 
                email    = email, 
                bio      = bio, 
                image    = image, 
                token    = token 
            };
        }
    }

    public class User
    {
        public string username { get; set; }
        public string email { get; set; }
        public string bio { get; set; }
        public string image { get; set; }
        public string token { get; set; }
    }
}
