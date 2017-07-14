using System;
using System.Collections.Generic;
using System.Linq;
using ContactsList.Server.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using CsvHelper;
using System.IO;

namespace ContactsList.Server
{
    public class SeedDbData
    {
        readonly ApplicationDbContext _context;
        private readonly IHostingEnvironment _hostingEnv;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public SeedDbData(IWebHost host, ApplicationDbContext context)
        {
            var services = (IServiceScopeFactory)host.Services.GetService(typeof(IServiceScopeFactory));
            var serviceScope = services.CreateScope();
            _hostingEnv = serviceScope.ServiceProvider.GetService<IHostingEnvironment>();
            _roleManager = serviceScope.ServiceProvider.GetService<RoleManager<ApplicationRole>>();
            _userManager = serviceScope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
            _context = context;
            CreateRoles(); // Add roles
            CreateUsers(); // Add users
            AddLanguagesAndContent();
        }

        private void CreateRoles()
        {
            var rolesToAdd = new List<ApplicationRole>(){
                new ApplicationRole { Name= "Admin", Description = "Full rights role"},
                new ApplicationRole { Name= "User", Description = "Limited rights role"}
            };
            foreach (var role in rolesToAdd)
            {
                if (!_roleManager.RoleExistsAsync(role.Name).Result)
                {
                    _roleManager.CreateAsync(role).Result.ToString();
                }
            }
        }
        private void CreateUsers()
        {
            if (!_context.ApplicationUsers.Any())
            {

                _userManager.CreateAsync(new ApplicationUser { UserName = "admin@admin.com", FirstName = "Admin first", LastName = "Admin last", Email = "admin@admin.com", EmailConfirmed = true, CreatedDate = DateTime.Now, IsEnabled = true }, "P@ssw0rd!").Result.ToString();
                _userManager.AddToRoleAsync(_userManager.FindByNameAsync("admin@admin.com").GetAwaiter().GetResult(), "Admin").Result.ToString();

                _userManager.CreateAsync(new ApplicationUser { UserName = "user@user.com", FirstName = "First", LastName = "Last", Email = "user@user.com", EmailConfirmed = true, CreatedDate = DateTime.Now, IsEnabled = true }, "P@ssw0rd!").Result.ToString();
                _userManager.AddToRoleAsync(_userManager.FindByNameAsync("user@user.com").GetAwaiter().GetResult(), "User").Result.ToString();
            }
        }
        private void AddLanguagesAndContent()
        {
            IEnumerable<Translation.Translation> translations = null;
            var basePath = $"{Directory.GetCurrentDirectory()}/Server/Translation/";

            if (!_context.Languages.Any())
            {
                var csv = new CsvReader(File.OpenText($"{basePath}languages.csv"));
                var languages = csv.GetRecords<ContactLists.Server.Translation.Language>().ToList();

                foreach (var language in languages)
                    _context.Languages.Add(
                        new Language
                        {
                            Id = Convert.ToInt32(language.Id),
                            Locale = language.Locale,
                            Description = language.Description
                        });
                _context.SaveChanges();
            }

            if (!_context.Content.Any())
            {
                var csv = new CsvReader(File.OpenText($"{basePath}translations.csv"));
                translations = csv.GetRecords<Translation.Translation>().ToList();

                foreach (var translation in translations)
                    _context.Content.Add(new Content
                    {
                        Id = translation.Id,
                        Key = translation.MachineName
                    });
                _context.SaveChanges();
            }

            if (!_context.ContentText.Any())
            {
                if (translations == null)
                {
                    var csv = new CsvReader(File.OpenText($"{basePath}translations.csv"));
                    translations = csv.GetRecords<Translation.Translation>().ToList();
                }

                foreach (var translation in translations)
                {
                    _context.ContentText.Add(new ContentText
                    {
                        Text = translation.Language1,
                        LanguageId = 1,
                        ContentId = translation.Id
                    });
                    _context.ContentText.Add(new ContentText
                    {
                        Text = translation.Language2,
                        LanguageId = 2,
                        ContentId = translation.Id
                    });
                    _context.ContentText.Add(new ContentText
                    {
                        Text = translation.Language3,
                        LanguageId = 3,
                        ContentId = translation.Id
                    });
                }

                _context.SaveChanges();
            }
        }

    }
}
