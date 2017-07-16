using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsList.Server.Services.ServiceModels
{
    public class CheckResult
    {
        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }
    }
}
