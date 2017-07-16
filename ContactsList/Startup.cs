using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ContactsList.Server;
using ContactsList.Server.Extensions;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.WebUtilities;
using System.IO;
using System;

namespace ContactsList
{
    public class Startup
    {
        // Order or run
        //1) Constructor
        //2) Configure services
        //3) Configure

        private IHostingEnvironment _hostingEnv;
        public Startup(IHostingEnvironment env)
        {
            _hostingEnv = env;

            Helpers.SetupSerilog();

            var builder = new ConfigurationBuilder()
                           .SetBasePath(env.ContentRootPath)
                           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                           .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                           .AddEnvironmentVariables();
            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();
        }

        public static IConfigurationRoot Configuration { get; set; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            if (_hostingEnv.IsDevelopment())
            {
                services.AddSslCertificate(_hostingEnv);
            }
            services.AddOptions();

            services.AddResponseCompression(options =>
            {
                options.MimeTypes = Helpers.DefaultMimeTypes;
            });

            services.AddCustomDbContext();

            services.AddCustomIdentity();

            services.AddCustomOpenIddict();

            services.AddMemoryCache();

            services.RegisterMapping();

            services.RegisterCustomServices();

            services.RegisterContactsServices();

            services.AddSingleton(provider => Configuration);
            
            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

            services.AddCustomizedMvc();

            // Node services are to execute any arbitrary nodejs code from .net
            services.AddNodeServices();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "AspNetCoreSpa", Version = "v1" });
            });
        }
        public void Configure(IApplicationBuilder app)
        {
            app.AddDevMiddlewares();

            if (_hostingEnv.IsProduction())
            {
                app.UseResponseCompression();
            }

            app.Use(async (context, next) =>
            {
                if (!Helpers.IsMultipartContentType(context.Request.ContentType))
                {
                    await next();
                    return;
                }

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/logfile.txt");
                if (File.Exists(path))
                    File.AppendAllLines(path, new string[] { $"{DateTime.Now} Start streaming file" });

                var boundary = Helpers.GetBoundary(context.Request.ContentType);
                var reader = new MultipartReader(boundary, context.Request.Body);
                var section = await reader.ReadNextSectionAsync();

                while (section != null)
                {
                    // process each image
                    const int chunkSize = 1024;
                    var buffer = new byte[chunkSize];
                    var bytesRead = 0;
                    var fileName = Helpers.GetFileName(section.ContentDisposition);

                    using (var stream = new FileStream($"wwwroot/files/{fileName}", FileMode.Append))
                    {
                        do
                        {
                            bytesRead = await section.Body.ReadAsync(buffer, 0, buffer.Length);
                            stream.Write(buffer, 0, bytesRead);

                        } while (bytesRead > 0);
                    }

                    section = await reader.ReadNextSectionAsync();
                }

                if (File.Exists(path))
                    File.AppendAllLines(path, new string[] { $"{DateTime.Now} End streaming file" });

                //await next();
                //context.Response.WriteAsync("Done.");
            });


            app.SetupMigrations();

            app.UseXsrf();

            app.UseStaticFiles();

            app.UseIdentity();

            app.UseOpenIddict();

            // Add a middleware used to validate access
            // tokens and protect the API endpoints.
            app.UseOAuthValidation();

            // Alternatively, you can also use the introspection middleware.
            // Using it is recommended if your resource server is in a
            // different application/separated from the authorization server.
            //
            // app.UseOAuthIntrospection(options => {
            //     options.AutomaticAuthenticate = true;
            //     options.AutomaticChallenge = true;
            //     options.Authority = "http://localhost:54895/";
            //     options.Audiences.Add("resource_server");
            //     options.ClientId = "resource_server";
            //     options.ClientSecret = "875sqd4s5d748z78z7ds1ff8zz8814ff88ed8ea4z4zzd";
            // });

            app.UseOAuthProviders();

            app.UseMvc(routes =>
            {
                // http://stackoverflow.com/questions/25982095/using-googleoauth2authenticationoptions-got-a-redirect-uri-mismatch-error
                routes.MapRoute(name: "signin-google", template: "signin-google", defaults: new { controller = "Account", action = "ExternalLoginCallback" });

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
