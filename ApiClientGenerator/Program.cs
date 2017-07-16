using NSwag.CodeGeneration.TypeScript;
using NSwag.SwaggerGeneration.WebApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ApiClientGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var currentFolder = Directory.GetCurrentDirectory();
            var pathToDll = Path.Combine(currentFolder, "..", "ContactsList/bin/Debug/netcoreapp1.1/AspNetCoreSpa.dll");
            var settings = new WebApiAssemblyToSwaggerGeneratorSettings
            {
                AssemblyPaths = new string[] { pathToDll },
                DefaultUrlTemplate = "api/{controller}/{action}/{id}",
                DefaultPropertyNameHandling = NJsonSchema.PropertyNameHandling.CamelCase
            };

            //var excluded = new string[] { "Manage", "Base", "Home" };
            var generator = new WebApiAssemblyToSwaggerGenerator(settings);
            var allClasses = generator.GetControllerClasses();
            //var necessaryClasses = allClasses.Except(allClasses.Where(x => excluded.Any(y => x.Contains(y)))
            //    ).ToArray();
            var controllersToGenerate = new string[] { "File" };
            var necessaryClasses = allClasses.Where(x => controllersToGenerate.Any(y => x.Contains(y)))
                .ToArray();
            var document = generator.GenerateForControllersAsync(necessaryClasses).Result;

            var settingsForClientApiGeneration = new SwaggerToTypeScriptClientGeneratorSettings
            {
                ClassName = "{controller}Client",
                Template = TypeScriptTemplate.Angular
            };

            var generatorApi = new SwaggerToTypeScriptClientGenerator(document, settingsForClientApiGeneration);
            var code = generatorApi.GenerateFile();
            code = ImproveGeneration(code);
            File.WriteAllText(Path.Combine(currentFolder, "..", "ContactsList/Client/app", "apiDefinitions.ts"), code);
        }

        private static string ImproveGeneration(string code)
        {
            var improvements = new Dictionary<string, string>();
            improvements.Add("import { Injectable, Inject, Optional, OpaqueToken } from '@angular/core';",
                @"
import { Router } from '@angular/router';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { NotifficationsService, MessageDescription, Status } from './shared/services/notiffications.service';");

            improvements.Add("export const API_BASE_URL = new OpaqueToken('API_BASE_URL');",
                @"export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL');

@Injectable()
export class ErrorHandler {
    constructor(public router: Router,
        public notifficationsService: NotifficationsService) { }

    handleErrors(error: any) {
        if (error.status === 401) {
            sessionStorage.clear();
            this.router.navigate(['/login']);
            this.notifficationsService.notify(
                'Please log in', Status.Error, 'Unauthorized');
        } else if (error.status === 403) {
            // Forbidden
            this.router.navigate(['/login']);
            this.notifficationsService.notify(
                'Please log in', Status.Error, 'Unauthorized');
        } else if (error.status === 500) {
            // 1st case for debug purposes
            if (error.message)
                this.notifficationsService.notify(
                    'Server error', Status.Error, error.message);
            else
                this.notifficationsService.notify(
                    'Please contact admin', Status.Error, 'Internal server error');
        } else if (error.status === 400) {
            console.log(error._body);
            if (error._body)
                (<string[]>JSON.parse(error._body)).forEach((e) => {
                    this.notifficationsService.notify(
                        e, Status.Error);
                });
        }
        else {
            console.log(error._body);
        }
    }
}");

            improvements.Add(", file.fileName ? file.fileName : \"file\"","");
            //improvements.Add("any = undefined",
            //    "any | undefined");
            //improvements.Add("import { Http, Headers, ResponseContentType, Response } from '@angular/http';",
            //    "import { Http, Headers, Response } from '@angular/http';");

            foreach (var improvement in improvements)
                code = code.Replace(improvement.Key, improvement.Value);

            //var result = "result200";
            //var pattern = $@"(let {result}:\s+[a-zA-Z]+\[\]\s=\s)(\w+);((\n[ \S]+)+)({result} = \[\];)";
            //code = Regex.Replace(code, pattern, "$1[];$3");

            var pattern = @"(let options_ = {([\n].+)+};)";
            code = Regex.Replace(code, pattern, @"$1

        addXsrfToken(options_);
        addAuthToken(options_);");

            var patternCtorInjection = @"(constructor\()(\@Inject\(Http\) http: Http)";
            code = Regex.Replace(code, patternCtorInjection, @"$1public errorHandler: ErrorHandler, $2");

            var patternErrorCatch= @"(\.catch\(\()(response_)(: any\) => {)((\s+.+){9})";
            code = Regex.Replace(code, patternErrorCatch, @"$1error$3  
            this.errorHandler.handleErrors(error);
            return Observable.throw(error);$5");

            code += @"

function addAuthToken(options: any) {
    const authTokens = localStorage.getItem('auth-tokens');
    if (authTokens) {
        // tslint:disable-next-line:whitespace
        options.headers.append('Authorization', 'Bearer ' + JSON.parse(<any>authTokens).access_token);
    }
    return options;
}

function addXsrfToken(options:any) {
    const xsrfToken = getXsrfCookie();
    if (xsrfToken) {
        options.headers.append('X-XSRF-TOKEN', xsrfToken)
    }
    return options;
}

function getXsrfCookie(): string {
    const matches = document.cookie.match(/\bXSRF-TOKEN=([^\s;]+)/);
    try {
        return matches ? decodeURIComponent(matches[1]) : '';
    } catch (decodeError) {
        return '';
    }
}";
            return code;
        }
    }
}
