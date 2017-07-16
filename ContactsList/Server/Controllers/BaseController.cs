using ContactsList.Server.Entities;
using ContactsList.Server.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsList.Server.Controllers
{
    [Authorize]
    [ServiceFilter(typeof(ApiExceptionFilter))]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class BaseController : Controller
    {
        protected readonly UserManager<ApplicationUser> _userManager;

        public BaseController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        protected Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);
    }
}
