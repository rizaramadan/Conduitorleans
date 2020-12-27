using System;
using System.Collections.Generic;
using System.Text;

namespace Grains.Security
{
    [Serializable]
    public class UserState
    {
        public string Email { get; set; }
        public byte[] Password { get; set; }
        public string Bio { get; set; }
        public Guid Salt { get; set; }
    }
}
