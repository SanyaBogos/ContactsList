using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsList.Server.ViewModels.FileViewModels
{
    public class ComplexResultViewModel
    {
        public ContactViewModel[] SuccessfullySaved { get; set; }
        public ContactViewModel[] IssuedElements { get; set; }
        public string[] ErrorMessages { get; set; }
        public string[] WarningMessages { get; set; }
    }
}
