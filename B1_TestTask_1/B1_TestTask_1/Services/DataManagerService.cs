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
    public static class DataManagerService
    {
        private static readonly ThreadLocal<Random> _random = new ThreadLocal<Random>(() => new Random());
        private const string LatinAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string RussianAlphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя";
        private static readonly DateTime StartDate = DateTime.Now.AddYears(-5);
        private static readonly int DateRange = (DateTime.Now - StartDate).Days;

        // Generate a random string using Latin and Russian alphabets, and random numbers.
        public static string GenerateString()
        {
            var sb = new StringBuilder(64);

            // Generate random characters from the Latin alphabet.
            for (int i = 0; i < 10; i++)
            {
                sb.Append(LatinAlphabet[_random.Value.Next(LatinAlphabet.Length)]);
            }
            sb.Append("||");

            // Generate random characters from the Russian alphabet.
            for (int i = 0; i < 10; i++)
            {
                sb.Append(RussianAlphabet[_random.Value.Next(RussianAlphabet.Length)]);
            }
            sb.Append("||");

            // Generate a random positive even integer.
            int randomEvenIntegerNumber = _random.Value.Next(1, 50000000) * 2;
            sb.Append(randomEvenIntegerNumber);
            sb.Append("||");

            // Generate a random positive number with 8 decimal places.
            double randomDoubleNumber = Math.Round(_random.Value.NextDouble() * (20 - 1) + 1, 8);
            sb.Append(randomDoubleNumber);
            sb.Append("||");

            // Calculate a random date.
            DateTime randomDate = StartDate.AddDays(_random.Value.Next(DateRange));
            sb.Insert(0, $"{randomDate:dd.MM.yyyy}||");

            return sb.ToString();
        }

        // Asynchronously write strings to files in parallel using ConcurrentQueue.
        public static async Task WriteToFileLoopAsync(string path)
        {
            var queues = new ConcurrentQueue<string>[100];
            for (int i = 0; i < queues.Length; i++)
            {
                queues[i] = new ConcurrentQueue<string>();
            }

            // Generate strings and add them to the corresponding queues.
            Parallel.For(0, 10000000, j =>
            {
                string line = GenerateString();
                int fileIndex = j % 100;
                queues[fileIndex].Enqueue(line);
            });

            // Write strings from queues to files.
            var tasks = new Task[100];
            for (int i = 0; i < 100; i++)
            {
                int fileIndex = i;
                tasks[fileIndex] = Task.Run(async () =>
                {
                    string filePath = $"{path}test{fileIndex}.txt";
                    await WriteLinesToFileAsync(filePath, queues[fileIndex]);
                });
            }

            // Wait for all asynchronous tasks to complete.
            await Task.WhenAll(tasks);

            Console.WriteLine("Writing to files completed.");
        }

        // Merge files and remove lines containing a specific substring.
        public static async Task MergeAllFilesAsync(string path, string deleteStr)
        {
            var resultFilePath = Path.Combine(path, "result", "result.txt");
            int totalRemovedLines = 0;

            using (var output = File.Create(resultFilePath))
            {
                foreach (var txtFilePath in Directory.EnumerateFiles(path, "*.txt"))
                {
                    using (var input = File.OpenRead(txtFilePath))
                    using (var reader = new StreamReader(input))
                    using (var writer = new StreamWriter(output, leaveOpen: true))
                    {
                        string line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            if (!line.Contains(deleteStr))
                            {
                                await writer.WriteLineAsync(line);
                            }
                            else
                            {
                                totalRemovedLines++;
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"Total number of removed lines: {totalRemovedLines}");
        }

        // Import merged file to the database.
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
                    int batchSize = 1000;

                    using (StreamReader reader = new StreamReader(mergedFilePath))
                    {
                        string? line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            await ParseAndAddToDatabaseAsync(dbContext, line);

                            if (importedLines % batchSize == 0)
                            {
                                await dbContext.SaveChangesAsync();
                            }

                            importedLines++;
                            Console.WriteLine($"Imported {importedLines} of {totalLines} lines.");
                        }

                        await dbContext.SaveChangesAsync();
                    }

                    Console.WriteLine("Data imported to the database successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Parse a string and add data to the database asynchronously.
        private static async Task ParseAndAddToDatabaseAsync(AppDbContext dbContext, string line)
        {
            string[] dataParts = line.Split(new[] { "||" }, StringSplitOptions.None);

            DateTime date = DateTime.ParseExact(dataParts[0], "dd.MM.yyyy", CultureInfo.InvariantCulture);
            string latinChars = dataParts[1];
            string russianChars = dataParts[2];
            int integerValue = int.Parse(dataParts[3]);
            double doubleValue = double.Parse(dataParts[4]);

            var stringsData = new StringsData
            {
                DateColumn = date,
                LatinCharsColumn = latinChars,
                RussianCharsColumn = russianChars,
                IntegerColumn = integerValue,
                DoubleColumn = doubleValue
            };

            await dbContext.StringsData.AddAsync(stringsData);
        }

        // Process the results of calculations.
        public static async Task ProcessCalculationResult()
        {
            using (var dbContext = new AppDbContext())
            {
                var calculationResult = await dbContext.ExecuteCalculateSumAndMedian();

                long sumOfIntegers = calculationResult.SumOfIntegers;
                double medianOfDoubles = calculationResult.MedianOfDoubles;

                Console.WriteLine($"Sum of Integers: {sumOfIntegers}");
                Console.WriteLine($"Median of Doubles: {medianOfDoubles}");
            }
        }

        // Write lines from a ConcurrentQueue to a file asynchronously.
        private static async Task WriteLinesToFileAsync(string filePath, ConcurrentQueue<string> queue)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920, useAsync: true))
            using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
            {
                while (queue.TryDequeue(out string line))
                {
                    await writer.WriteLineAsync(line);
                }
            }
        }
    }
}
