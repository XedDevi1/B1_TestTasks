using B1_TestTask_2.Models;
using B1_TestTask_2.Persistence;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1_TestTask_2.Services
{
    public class ExcelImportService
    {
        public static void InsertExcelDataToDatabase(string excelPath, AppDbContext context)
        {
            // Создаем объект файла и добавляем его в контекст
            var file = new Files
            {
                FileName = Path.GetFileName(excelPath) // Получаем название файла из пути
            };
            context.Files.Add(file); // Добавляем файл в контекст
            context.SaveChanges(); // Сохраняем изменения, чтобы получить Id для файла

            // Загружаем данные из Excel файла
            using (var package = new ExcelPackage())
            {
                using (var stream = File.OpenRead(excelPath))
                {
                    package.Load(stream);
                }
                var ws = package.Workbook.Worksheets["Sheet1"];

                Classes currentClass = null;
                Dictionary<int, AccountGroups> accountGroupsDict = new Dictionary<int, AccountGroups>();

                // Итерируем по строкам в Excel
                for (int rowNum = 9; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var firstCellValue = ws.Cells[rowNum, 1].Text.Trim();

                    // Если первая ячейка начинается с "КЛАСС", создаем новый объект класса
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
                        // Если первая ячейка представляет собой число и оно больше 1000
                        if (int.TryParse(firstCellValue, out accountNumber) && accountNumber >= 1000)
                        {
                            int accountGroupKey = accountNumber / 100;

                            AccountGroups accountGroup;
                            // Если группа с таким ключом уже существует, используем ее, иначе создаем новую
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

                            // Создаем объект деталей счета и объект счета
                            var accountDetails = new AccountDetails
                            {
                                ActiveOpeningBalance = decimal.Parse(ws.Cells[rowNum, 2].Text, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU")),
                                PassiveOpeningBalance = decimal.Parse(ws.Cells[rowNum, 3].Text, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU")),
                                DebitTurnover = decimal.Parse(ws.Cells[rowNum, 4].Text, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU")),
                                LoanTurnover = decimal.Parse(ws.Cells[rowNum, 5].Text, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU")),
                                ActiveClosingBalance = decimal.Parse(ws.Cells[rowNum, 6].Text, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU")),
                                PassiveClosingBalance = decimal.Parse(ws.Cells[rowNum, 7].Text, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU")),
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
                        // Если первая ячейка содержит две цифры, или "ПО КЛАССУ", или "БАЛАНС", игнорируем строку
                        else if (firstCellValue.Length == 2 || firstCellValue == "ПО КЛАССУ" || firstCellValue == "БАЛАНС")
                        {
                            continue;
                        }
                    }
                }
                context.SaveChanges();
            }
        }

        // Проверка наличия файла в базе данных по его имени
        public static bool FileExistsInDatabase(string fileName, AppDbContext context)
        {
            return context.Files.Any(f => f.FileName == fileName);
        }
    }
}
