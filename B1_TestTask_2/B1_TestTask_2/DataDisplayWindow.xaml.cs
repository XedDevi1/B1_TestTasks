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
        }
    }
}
