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
    // Класс для импорта данных из Excel файла в базу данных.
    public class ExcelImportService
    {
        // Метод для вставки данных из Excel файла в базу данных.
        public static void InsertExcelDataToDatabase(string excelPath, AppDbContext context)
        {
            // Создание нового пакета Excel.
            using (var package = new ExcelPackage())
            {
                // Открытие файла Excel для чтения.
                using (var stream = File.OpenRead(excelPath))
                {
                    // Загрузка данных из файла в пакет.
                    package.Load(stream);
                }
                // Получение рабочего листа с именем "Sheet1".
                var ws = package.Workbook.Worksheets["Sheet1"];

                // Переменная для хранения текущего класса.
                Classes currentClass = null;
                // Словарь для хранения групп счетов.
                Dictionary<int, AccountGroups> accountGroupsDict = new Dictionary<int, AccountGroups>();

                // Перебор строк с 9-й по последнюю в рабочем листе.
                for (int rowNum = 9; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    // Получение значения первой ячейки в строке и удаление пробелов.
                    var firstCellValue = ws.Cells[rowNum, 1].Text.Trim();

                    // Проверка, начинается ли значение ячейки с "КЛАСС".
                    if (firstCellValue.StartsWith("КЛАСС"))
                    {
                        // Создание и добавление нового класса в контекст базы данных.
                        currentClass = new Classes
                        {
                            ClassName = firstCellValue
                        };
                        context.Classes.Add(currentClass);
                        // Сохранение изменений в базе данных.
                        context.SaveChanges();
                    }
                    else
                    {
                        // Переменная для хранения номера счета.
                        int accountNumber;
                        // Попытка преобразования значения ячейки в число и проверка, что оно больше или равно 1000.
                        if (int.TryParse(firstCellValue, out accountNumber) && accountNumber >= 1000)
                        {
                            // Вычисление ключа группы счетов.
                            int accountGroupKey = accountNumber / 100;

                            // Переменная для хранения группы счетов.
                            AccountGroups accountGroup;
                            // Проверка наличия группы счетов в словаре.
                            if (!accountGroupsDict.TryGetValue(accountGroupKey, out accountGroup))
                            {
                                // Создание и добавление новой группы счетов в контекст базы данных.
                                accountGroup = new AccountGroups
                                {
                                    AccountGroup = accountGroupKey
                                };
                                context.AccountGroups.Add(accountGroup);
                                // Сохранение изменений в базе данных.
                                context.SaveChanges();
                                // Добавление группы счетов в словарь.
                                accountGroupsDict[accountGroupKey] = accountGroup;
                            }

                            // Создание деталей счета с преобразованием значений из ячеек в числа с учетом региональных настроек.
                            var accountDetails = new AccountDetails
                            {
                                ActiveOpeningBalance = decimal.Parse(ws.Cells[rowNum, 2].Text, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU")),
                                PassiveOpeningBalance = decimal.Parse(ws.Cells[rowNum, 3].Text, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU")),
                                DebitTurnover = decimal.Parse(ws.Cells[rowNum, 4].Text, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU")),
                                LoanTurnover = decimal.Parse(ws.Cells[rowNum, 5].Text, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU")),
                                ActiveClosingBalance = decimal.Parse(ws.Cells[rowNum, 6].Text, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU")),
                                PassiveClosingBalance = decimal.Parse(ws.Cells[rowNum, 7].Text, NumberStyles.Number, CultureInfo.GetCultureInfo("ru-RU")),
                            };

                            // Создание и добавление нового счета в контекст базы данных.
                            var account = new Accounts
                            {
                                AccountNumber = accountNumber,
                                ClassId = currentClass?.ClassNumber ?? 0,
                                AccountGroupId = accountGroup.Id,
                                AccountDetails = accountDetails
                            };

                            context.Accounts.Add(account);
                        }
                        // Пропуск строки, если значение ячейки не соответствует ожидаемому формату.
                        else if (firstCellValue.Length == 2 || firstCellValue == "ПО КЛАССУ" || firstCellValue == "БАЛАНС")
                        {
                            continue;
                        }
                    }
                }
                // Сохранение всех изменений в базе данных.
                context.SaveChanges();
            }
        }
    }
}
