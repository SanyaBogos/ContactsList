using ContactsList.Server.ViewModels.FileViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsList.Server.Services.Abstract
{
    public interface IFileFormatService
    {
        ComplexResultViewModel GetContactsFromFile(FilesViewModel fileVM);
    }
}
