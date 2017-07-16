using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsList.Server.Filters.Exceptions
{
    public class FileUploadException : ApiException
    {
        public FileUploadException(string message) : base(message) { }
    }
}
