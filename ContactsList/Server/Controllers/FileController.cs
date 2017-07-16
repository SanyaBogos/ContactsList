using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactsList.Server.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Http;
using ContactsList.Server.Filters.Exceptions;
using Microsoft.Net.Http.Headers;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using ContactsList.Server.ViewModels.FileViewModels;
using System.Reflection;
using ContactsList.Server.Services;

namespace ContactsList.Server.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    public class FileController : BaseController
    {
        private readonly FileFormatService _fileFormatService;

        public FileController(
            UserManager<ApplicationUser> userManager,
            FileFormatService fileFormatService
            )
            : base(userManager)
        {
            _fileFormatService = fileFormatService;
        }


        [HttpPost]
        [SwaggerResponse(200, typeof(ComplexResultViewModel))]
        public ComplexResultViewModel SaveContacts([FromBody]FilesViewModel fileVM)
        {
            // TODO: implement saving to db
            return _fileFormatService.GetContactsFromFile(fileVM);
        }
    }
}
