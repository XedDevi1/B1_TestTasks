using B1_TestTask_2.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace B1_TestTask_2.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<AccountDetails> AccountDetails { get; set; }
        public DbSet<Classes> Classes { get; set; }
        public DbSet<Accounts> Accounts { get; set; }
        public DbSet<AccountGroups> AccountGroups { get; set; }
        public DbSet<Files> Files { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountDetails>().HasKey(ad => ad.Id);
            modelBuilder.Entity<AccountGroups>().HasKey(ag => ag.Id);
            modelBuilder.Entity<Accounts>().HasKey(a => a.Id);
            modelBuilder.Entity<Classes>().HasKey(c => c.ClassNumber);
            modelBuilder.Entity<Files>().HasKey(f => f.Id);

            modelBuilder.Entity<Accounts>()
                .HasOne(a => a.AccountDetails)
                .WithOne(ad => ad.Account)
                .HasForeignKey<AccountDetails>(ad => ad.Id);

            modelBuilder.Entity<AccountGroups>()
                .HasMany(ag => ag.Accounts)
                .WithOne(a => a.AccountGroups)
                .HasForeignKey(a => a.AccountGroupId);

            modelBuilder.Entity<Classes>()
                .HasMany(c => c.Accounts)
                .WithOne(a => a.Class)
                .HasForeignKey(a => a.ClassId);

            modelBuilder.Entity<Files>()
                .HasMany(c => c.Classes)
                .WithOne(a => a.File)
                .HasForeignKey(a => a.FileId);

            modelBuilder.Entity<AccountDetails>()
                .Property(ad => ad.ActiveOpeningBalance)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<AccountDetails>()
                .Property(ad => ad.ActiveClosingBalance)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<AccountDetails>()
                .Property(ad => ad.DebitTurnover)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<AccountDetails>()
                .Property(ad => ad.LoanTurnover)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<AccountDetails>()
                .Property(ad => ad.PassiveOpeningBalance)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<AccountDetails>()
                .Property(ad => ad.PassiveClosingBalance)
                .HasColumnType("decimal(18, 2)");

            base.OnModelCreating(modelBuilder);
        }
    }
}
