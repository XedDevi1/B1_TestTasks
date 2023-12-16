using B1_TestTask_2.Persistence;
using B1_TestTask_2.ViewModels;
using Microsoft.EntityFrameworkCore;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;
using System.Xml;

namespace B1_TestTask_2.Services
{
    public class DisplayDataService
    {
        public CollectionViewSource AccountsViewSource { get; set; }
        public SfDataGrid DataGrid { get; set; }

        // Конструктор для DisplayDataService
        public DisplayDataService(int fileId, SfDataGrid dataGrid)
        {
            AccountsViewSource = new CollectionViewSource();
            DataGrid = dataGrid;
            LoadDataFromDatabase(fileId);
        }

        // Загрузка данных из базы данных и заполнение SfDataGrid
        private void LoadDataFromDatabase(int fileId)
        {
            using (var context = new AppDbContext())
            {
                // Получение информации о файле из базы данных
                var fileInDb = context.Files.FirstOrDefault(f => f.Id == fileId);

                // Получение данных по счетам с соответствующими связанными сущностями из базы данных
                var accounts = context.Accounts
                    .Include(a => a.AccountDetails)
                    .Include(a => a.Class)
                    .Include(a => a.AccountGroups)
                    .Where(a => a.Class.FileId == fileId)
                    .ToList();

                // Преобразование данных по счетам в модели отображения
                var accountDisplayModels = accounts.Select(account => new AccountDisplayModel
                {
                    // Отображение свойств счета на свойства модели отображения
                    AccountNumber = account.AccountNumber,
                    ClassName = account.Class.ClassName,
                    AccountGroup = account.AccountGroups.AccountGroup,
                    ActiveOpeningBalance = account.AccountDetails.ActiveOpeningBalance,
                    PassiveOpeningBalance = account.AccountDetails.PassiveOpeningBalance,
                    DebitTurnover = account.AccountDetails.DebitTurnover,
                    LoanTurnover = account.AccountDetails.LoanTurnover,
                    ActiveClosingBalance = account.AccountDetails.ActiveClosingBalance,
                    PassiveClosingBalance = account.AccountDetails.PassiveClosingBalance,
                    IsGroupSummary = false,
                    IsClassSummary = false,
                    DisplayText = account.AccountNumber.ToString(),
                }).ToList();

                var displayData = new List<AccountDisplayModel>();

                // Группировка и суммирование моделей отображения счетов
                foreach (var classGroup in accountDisplayModels.GroupBy(a => a.ClassName).OrderBy(g => g.Key))
                {
                    // Добавление заголовка класса
                    displayData.Add(new AccountDisplayModel { DisplayText = classGroup.Key, IsClassHeader = true });

                    // Перебор групп счетов внутри класса
                    foreach (var group in classGroup.GroupBy(a => a.AccountGroup).OrderBy(g => g.Key))
                    {
                        // Добавление отдельных счетов
                        displayData.AddRange(group.OrderBy(a => a.AccountNumber));

                        // Добавление суммарной информации по группе счетов
                        var groupSummary = new AccountDisplayModel
                        {
                            // Заполнение свойств суммарной информации по группе
                            DisplayText = $"{group.Key}",
                            ActiveOpeningBalance = group.Sum(a => a.ActiveOpeningBalance),
                            PassiveOpeningBalance = group.Sum(a => a.PassiveOpeningBalance),
                            DebitTurnover = group.Sum(a => a.DebitTurnover),
                            LoanTurnover = group.Sum(a => a.LoanTurnover),
                            ActiveClosingBalance = group.Sum(a => a.ActiveClosingBalance),
                            PassiveClosingBalance = group.Sum(a => a.PassiveClosingBalance),
                            IsGroupSummary = true,
                        };

                        displayData.Add(groupSummary);
                    }

                    // Добавление суммарной информации по классу
                    var classSummary = new AccountDisplayModel
                    {
                        // Заполнение свойств суммарной информации по классу
                        DisplayText = "ПО КЛАССУ",
                        ActiveOpeningBalance = classGroup.Sum(a => a.ActiveOpeningBalance),
                        PassiveOpeningBalance = classGroup.Sum(a => a.PassiveOpeningBalance),
                        DebitTurnover = classGroup.Sum(a => a.DebitTurnover),
                        LoanTurnover = classGroup.Sum(a => a.LoanTurnover),
                        ActiveClosingBalance = classGroup.Sum(a => a.ActiveClosingBalance),
                        PassiveClosingBalance = classGroup.Sum(a => a.PassiveClosingBalance),
                        IsClassSummary = true,
                    };

                    displayData.Add(classSummary);
                }
                // Установка данных отображения в качестве источника для CollectionViewSource
                AccountsViewSource.Source = displayData;
            }
        }
    }
}
