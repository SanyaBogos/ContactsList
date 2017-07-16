using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsList.Server.ViewModels.FileViewModels
{
    public class ContactViewModel
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        //public string FullAddress { get; set; }
        public string Index { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public List<string> IssuedColumns { get; set; }
    }
}
