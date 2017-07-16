using ContactsList.Server.Filters.Exceptions;
using ContactsList.Server.Services.ServiceModels;
using ContactsList.Server.ViewModels.FileViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ContactsList.Server.Services
{
    public class FileFormatService
    {
        private readonly string _fileStoragePath;
        private readonly string[] _allowedColumns;
        private readonly string[] _necessaryColumns;
        private readonly string[] _optionalColumns;

        private const string NO_NON_MANDATORY_FIELD_WARN = "Some of non mandatory fields are not exist";

        public FileFormatService(
            IConfigurationRoot configuration,
            IHostingEnvironment env)
        {
            _fileStoragePath = $"{env.WebRootPath}/{configuration["Data:FileStorage:Path"]}";
            _allowedColumns = configuration["Data:AllowedColumns"].Split(';');
            _necessaryColumns = configuration["Data:NecessaryColumns"].Split(';');
            _optionalColumns = _allowedColumns.Except(_necessaryColumns).ToArray();
        }

        public ComplexResultViewModel GetContactsFromFile(FilesViewModel fileVM)
        {
            CheckFiles(fileVM.FileNames);

            var successContacts = new List<ContactViewModel>();
            var issuedContacts = new List<ContactViewModel>();
            var errors = new List<string>();
            var warnings = new List<string>();

            foreach (var fileName in fileVM.FileNames)
            {
                var file = new FileInfo($"{_fileStoragePath}/{fileName}");
                using (var package = new ExcelPackage(file))
                {
                    foreach (var worksheet in package.Workbook.Worksheets)
                    {
                        if (worksheet.Dimension == null)
                            continue;

                        int rowCount = worksheet.Dimension.Rows;
                        int colCount = worksheet.Dimension.Columns;

                        var headerDictionary = GetHeaderDictionary(worksheet, colCount);
                        var result = CheckHeader(headerDictionary.Keys);

                        if (result.Errors.Count > 0)
                        {
                            errors.AddRange(result.Errors.Select(x => $"[{worksheet.Name}] {x}"));
                            continue;
                        }

                        var notEnoughAddressFields =
                            result.Warnings.Any(x => x.Contains(NO_NON_MANDATORY_FIELD_WARN));

                        warnings.AddRange(result.Warnings.Select(x => $"[{worksheet.Name}] {x}"));

                        var buildContactParams = new BuildContactParams
                        {
                            Worksheet = worksheet,
                            HeaderDictionary = headerDictionary
                        };

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var contact = new ContactViewModel { IssuedColumns = new List<string>() };
                            buildContactParams.ContactVM = contact;
                            buildContactParams.ColumnNames = _necessaryColumns;
                            buildContactParams.Row = row;

                            BuildColumnsForContact(buildContactParams);

                            if (notEnoughAddressFields)
                                continue;

                            if (IsAddressValid(buildContactParams))
                            {
                                buildContactParams.ColumnNames = _optionalColumns;
                                BuildColumnsForContact(buildContactParams);
                            }

                            if (contact.IssuedColumns.Count > 0)
                                issuedContacts.Add(contact);
                            else
                                successContacts.Add(contact);
                        }
                    }
                }
            }

            DisposeFiles(fileVM.FileNames);

            return new ComplexResultViewModel()
            {
                SuccessfullySaved = successContacts.ToArray(),
                IssuedElements = issuedContacts.ToArray(),
                ErrorMessages = errors.ToArray(),
                WarningMessages = warnings.ToArray()
            };
        }

        private void DisposeFiles(string[] files)
        {
            foreach (var file in files)
            {
                var path = $"{_fileStoragePath}/{file}";
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        private FormatResult FormatPhoneNumber(string number)
        {
            var prepared = number.Replace(" ", "").Replace("-", "")
                .Replace("(", "").Replace(")", "")
                .Replace("+", "");

            if (!Regex.IsMatch(prepared, @"^\d+$"))
                return new FormatResult { IsSuccess = false, Value = "Incorrect symbols for phone" };

            if (prepared.Length == 10)
                prepared = $"7{prepared}";
            else if (prepared.Length > 11 || prepared.Length < 10)
                return new FormatResult { IsSuccess = false, Value = "Incorrect digits count in phone number" };

            return new FormatResult { IsSuccess = true, Value = prepared };
        }

        private void BuildColumnsForContact(BuildContactParams param)
        {
            foreach (var column in param.ColumnNames)
            {
                var val = param.Worksheet.Cells[param.Row, param.HeaderDictionary[column]]
                    .Value.ToString();

                if (column.Equals("Phone"))
                {
                    var result = FormatPhoneNumber(val);
                    if (!result.IsSuccess)
                        param.ContactVM.IssuedColumns.Add(
                            $"{result.Value}. Fix column:{GetColumnName(param.HeaderDictionary[column])}{param.Row} (row: {param.Row}, column: {param.HeaderDictionary[column]})");
                    else
                        val = result.Value;
                }

                param.ContactVM.GetType().GetProperty(column).SetValue(param.ContactVM, val);
            }
        }

        private bool IsAddressValid(BuildContactParams param)
        {
            foreach (var column in _optionalColumns)
            {
                var val = param.Worksheet.Cells[param.Row, param.HeaderDictionary[column]]
                                .Value;

                if (val == null)
                    return false;

                if (string.IsNullOrEmpty(val.ToString()))
                    return false;
            }

            return true;
        }

        private Dictionary<string, int> GetHeaderDictionary(ExcelWorksheet worksheet, int colCount)
        {
            var headerColumnsDict = new Dictionary<string, int>();
            for (int col = 1; col <= colCount; col++)
                headerColumnsDict.Add(worksheet.Cells[1, col].Value.ToString(), col);

            return headerColumnsDict;
        }

        private CheckResult CheckHeader(IEnumerable<string> headerColumns)
        {
            var result = new CheckResult
            {
                Errors = new List<string>(),
                Warnings = new List<string>()
            };

            // clean possible empty columns and remove from column list
            headerColumns = headerColumns.Where(x => !string.IsNullOrEmpty(x.Replace(" ", "")))
                .ToList();

            var intersected = headerColumns.Intersect(_allowedColumns);
            var areCompletelyTheSame = Enumerable.SequenceEqual(intersected.OrderBy(x => x), _allowedColumns.OrderBy(x => x));

            if (areCompletelyTheSame)
                return result;

            var missedRequiredColumns = _necessaryColumns.Except(intersected);

            if (missedRequiredColumns.Count() > 0)
                result.Errors.Add($"Missed required field(s) {string.Join(",", missedRequiredColumns)}");

            var extraFields = headerColumns.Except(_allowedColumns).ToArray();
            var lackOfNonRequired = _allowedColumns.Except(headerColumns).ToArray();

            if (extraFields.Count() > 0)
                result.Warnings.Add($"Extra fields ({string.Join(",", extraFields)}) has been ignored.");

            if (lackOfNonRequired.Count() > 0)
                result.Warnings.Add(
                    $"{NO_NON_MANDATORY_FIELD_WARN}: {string.Join(",", extraFields)}. Whole address won't be saved. Please add the following fields and upload excel file one more time.");

            return result;
        }

        private void CheckFiles(string[] fileNames)
        {
            if (fileNames.Any(x => string.IsNullOrEmpty(x)))
                throw new FileUploadException("Name of file shouldn't be empty.");

            if (fileNames.Any(x => !System.IO.File.Exists($"{_fileStoragePath}/{x}")))
                throw new FileUploadException("At least one file hasn't been uploaded.");

            if (fileNames.Any(x => Path.GetExtension($"{_fileStoragePath}/{x}").Equals("xlsx")))
                throw new FileUploadException("At least one file has incorrect type.");
        }

        private string GetColumnName(int index)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var value = "";

            if (index >= letters.Length)
                value += letters[index / letters.Length - 1];

            value += letters[index % letters.Length];

            return value;
        }

        public static string GenerateUniqueFileName(string path, string fileName)
        {
            var filesInFolder = Directory.GetFiles(path);
            filesInFolder = filesInFolder
                .Select(x => x.Split('/').Last()).ToArray();

            if (filesInFolder.Contains(fileName))
                fileName = GenerateUniqueFileName(filesInFolder, fileName);

            return fileName;
        }

        private static string GenerateUniqueFileName(string[] filesInFolder, string fileName)
        {
            var splitter = fileName.LastIndexOf('.');
            var type = fileName.Substring(splitter, fileName.Length - splitter);
            var name = fileName.Substring(0, splitter);
            string newFileName;
            var index = 0;
            var additionalSymbolsCount = 1;

            // this magic checks possible file name, tries 5 times to generate random symbols
            // and increase random symbols count if no luck
            do
            {
                newFileName = $"{name}{Helpers.GenerateRandomSymbols(additionalSymbolsCount)}{type}";
                index++;
                if (index % 5 == 0)
                    additionalSymbolsCount++;
            }
            while (filesInFolder.Contains(newFileName));

            return newFileName;
        }
    }
}
