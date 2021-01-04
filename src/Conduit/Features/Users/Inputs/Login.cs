﻿using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conduit.Features.Users.Inputs
{
    public class LoginWrapper
    {
        public Login User { get; set; }
    }

    public class Login
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginValidator : AbstractValidator<LoginWrapper> 
    {
        public LoginValidator() 
        {
            RuleFor(lw => lw.User).NotNull();
            RuleFor(lw => lw.User.Email).NotEmpty();
            RuleFor(lw => lw.User.Password).NotEmpty();
        }
    }
}