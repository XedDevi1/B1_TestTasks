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
        // Основная точка входа в программу
        static async Task Main(string[] args)
        {
            const string path = "D:\\Тестовые\\TestTasks\\B1_TestTask_1\\ForFiles\\"; // Путь к директории с файлами

            // Цикл меню для выбора действий пользователем
            do
            {
                Console.WriteLine("Choose an option:");
                Console.WriteLine("1. Write to files");
                Console.WriteLine("2. Merge files");
                Console.WriteLine("3. Import to database");
                Console.WriteLine("4. Process calculation result");
                Console.WriteLine("5. Exit");

                string? input = Console.ReadLine(); // Чтение выбора пользователя

                // Обработка выбора пользователя
                switch (input)
                {
                    case "1":
                        await StringGeneratorService.WriteToFileLoopAsync(path); // Запись строк в файлы
                        break;
                    case "2":
                        string? inputString = Console.ReadLine(); // Чтение строки для удаления
                        await StringGeneratorService.MergeAllFiles(path, inputString); // Объединение файлов
                        break;
                    case "3":
                        await StringGeneratorService.ImportMergedFileToDatabase(path); // Импорт объединенного файла в базу данных
                        break;
                    case "4":
                        await StringGeneratorService.ProcessCalculationResult(); // Обработка результатов вычислений
                        break;
                    case "5":
                        Console.WriteLine("Exiting the program."); // Выход из программы
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please choose a valid option."); // Неверный выбор
                        break;
                }

            } while (Console.ReadLine() != "5"); // Повторять, пока пользователь не выберет "Выход"
        }  
    }
}
