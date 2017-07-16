using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsList.Server.Services.ServiceModels
{
    public class CheckHeaderResult
    {
        public string[] LackOfNonRequiredFields { get; set; }
        public string[] ExtraFields { get; set; }
    }
}
