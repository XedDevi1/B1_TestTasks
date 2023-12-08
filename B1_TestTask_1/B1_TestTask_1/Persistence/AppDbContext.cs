using B1_TestTask_1.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1_TestTask_1.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<StringsData> StringsData { get; set; }
        public DbSet<CalculationResult> CalculationResults { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS01;Initial Catalog=testTask_1;Trusted_Connection=True;TrustServerCertificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CalculationResult>().HasNoKey();
        }

        public async Task<CalculationResult> ExecuteCalculateSumAndMedian()
        {
            var result = await CalculationResults
                .FromSqlRaw("EXEC CalculateSumAndMedian")
                .AsNoTracking()
                .ToListAsync();

            return result.FirstOrDefault();
        }
    }
}
