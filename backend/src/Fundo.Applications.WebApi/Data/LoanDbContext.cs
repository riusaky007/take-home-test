using Fundo.Applications.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Applications.WebApi.Data
{
    public class LoanDbContext : DbContext
    {
        public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options)
        {
        }

        public DbSet<Loan> Loans => Set<Loan>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Loan>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.Amount).HasColumnType("decimal(18,2)");
                entity.Property(l => l.CurrentBalance).HasColumnType("decimal(18,2)");
                entity.Property(l => l.ApplicantName).IsRequired().HasMaxLength(200);
                entity.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
            });

            modelBuilder.Entity<Loan>().HasData(
                new Loan
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Amount = 25000.00m,
                    CurrentBalance = 18750.00m,
                    ApplicantName = "John Doe",
                    Status = LoanStatus.Active
                },
                new Loan
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Amount = 15000.00m,
                    CurrentBalance = 0m,
                    ApplicantName = "Jane Smith",
                    Status = LoanStatus.Paid
                },
                new Loan
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Amount = 50000.00m,
                    CurrentBalance = 32500.00m,
                    ApplicantName = "Robert Johnson",
                    Status = LoanStatus.Active
                });
        }
    }
}
