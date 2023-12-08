using B1_TestTask_1.Models;
using B1_TestTask_1.Persistence;
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
            var start = DateTime.Now; // Запоминаем время начала выполнения программы
            const string path = "D:\\Тестовые\\B1_TestTask_1\\ForFiles\\"; // Путь к директории с файлами

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
                        WriteToFileLoop(path); // Запись строк в файлы
                        break;
                    case "2":
                        string? inputString = Console.ReadLine(); // Чтение строки для удаления
                        await MergeAllFiles(path, inputString); // Объединение файлов
                        break;
                    case "3":
                        await ImportMergedFileToDatabase(path); // Импорт объединенного файла в базу данных
                        break;
                    case "4":
                        await ProcessCalculationResult(); // Обработка результатов вычислений
                        break;
                    case "5":
                        Console.WriteLine("Exiting the program."); // Выход из программы
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please choose a valid option."); // Неверный выбор
                        break;
                }

            } while (Console.ReadLine() != "5"); // Повторять, пока пользователь не выберет "Выход"

            var finished = DateTime.Now - start; // Расчет общего времени выполнения
            Console.WriteLine($"Total execution time: {finished}");
        }

        // Генерация случайной строки с использованием латинского и русского алфавитов
        public static string GenerateString()
        {
            // Определение алфавитов
            const string _latinAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            const string _russianAlphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя";

            // Инициализация StringBuilder для оптимизации работы со строками
            StringBuilder tenRandomLatinChars = new StringBuilder(10);
            StringBuilder tenRandomRussianChars = new StringBuilder(10);

            // Переменные для случайных чисел
            int randomIntegerNumber;
            double randomDoubleNumber;
            Random random = new Random();

            // Вычисление даты пять лет назад от текущего момента
            DateTime fiveYearsAgo = DateTime.Now.AddYears(-5);
            int range = (DateTime.Now - fiveYearsAgo).Days;
            DateTime randomDate = fiveYearsAgo.AddDays(random.Next(range));

            // Генерация случайных символов из алфавитов
            for (int i = 0; i < 10; i++)
            {
                tenRandomLatinChars.Append(_latinAlphabet[random.Next(_latinAlphabet.Length)]);
            }
            for (int i = 0; i < 10; i++)
            {
                tenRandomRussianChars.Append(_russianAlphabet[random.Next(_russianAlphabet.Length)]);
            }

            // Генерация случайных чисел
            randomIntegerNumber = random.Next(1, 50000000) * 2;
            randomDoubleNumber = Math.Round(random.NextDouble() * (20 - 1) + 1, 8);

            // Формирование итоговой строки
            return randomDate.ToString("dd.MM.yyyy") + "||"
                + tenRandomLatinChars.ToString() + "||"
                + tenRandomRussianChars.ToString() + "||"
                + randomIntegerNumber.ToString() + "||"
                + randomDoubleNumber.ToString() + "||";
        }

        // Запись строк в файлы в параллельном режиме
        public static void WriteToFileLoop(string path)
        {
            Parallel.For(0, 100, i =>
            {
                WriteToFile(path, i);
            });

            Console.WriteLine("Writing to files completed."); // Сообщение о завершении записи
        }

        // Запись строк в отдельный файл
        public static void WriteToFile(string path, int i)
        {
            using (StreamWriter writer = new StreamWriter($"{path}test{i}.txt"))
            {
                for (int j = 0; j < 1000; j++)
                {
                    writer.WriteLine(GenerateString());
                }
            }
        }

        // Объединение файлов и удаление строк, содержащих определенную подстроку
        public static async Task MergeAllFiles(string path, string deleteStr)
        {
            var txtFiles = Directory.EnumerateFiles(path, "*.txt"); // Получение списка текстовых файлов
            int totalRemovedLines = 0; // Счетчик удаленных строк

            // Создание результирующего файла
            using (var output = File.Create(path + "result\\result.txt"))
            {
                foreach (var txtFile in txtFiles)
                {
                    var lines = File.ReadAllLines(txtFile); // Чтение строк из файла
                    var newLines = lines.Where(line => !line.Contains(deleteStr)).ToArray(); // Удаление строк с подстрокой
                    totalRemovedLines += lines.Length - newLines.Length; // Обновление счетчика удаленных строк
                    File.WriteAllLines(txtFile, newLines); // Перезапись файла без удаленных строк
                    using (var input = File.OpenRead(txtFile))
                    {
                        await input.CopyToAsync(output); // Копирование содержимого файла в результирующий файл
                    }
                }
            }
            Console.WriteLine($"Общее количество удаленных строк: {totalRemovedLines}"); // Вывод количества удаленных строк
        }

        // Метод для импорта объединенного файла в базу данных
        public static async Task ImportMergedFileToDatabase(string path)
        {
            try
            {
                // Путь к объединенному файлу
                string mergedFilePath = Path.Combine(path, "result", "result.txt");

                // Использование контекста базы данных
                using (var dbContext = new AppDbContext())
                {
                    // Убедиться, что база данных создана
                    await dbContext.Database.EnsureCreatedAsync();

                    // Подсчет общего количества строк в файле
                    int totalLines = File.ReadLines(mergedFilePath).Count();
                    int importedLines = 0;

                    // Чтение файла построчно
                    using (StreamReader reader = new StreamReader(mergedFilePath))
                    {
                        List<Task> importTasks = new List<Task>();

                        string? line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            // Добавление задачи импорта строки в базу данных
                            importTasks.Add(ParseAndAddToDatabaseAsync(dbContext, line));

                            // Отображение прогресса импорта
                            importedLines++;
                            Console.WriteLine($"Imported {importedLines} of {totalLines} lines.");
                        }

                        // Ожидание завершения всех задач импорта
                        await Task.WhenAll(importTasks);
                        // Сохранение изменений в базе данных
                        await dbContext.SaveChangesAsync();
                    }

                    // Сообщение об успешном импорте данных
                    Console.WriteLine("Data imported to the database successfully.");
                }
            }
            catch (Exception ex)
            {
                // Вывод сообщения об ошибке
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Метод для разбора строки и добавления данных в базу данных
        private static async Task ParseAndAddToDatabaseAsync(AppDbContext dbContext, string line)
        {
            // Разбиение строки на части
            string[] dataParts = line.Split(new[] { "||" }, StringSplitOptions.None);

            // Преобразование данных из строки в соответствующие типы
            DateTime date = DateTime.ParseExact(dataParts[0], "dd.MM.yyyy", CultureInfo.InvariantCulture);
            string latinChars = dataParts[1];
            string russianChars = dataParts[2];
            int integerValue = int.Parse(dataParts[3]);
            double doubleValue = double.Parse(dataParts[4]);

            // Создание объекта для добавления в базу данных
            var stringsData = new StringsData
            {
                DateColumn = date,
                LatinCharsColumn = latinChars,
                RussianCharsColumn = russianChars,
                IntegerColumn = integerValue,
                DoubleColumn = doubleValue
            };

            // Асинхронное добавление объекта в базу данных
            await dbContext.StringsData.AddAsync(stringsData);
        }

        // Метод для обработки результатов вычислений
        public static async Task ProcessCalculationResult()
        {
            // Использование контекста базы данных
            using (var dbContext = new AppDbContext())
            {
                // Выполнение вычислений в базе данных
                var calculationResult = await dbContext.ExecuteCalculateSumAndMedian();

                // Получение результатов вычислений
                long sumOfIntegers = calculationResult.SumOfIntegers;
                double medianOfDoubles = calculationResult.MedianOfDoubles;

                // Вывод результатов вычислений
                Console.WriteLine($"Sum of Integers: {sumOfIntegers}");
                Console.WriteLine($"Median of Doubles: {medianOfDoubles}");
            }
        }
    }
}
