namespace Contracts.Users
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IUser
    {
        string Email { get; set; }
        byte[] Password { get; set; }
        string Bio { get; set; }
        Guid Salt { get; set; }
    }
}
