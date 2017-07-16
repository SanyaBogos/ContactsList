using ContactsList.Server.ViewModels.FileViewModels;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactsList.Server.Services.ServiceModels
{
    public class BuildContactParams
    {
        public ExcelWorksheet Worksheet { get; set; }
        public Dictionary<string, int> HeaderDictionary { get; set; }
        public string[] ColumnNames { get; set; }
        public ContactViewModel ContactVM { get; set; }
        public int Row { get; set; }
    }
}
