using ContactsList;
using ContactsList.Server.Entities;
using ContactsList.Server.Security;
using ContactsList.Server.ViewModels;
using ContactsList.Server.ViewModels.AccountViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ContactsListTest
{
    public class DatabaseFixture : IDisposable
    {
        private RegisterViewModel _newUser;

        public IConfigurationRoot Configuration { get; set; }
        public TestServer Server { get; set; }
        public HttpClient Client { get; set; }
        public ApplicationUser NewUser { get; set; }
        public string CurrentDirectory { get; set; }

        public DatabaseFixture()
        {
            CurrentDirectory = Path.Combine(Directory.GetCurrentDirectory()
                .Split(new string[] { "bin" }, StringSplitOptions.RemoveEmptyEntries).First(),
                "../ContactsList");
            var appSettingsPath = Path.Combine(CurrentDirectory, "appsettings.json");
            var builder = new ConfigurationBuilder()
                .AddJsonFile(appSettingsPath, optional: false, reloadOnChange: true);
            Configuration = builder.Build();

            var host = new WebHostBuilder()
                .UseConfiguration(Configuration)
                .UseContentRoot(CurrentDirectory)
                .UseStartup<Startup>();

            Server = new TestServer(host);
            Client = Server.CreateClient();

            _newUser = new RegisterViewModel
            {
                Username = "Barada",
                Firstname = "Name",
                Lastname = "Surname",
                Email = "dasistfantastisch125@gmail.com",
                Password = "123qweASD!@#",
                //ConfirmPassword = "123qweASD!@#"
            };

            NewUser = GetNewUser();
            if (NewUser == null)
            {
                CreateNewUser();
                NewUser = GetNewUser();
            }
        }

        public void Dispose()
        {
            DeleteNewUser();
            Server.Dispose();
            Client.Dispose();
        }

        private ApplicationUser GetNewUser()
        {
            var loginResult = LoginAsAdmin();
            SetAuthCookie(loginResult);

            var jsonInString = JsonConvert.SerializeObject(new string[] { _newUser.Email },
                            new JsonSerializerSettings
                            {
                                ContractResolver = new CamelCasePropertyNamesContractResolver()
                            });

            var userResponse = Client.PostAsync($"api/Account/GetUsersByEmails",
                new StringContent(jsonInString, Encoding.UTF8, "application/json")).Result;
            var stream = userResponse.Content.ReadAsStringAsync().Result;
            var user = JsonConvert.DeserializeObject<ApplicationUser[]>(stream).FirstOrDefault();
            DropAuthCookie();
            return user;
        }

        private void CreateNewUser()
        {
            var jsonInString = JsonConvert.SerializeObject(_newUser,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
            var response = Client.PostAsync("api/Account/register",
                new StringContent(jsonInString, Encoding.UTF8, "application/json")).Result;
            response.EnsureSuccessStatusCode();
        }

        private void DeleteNewUser()
        {
            var loginResult = LoginAsAdmin();
            SetAuthCookie(loginResult);
            var deleteResult = Client.DeleteAsync($"api/Account/Delete?email={_newUser.Email}").Result;
            deleteResult.EnsureSuccessStatusCode();
        }

        private void SetAuthCookie(HttpResponseMessage response)
        {
            var authCookies = response.Headers.GetValues("Set-Cookie").First();
            var indexOfFirstSplitter = authCookies.IndexOf('=');
            var indexOfLastSplitter = authCookies.IndexOf(';');
            var key = authCookies.Substring(0, indexOfFirstSplitter);
            var value = authCookies.Substring(indexOfFirstSplitter + 1, authCookies.Length - indexOfFirstSplitter - (authCookies.Length - indexOfLastSplitter) - 1);
            Client.DefaultRequestHeaders.Add("Cookie", new CookieHeaderValue(key, value).ToString());
        }

        private void DropAuthCookie()
        {
            Client.DefaultRequestHeaders.Remove("Cookie");
        }

        private HttpResponseMessage LoginAsAdmin()
        {
            var admin = SecurityRulesSingleton.Instance.Rules.Users.First(u => u.Role.Equals("admin"));

            var jsonInString = JsonConvert.SerializeObject(
                new LoginViewModel { Email = admin.Email, Password = admin.Password },
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

            var loginResult = Client.PostAsync("api/Account/login",
                new StringContent(jsonInString, Encoding.UTF8, "application/json")).Result;
            loginResult.EnsureSuccessStatusCode();
            return loginResult;
        }
    }
}
