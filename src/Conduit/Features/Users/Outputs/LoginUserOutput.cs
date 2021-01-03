using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conduit.Features.Users.Outputs
{
    public class LoginUserOutput : RegisterUserOutput
    {
        public LoginUserOutput(
            string username,
            string email,
            string bio,
            string image,
            string token
        ) : base(username, email, bio, image, token) { }
    }
}
