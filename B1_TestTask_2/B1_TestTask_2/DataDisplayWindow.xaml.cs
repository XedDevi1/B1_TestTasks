using B1_TestTask_2.Services;
using B1_TestTask_2.ViewModels;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace B1_TestTask_2
{
    public partial class DataDisplayWindow : Window
    {
        public DataDisplayWindow(int fileId)
        {
            InitializeComponent();
            DataContext = new DisplayDataService(fileId, sfDataGrid);
            sfDataGrid.SelectionUnit = GridSelectionUnit.Cell;
            sfDataGrid.NavigationMode = Syncfusion.UI.Xaml.Grid.NavigationMode.Cell;
            sfDataGrid.QueryCoveredRange += sfDataGrid_QueryCoveredRange;
        }

        // Настройка обработки события QueryCoveredRange в SfDataGrid
        private void sfDataGrid_QueryCoveredRange(object sender, Syncfusion.UI.Xaml.Grid.GridQueryCoveredRangeEventArgs e)
        {
            var dataGrid = sender as SfDataGrid;

            var recordIndex = dataGrid.ResolveToRecordIndex(e.RowColumnIndex.RowIndex);

            var record = dataGrid.View.Records[recordIndex].Data as AccountDisplayModel;
            // Проверка, является ли запись заголовком класса
            if (record.DisplayText != null && (record.DisplayText.StartsWith("КЛАСС") || record.IsClassHeader))
            {
                e.Range = new CoveredCellInfo(0, 6, e.RowColumnIndex.RowIndex, e.RowColumnIndex.RowIndex);
                e.Handled = true;
            }
        }
    }
}
