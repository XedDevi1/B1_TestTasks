using B1_TestTask_2.Persistence;
using B1_TestTask_2.Services;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
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
    // Основной класс окна приложения.
    public partial class MainWindow : Window
    {
        // Коллекция для хранения информации о файлах Excel.
        public ObservableCollection<FileInfo> ExcelFiles { get; set; }

        // Конструктор основного окна.
        public MainWindow()
        {
            // Инициализация компонентов окна.
            InitializeComponent();
            // Установка лицензионного контекста для ExcelPackage.
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Инициализация коллекции файлов Excel.
            ExcelFiles = new ObservableCollection<FileInfo>();
            // Привязка коллекции к ListView в пользовательском интерфейсе.
            FilesListView.ItemsSource = ExcelFiles;
            // Загрузка списка файлов Excel.
            LoadExcelFiles();
        }

        // Метод для загрузки файлов Excel из заданной папки.
        private void LoadExcelFiles()
        {
            // Путь к папке с файлами Excel.
            var folderPath = @"D:\Тестовые\TestProject\";
            // Получение всех файлов с расширением .xlsx из папки.
            var files = Directory.GetFiles(folderPath, "*.xlsx");
            // Очистка текущей коллекции файлов.
            ExcelFiles.Clear();
            // Добавление информации о файлах в коллекцию.
            foreach (var file in files)
            {
                ExcelFiles.Add(new FileInfo(file));
            }
        }

        // Обработчик события двойного клика мыши по элементу ListView.
        private void FilesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Проверка выбранного элемента в ListView.
            if (FilesListView.SelectedItem is FileInfo selectedFile)
            {
                // Получение полного пути к выбранному файлу Excel.
                string excelPath = selectedFile.FullName;

                // Создание контекста базы данных.
                using (var context = new AppDbContext())
                {
                    // Проверка наличия записей в таблице Accounts.
                    if (!context.Accounts.Any())
                    {
                        // Вставка данных из Excel файла в базу данных.
                        ExcelImportService.InsertExcelDataToDatabase(excelPath, context);
                    }
                }

                // Создание и отображение окна для отображения данных.
                DataDisplayWindow dataDisplayWindow = new DataDisplayWindow();
                dataDisplayWindow.Show();
            }
        }
    }
}