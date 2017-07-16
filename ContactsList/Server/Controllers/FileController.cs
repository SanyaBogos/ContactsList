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
using ContactsList.Server.Repositories;
using System.Linq;
using AutoMapper;

namespace ContactsList.Server.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    public class FileController : BaseController
    {
        private readonly FileFormatService _fileFormatService;
        private readonly ContactRepository _contactRepository;

        public FileController(
            UserManager<ApplicationUser> userManager,
            FileFormatService fileFormatService,
            ContactRepository contactRepository
            )
            : base(userManager)
        {
            _fileFormatService = fileFormatService;
            _contactRepository = contactRepository;
        }

        [HttpGet]
        [SwaggerResponse(200, typeof(ContactViewModel[]))]
        public async Task<ContactViewModel[]> GetContacts()
        {
            var userId = (await GetCurrentUserAsync()).Id;
            var dbElements = await _contactRepository.GetContactsById(userId);
            return Mapper.Map<ContactViewModel[]>(dbElements);
        }


        [HttpPost]
        [SwaggerResponse(200, typeof(ComplexResultViewModel))]
        public async Task<ComplexResultViewModel> SaveContacts([FromBody]FilesViewModel fileVM)
        {
            var userId = (await GetCurrentUserAsync()).Id;
            var dbElements = await _contactRepository.GetContactsById(userId);
            var contactsFromUser = _fileFormatService.GetContactsFromFile(fileVM);

            var successContacts = Mapper.Map<IEnumerable<Contact>>(contactsFromUser.SuccessfullySaved)
                .ToArray();
            for (int i = 0; i < successContacts.Count(); i++)
                successContacts[i].UserId = userId;

            var successContactsCutVer = successContacts.Select(x => new { x.Name, x.Phone });
            var forInsertCutVer = successContactsCutVer.Except(
                dbElements.Select(x => new { x.Name, x.Phone }));
            var forUpdateCutVer = successContactsCutVer.Except(forInsertCutVer);

            var forInsert = successContacts.Where(x => forInsertCutVer.Contains(new { x.Name, x.Phone }))
                .ToArray();
            var forUpdate = successContacts.Where(x => forUpdateCutVer.Contains(new { x.Name, x.Phone }))
                .ToArray();

            await _contactRepository.InsertMany(forInsert);
            await _contactRepository.Update(forUpdate);

            return contactsFromUser;
        }
    }
}
