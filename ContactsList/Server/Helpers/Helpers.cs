using System.Collections.Generic;
using System.Linq;
using Serilog;
using Serilog.Events;
using static Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults;
using System;

namespace ContactsList.Server
{
    public static class Helpers
    {
        public static void SetupSerilog()
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel
            .Information()
            .WriteTo.RollingFile("logs/log-{Date}.txt", LogEventLevel.Information) // Uncomment if logging required on text file
            .WriteTo.Seq("http://localhost:5341/")
            .CreateLogger();
        }

        public static IEnumerable<string> DefaultMimeTypes => MimeTypes.Concat(new[]
                                {
                                    "image/svg+xml",
                                    "application/font-woff2"
                                });

        public static bool IsMultipartContentType(string contentType)
        {
            return
                !string.IsNullOrEmpty(contentType) &&
                contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string GetBoundary(string contentType)
        {
            var elements = contentType.Split(' ');
            var element = elements.Where(entry => entry.StartsWith("boundary=")).First();
            var boundary = element.Substring("boundary=".Length);
            // Remove quotes
            if (boundary.Length >= 2 && boundary[0] == '"' &&
                boundary[boundary.Length - 1] == '"')
            {
                boundary = boundary.Substring(1, boundary.Length - 2);
            }
            return boundary;
        }

        public static string GetFileName(string contentDisposition)
        {
            return contentDisposition
                .Split(';')
                .SingleOrDefault(part => part.Contains("filename"))
                .Split('=')
                .Last()
                .Trim('"');
        }

    }
}