namespace Contracts.Users
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class User
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public byte[] Password { get; set; }
        public string Bio { get; set; }
        public Guid Salt { get; set; }
        public string Image { get; set; }

    }
}
