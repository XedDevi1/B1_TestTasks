using B1_TestTask_2.Persistence;
using B1_TestTask_2.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace B1_TestTask_2.Services
{
    // Класс для управления отображением данных счетов.
    public class DisplayDataService
    {
        // Источник данных для представления счетов.
        public CollectionViewSource AccountsViewSource { get; set; }

        // Конструктор класса, инициализирующий источник данных и загружающий данные из базы данных.
        public DisplayDataService()
        {
            AccountsViewSource = new CollectionViewSource();
            LoadDataFromDatabase();
        }

        // Метод для загрузки данных из базы данных.
        private void LoadDataFromDatabase()
        {
            // Создание контекста базы данных.
            using (var context = new AppDbContext())
            {
                // Получение списка счетов с подробностями, классами и группами счетов.
                var accounts = context.Accounts
                    .Include(a => a.AccountDetails)
                    .Include(a => a.Class)
                    .Include(a => a.AccountGroups)
                    .ToList();

                // Преобразование списка счетов в список моделей отображения счетов.
                var accountDisplayModels = accounts.Select(account => new AccountDisplayModel
                {
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
                    DisplayText = account.AccountNumber.ToString()
                }).ToList();

                // Список для хранения обработанных данных для отображения.
                var displayData = new List<AccountDisplayModel>();

                // Группировка моделей отображения счетов по названию класса и их сортировка.
                foreach (var classGroup in accountDisplayModels.GroupBy(a => a.ClassName).OrderBy(g => g.Key))
                {
                    // Добавление заголовка класса.
                    displayData.Add(new AccountDisplayModel { DisplayText = classGroup.Key, IsClassHeader = true });

                    // Группировка счетов по группе счетов и их сортировка.
                    foreach (var group in classGroup.GroupBy(a => a.AccountGroup).OrderBy(g => g.Key))
                    {
                        // Добавление счетов в отображаемые данные.
                        displayData.AddRange(group.OrderBy(a => a.AccountNumber));

                        // Создание и добавление сводки по группе счетов.
                        var groupSummary = new AccountDisplayModel
                        {
                            DisplayText = $"{group.Key}",
                            ActiveOpeningBalance = group.Sum(a => a.ActiveOpeningBalance),
                            PassiveOpeningBalance = group.Sum(a => a.PassiveOpeningBalance),
                            DebitTurnover = group.Sum(a => a.DebitTurnover),
                            LoanTurnover = group.Sum(a => a.LoanTurnover),
                            ActiveClosingBalance = group.Sum(a => a.ActiveClosingBalance),
                            PassiveClosingBalance = group.Sum(a => a.PassiveClosingBalance),
                            IsGroupSummary = true
                        };
                        displayData.Add(groupSummary);
                    }

                    // Создание и добавление сводки по классу счетов.
                    var classSummary = new AccountDisplayModel
                    {
                        DisplayText = "ПО КЛАССУ",
                        ActiveOpeningBalance = classGroup.Sum(a => a.ActiveOpeningBalance),
                        PassiveOpeningBalance = classGroup.Sum(a => a.PassiveOpeningBalance),
                        DebitTurnover = classGroup.Sum(a => a.DebitTurnover),
                        LoanTurnover = classGroup.Sum(a => a.LoanTurnover),
                        ActiveClosingBalance = classGroup.Sum(a => a.ActiveClosingBalance),
                        PassiveClosingBalance = classGroup.Sum(a => a.PassiveClosingBalance),
                        IsClassSummary = true
                    };
                    displayData.Add(classSummary);
                }

                // Установка источника данных для представления счетов.
                AccountsViewSource.Source = displayData;
            }
        }
    }

}
