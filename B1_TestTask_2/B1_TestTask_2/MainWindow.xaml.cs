using B1_TestTask_2.Persistence;
using B1_TestTask_2.Services;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices;
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
    public partial class MainWindow : Window
    {
        // Коллекция для хранения информации о файлах Excel
        public ObservableCollection<FileInfo> ExcelFiles { get; set; }

        public MainWindow()
        {
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

        private void FilesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FilesListView.SelectedItem is FileInfo selectedFile)
            {
                string excelPath = selectedFile.FullName;
                string fileName = System.IO.Path.GetFileName(excelPath);

                using (var context = new AppDbContext())
                {
                    // Проверяем, существует ли файл в базе данных.
                    var fileInDb = context.Files.FirstOrDefault(f => f.FileName == fileName);
                    if (fileInDb == null)
                    {
                        // Если файла нет, добавляем данные из Excel в базу данных.
                        ExcelImportService.InsertExcelDataToDatabase(excelPath, context);
                        // Обновляем контекст, чтобы получить Id только что добавленного файла.
                        context.SaveChanges();
                        // Получаем Id добавленного файла.
                        fileInDb = context.Files.FirstOrDefault(f => f.FileName == fileName);
                    }
                    // Если файл найден или только что был добавлен, получаем его Id.
                    var fileId = fileInDb?.Id; // fileId будет содержать Id файла или null, если файл не найден.
                    if (fileId.HasValue)
                    {
                        // Открываем окно для отображения данных.
                        DataDisplayWindow dataDisplayWindow = new DataDisplayWindow(fileId.Value);
                        dataDisplayWindow.Show();
                    }
                }
            }
        }
    }
}