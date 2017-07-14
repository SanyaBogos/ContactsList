using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactLists.Server.Translation
{
    public class Language
    {
        public int Id { get; set; }
        public string Locale { get; set; }
        public string Description { get; set; }
    }
}
