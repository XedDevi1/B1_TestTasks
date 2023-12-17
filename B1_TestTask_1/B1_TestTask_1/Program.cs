using B1_TestTask_1.Models;
using B1_TestTask_1.Persistence;
using B1_TestTask_1.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;

namespace B1_TestTask_1
{
    internal class Program
    {
        private const string FilesDirectoryPath = "D:\\Тестовые\\TestTasks\\B1_TestTask_1\\ForFiles\\";

        static async Task Main(string[] args)
        {
            // Main menu loop for user actions
            do
            {
                PrintMenuOptions(); // Display menu options

                string? userInput = Console.ReadLine(); // Read user input

                // Process user input
                switch (userInput)
                {
                    case "1":
                        await WriteToFileOption(); // Write strings to files
                        break;
                    case "2":
                        await MergeFilesOption(); // Merge files
                        break;
                    case "3":
                        await ImportToDatabaseOption(); // Import to database
                        break;
                    case "4":
                        await ProcessCalculationResultOption(); // Process calculation results
                        break;
                    case "5":
                        Console.WriteLine("Exiting the program."); // Exit the program
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please choose a valid option."); // Invalid option
                        break;
                }

            } while (Console.ReadLine() != "5"); // Repeat until the user chooses "Exit"
        }

        private static void PrintMenuOptions()
        {
            Console.WriteLine("Choose an option:");
            Console.WriteLine("1. Write to files");
            Console.WriteLine("2. Merge files");
            Console.WriteLine("3. Import to database");
            Console.WriteLine("4. Process calculation result");
            Console.WriteLine("5. Exit");
        }

        private static async Task WriteToFileOption()
        {
            await DataManagerService.WriteToFileLoopAsync(FilesDirectoryPath); // Write strings to files
        }

        private static async Task MergeFilesOption()
        {
            Console.WriteLine("Enter the substring to delete:");
            string? inputString = Console.ReadLine(); // Read the string to delete
            await DataManagerService.MergeAllFilesAsync(FilesDirectoryPath, inputString); // Merge files
        }

        private static async Task ImportToDatabaseOption()
        {
            await DataManagerService.ImportMergedFileToDatabase(FilesDirectoryPath); // Import merged file to the database
        }

        private static async Task ProcessCalculationResultOption()
        {
            await DataManagerService.ProcessCalculationResult(); // Process calculation results
        }
    }
}
