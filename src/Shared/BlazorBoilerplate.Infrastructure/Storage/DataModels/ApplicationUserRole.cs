﻿using BlazorBoilerplate.Infrastructure.AuthorizationDefinitions;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Identity;
using System;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    [MultiTenant]
    [Permissions(Actions.CRUD)]
    public class ApplicationUserRole : IdentityUserRole<Guid>
    {
        public virtual ApplicationUser User { get; set; }
        public virtual ApplicationRole Role { get; set; }
    }
}