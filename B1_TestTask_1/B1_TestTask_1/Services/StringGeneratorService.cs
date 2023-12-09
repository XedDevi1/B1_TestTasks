using B1_TestTask_1.Models;
using B1_TestTask_1.Persistence;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1_TestTask_1.Services
{
    public static class StringGeneratorService
    {
        private static readonly ThreadLocal<Random> _random = new ThreadLocal<Random>(() => new Random());
        private const string LatinAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string RussianAlphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя";
        private static readonly DateTime StartDate = DateTime.Now.AddYears(-5);
        private static readonly int DateRange = (DateTime.Now - StartDate).Days;

        // Генерация случайной строки с использованием латинского и русского алфавитов
        public static string GenerateString()
        {
            var sb = new StringBuilder(64); // Предполагаемая длина строки

            // Генерация случайных символов из латинского алфавита
            for (int i = 0; i < 10; i++)
            {
                sb.Append(LatinAlphabet[_random.Value.Next(LatinAlphabet.Length)]);
            }
            sb.Append("||");

            // Генерация случайных символов из русского алфавита
            for (int i = 0; i < 10; i++)
            {
                sb.Append(RussianAlphabet[_random.Value.Next(RussianAlphabet.Length)]);
            }
            sb.Append("||");

            // Генерация случайного положительного четного целочисленного числа
            int randomEvenIntegerNumber = _random.Value.Next(1, 50000000) * 2;
            sb.Append(randomEvenIntegerNumber);
            sb.Append("||");

            // Генерация случайного положительного числа с 8 знаками после запятой
            double randomDoubleNumber = Math.Round(_random.Value.NextDouble() * (20 - 1) + 1, 8);
            sb.Append(randomDoubleNumber);
            sb.Append("||");

            // Вычисление случайной даты
            DateTime randomDate = StartDate.AddDays(_random.Value.Next(DateRange));
            sb.Insert(0, $"{randomDate:dd.MM.yyyy}||");

            return sb.ToString();
        }

        // Асинхронная запись строк в файлы в параллельном режиме с использованием ConcurrentQueue
        public static async Task WriteToFileLoopAsync(string path)
        {
            var queues = new ConcurrentQueue<string>[100];
            for (int i = 0; i < queues.Length; i++)
            {
                queues[i] = new ConcurrentQueue<string>();
            }

            // Генерация строк и добавление их в соответствующие очереди
            Parallel.For(0, 10000000, j =>
            {
                string line = StringGeneratorService.GenerateString();
                int fileIndex = j % 100;
                queues[fileIndex].Enqueue(line);
            });

            // Запись строк из очередей в файлы
            var tasks = new Task[100];
            for (int i = 0; i < 100; i++)
            {
                int fileIndex = i;
                tasks[fileIndex] = Task.Run(async () =>
                {
                    string filePath = $"{path}test{fileIndex}.txt";
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true))
                    using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
                    {
                        while (queues[fileIndex].TryDequeue(out string line))
                        {
                            await writer.WriteLineAsync(line);
                        }
                    }
                });
            }

            // Ожидаем завершения всех асинхронных задач
            await Task.WhenAll(tasks);

            Console.WriteLine("Writing to files completed."); // Сообщение о завершении записи
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

        public static async Task ImportMergedFileToDatabase(string path)
        {
            try
            {
                string mergedFilePath = Path.Combine(path, "result", "result.txt");
                using (var dbContext = new AppDbContext())
                {
                    dbContext.ChangeTracker.AutoDetectChangesEnabled = false;

                    int totalLines = File.ReadLines(mergedFilePath).Count();
                    int importedLines = 0;
                    int batchSize = 1000; // Размер пакета для пакетной обработки
                    List<Task> importTasks = new List<Task>();

                    using (StreamReader reader = new StreamReader(mergedFilePath))
                    {
                        string? line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            importTasks.Add(ParseAndAddToDatabaseAsync(dbContext, line));

                            if (importTasks.Count >= batchSize)
                            {
                                await Task.WhenAll(importTasks);
                                await dbContext.SaveChangesAsync();
                                importTasks.Clear();
                            }

                            importedLines++;
                            Console.WriteLine($"Imported {importedLines} of {totalLines} lines.");
                        }

                        if (importTasks.Count > 0)
                        {
                            await Task.WhenAll(importTasks);
                            await dbContext.SaveChangesAsync();
                        }
                    }

                    Console.WriteLine("Data imported to the database successfully.");
                }
            }
            catch (Exception ex)
            {
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
