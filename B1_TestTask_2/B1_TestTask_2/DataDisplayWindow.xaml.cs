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

namespace B1_TestTask_2
{
    public partial class DataDisplayWindow : Window
    {
        public DataDisplayWindow()
        {
            // Инициализация компонентов окна.
            InitializeComponent();
            // Установка контекста данных окна.
            DataContext = new DisplayDataService();
            // Подписка на событие QueryCoveredRange элемента управления sfDataGrid.
            sfDataGrid.QueryCoveredRange += SfDataGrid_QueryCoveredRange;
        }

        // Обработчик события QueryCoveredRange для sfDataGrid.
        private void SfDataGrid_QueryCoveredRange(object sender, GridQueryCoveredRangeEventArgs e)
        {
            // Получение индекса строки и столбца, для которых запрашивается информация о покрытии.
            var rowIndex = e.RowColumnIndex.RowIndex;
            var columnIndex = e.RowColumnIndex.ColumnIndex;
            // Приведение отправителя события к типу SfDataGrid.
            var dataGrid = sender as SfDataGrid;

            // Проверка, что dataGrid не null.
            if (dataGrid != null)
            {
                // Получение записи из модели представления по индексу строки.
                var record = dataGrid.View.Records[rowIndex - 1].Data as AccountDisplayModel;
                // Проверка, что запись не null и является заголовком класса.
                if (record != null && record.IsClassHeader)
                {
                    // Установка диапазона покрытия для заголовка класса на всю ширину таблицы.
                    e.Range = new CoveredCellInfo(1, dataGrid.Columns.Count, rowIndex, rowIndex);
                    // Установка флага Handled в true, чтобы предотвратить дальнейшую обработку события.
                    e.Handled = true;
                }
            }
        }
    }
}
