using B1_TestTask_2.Persistence;
using B1_TestTask_2.ViewModels;
using Microsoft.EntityFrameworkCore;
using Syncfusion.UI.Xaml.Grid;
using System.Windows.Data;

namespace B1_TestTask_2.Services
{
    public class DisplayDataService
    {
        public CollectionViewSource AccountsViewSource { get; set; }
        public SfDataGrid DataGrid { get; set; }

        public DisplayDataService(int fileId, SfDataGrid dataGrid)
        {
            AccountsViewSource = new CollectionViewSource();
            DataGrid = dataGrid;
            LoadDataFromDatabase(fileId);
        }

        // Load data from the database and populate SfDataGrid
        private void LoadDataFromDatabase(int fileId)
        {
            using (var context = new AppDbContext())
            {
                // Retrieve file information from the database
                var fileInDb = context.Files.FirstOrDefault(f => f.Id == fileId);

                // Retrieve account data with associated entities from the database
                var accounts = context.Accounts
                    .Include(a => a.AccountDetails)
                    .Include(a => a.Class)
                    .Include(a => a.AccountGroups)
                    .Where(a => a.Class.FileId == fileId)
                    .ToList();

                // Transform account data into display model
                var accountDisplayModels = accounts.Select(account => new AccountDisplayModel
                {
                    // Map account properties to display model properties
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

                // Group and sum up account display models
                foreach (var classGroup in accountDisplayModels.GroupBy(a => a.ClassName).OrderBy(g => g.Key))
                {
                    // Add class header
                    displayData.Add(new AccountDisplayModel { DisplayText = classGroup.Key, IsClassHeader = true });

                    // Iterate through account groups within the class
                    foreach (var group in classGroup.GroupBy(a => a.AccountGroup).OrderBy(g => g.Key))
                    {
                        // Add individual accounts
                        displayData.AddRange(group.OrderBy(a => a.AccountNumber));

                        // Add summary information for the account group
                        var groupSummary = new AccountDisplayModel
                        {
                            // Fill properties for the group summary
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

                    // Add summary information for the class
                    var classSummary = new AccountDisplayModel
                    {
                        // Fill properties for the class summary
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

                // Set display data as the source for CollectionViewSource
                AccountsViewSource.Source = displayData;
            }
        }
    }

}
