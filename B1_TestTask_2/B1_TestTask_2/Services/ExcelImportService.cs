using B1_TestTask_2.Models;
using B1_TestTask_2.Persistence;
using OfficeOpenXml;
using System.Globalization;
using System.IO;

namespace B1_TestTask_2.Services
{
    public class ExcelImportService
    {
        public static void InsertExcelDataToDatabase(string excelPath, AppDbContext context)
        {
            // Create a file object and add it to the context
            var file = new Files
            {
                FileName = Path.GetFileName(excelPath) // Get the file name from the path
            };
            context.Files.Add(file); // Add the file to the context
            context.SaveChanges(); // Save changes to get the Id for the file

            // Load data from the Excel file
            using (var package = new ExcelPackage())
            {
                using (var stream = File.OpenRead(excelPath))
                {
                    package.Load(stream);
                }
                var worksheet = package.Workbook.Worksheets["Sheet1"];

                Classes currentClass = null;
                Dictionary<int, AccountGroups> accountGroupsDict = new Dictionary<int, AccountGroups>();

                // Iterate through rows in Excel
                for (int rowNum = 9; rowNum <= worksheet.Dimension.End.Row; rowNum++)
                {
                    var firstCellValue = worksheet.Cells[rowNum, 1].Text.Trim();

                    // If the first cell starts with "КЛАСС," create a new class object
                    if (firstCellValue.StartsWith("КЛАСС"))
                    {
                        currentClass = new Classes
                        {
                            ClassName = firstCellValue,
                            FileId = file.Id
                        };
                        context.Classes.Add(currentClass);
                        context.SaveChanges();
                    }
                    else
                    {
                        int accountNumber;
                        // If the first cell is a number and it is greater than or equal to 1000
                        if (int.TryParse(firstCellValue, out accountNumber) && accountNumber >= 1000)
                        {
                            int accountGroupKey = accountNumber / 100;

                            AccountGroups accountGroup;
                            // If a group with the key already exists, use it; otherwise, create a new one
                            if (!accountGroupsDict.TryGetValue(accountGroupKey, out accountGroup))
                            {
                                accountGroup = new AccountGroups
                                {
                                    AccountGroup = accountGroupKey
                                };
                                context.AccountGroups.Add(accountGroup);
                                context.SaveChanges();
                                accountGroupsDict[accountGroupKey] = accountGroup;
                            }

                            // Create an account details object and an account object
                            var accountDetails = new AccountDetails
                            {
                                ActiveOpeningBalance = decimal.Parse(worksheet.Cells[rowNum, 2].Text, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU")),
                                PassiveOpeningBalance = decimal.Parse(worksheet.Cells[rowNum, 3].Text, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU")),
                                DebitTurnover = decimal.Parse(worksheet.Cells[rowNum, 4].Text, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU")),
                                LoanTurnover = decimal.Parse(worksheet.Cells[rowNum, 5].Text, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU")),
                                ActiveClosingBalance = decimal.Parse(worksheet.Cells[rowNum, 6].Text, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU")),
                                PassiveClosingBalance = decimal.Parse(worksheet.Cells[rowNum, 7].Text, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU")),
                            };

                            var account = new Accounts
                            {
                                AccountNumber = accountNumber,
                                ClassId = currentClass?.ClassNumber ?? 0,
                                AccountGroupId = accountGroup.Id,
                                AccountDetails = accountDetails
                            };

                            context.Accounts.Add(account);
                        }
                        // If the first cell contains two digits, or "ПО КЛАССУ," or "БАЛАНС," ignore the row
                        else if (firstCellValue.Length == 2 || firstCellValue == "ПО КЛАССУ" || firstCellValue == "БАЛАНС")
                        {
                            continue;
                        }
                    }
                }
                context.SaveChanges();
            }
        }

        // Check if the file exists in the database based on its name
        public static bool FileExistsInDatabase(string fileName, AppDbContext context)
        {
            return context.Files.Any(f => f.FileName == fileName);
        }
    }
}
