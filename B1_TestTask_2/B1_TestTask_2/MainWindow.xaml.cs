using B1_TestTask_2.Persistence;
using B1_TestTask_2.Services;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace B1_TestTask_2
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<FileInfo> ExcelFiles { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            // Set the license context for ExcelPackage.
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelFiles = new ObservableCollection<FileInfo>();
            FilesListView.ItemsSource = ExcelFiles;
            // Load the list of Excel files.
            LoadExcelFiles();
        }

        private void LoadExcelFiles()
        {
            var folderPath = @"D:\Тестовые\TestProject\";
            var files = Directory.GetFiles(folderPath, "*.xlsx");
            // Clear the current collection of files.
            ExcelFiles.Clear();
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
                    // Check if the file exists in the database.
                    var fileInDb = context.Files.FirstOrDefault(f => f.FileName == fileName);
                    if (fileInDb == null)
                    {
                        // If the file doesn't exist, insert data from Excel into the database.
                        ExcelImportService.InsertExcelDataToDatabase(excelPath, context);
                        context.SaveChanges();
                        fileInDb = context.Files.FirstOrDefault(f => f.FileName == fileName);
                    }
                    // If the file is found or has just been added, get its Id.
                    var fileId = fileInDb?.Id;
                    if (fileId.HasValue)
                    {
                        // Open a window to display the data.
                        DataDisplayWindow dataDisplayWindow = new DataDisplayWindow(fileId.Value);
                        dataDisplayWindow.Show();
                    }
                }
            }
        }
    }
}