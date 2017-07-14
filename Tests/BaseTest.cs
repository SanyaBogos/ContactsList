using ContactsList.Server;
using ContactsList.Server.Entities;
using ContactsList.Server.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsListTest
{
    public class BaseTest<T> where T : class
    {
        protected T _sut;
        protected UserManager<ApplicationUser> _userManager;

        public BaseTest()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            var userStore = new Mock<IUserStore<ApplicationUser>>();
            _userManager = new UserManager<ApplicationUser>(userStore.Object, 
                null, null, null, null, 
                null, null, null, null);
        }
    }
}
