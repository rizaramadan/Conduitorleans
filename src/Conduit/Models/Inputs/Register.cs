﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conduit.Models.Inputs
{
    public class RegisterWrapper
    {
        public Register User { get; set; }
    }

    public class Register
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}