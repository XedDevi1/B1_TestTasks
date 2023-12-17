using B1_TestTask_2.Services;
using B1_TestTask_2.ViewModels;
using Syncfusion.UI.Xaml.Grid;
using System.Windows;

namespace B1_TestTask_2
{
    public partial class DataDisplayWindow : Window
    {
        public DataDisplayWindow(int fileId)
        {
            InitializeComponent();
            DataContext = new DisplayDataService(fileId, sfDataGrid);
            ConfigureDataGridSettings();
        }

        // Configure the settings for SfDataGrid, including event handling
        private void ConfigureDataGridSettings()
        {
            sfDataGrid.SelectionUnit = GridSelectionUnit.Cell;
            sfDataGrid.NavigationMode = Syncfusion.UI.Xaml.Grid.NavigationMode.Cell;
            sfDataGrid.QueryCoveredRange += OnQueryCoveredRange;
        }

        // Event handler for the QueryCoveredRange event in SfDataGrid
        private void OnQueryCoveredRange(object sender, Syncfusion.UI.Xaml.Grid.GridQueryCoveredRangeEventArgs e)
        {
            var dataGrid = sender as SfDataGrid;

            var recordIndex = dataGrid.ResolveToRecordIndex(e.RowColumnIndex.RowIndex);

            var record = dataGrid.View.Records[recordIndex].Data as AccountDisplayModel;

            // Check if the record is a class header
            if (record.DisplayText != null && record.IsClassHeader)
            {
                // Specify the covered cell range for class headers
                e.Range = new CoveredCellInfo(0, 6, e.RowColumnIndex.RowIndex, e.RowColumnIndex.RowIndex);
                e.Handled = true;
            }
        }
    }
}
