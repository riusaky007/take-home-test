using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.Dtos;
using Fundo.Applications.WebApi.Models;
using Fundo.Applications.WebApi.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Fundo.Services.Tests.Unit
{
    public class LoanServiceTests
    {
        private static LoanDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<LoanDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new LoanDbContext(options);
        }

        private static LoanService CreateService(LoanDbContext context) =>
            new(context, NullLogger<LoanService>.Instance);

        [Fact]
        public async Task CreateAsync_SetsBalanceToAmountAndStatusActive()
        {
            using var context = CreateContext();
            var service = CreateService(context);

            var result = await service.CreateAsync(new CreateLoanRequest
            {
                Amount = 1000m,
                ApplicantName = "  Maria Silva  "
            });

            result.Amount.Should().Be(1000m);
            result.CurrentBalance.Should().Be(1000m);
            result.ApplicantName.Should().Be("Maria Silva");
            result.Status.Should().Be("active");

            (await context.Loans.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenLoanDoesNotExist()
        {
            using var context = CreateContext();
            var service = CreateService(context);

            var result = await service.GetByIdAsync(Guid.NewGuid());

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllLoans()
        {
            using var context = CreateContext();
            context.Loans.AddRange(
                new Loan { Id = Guid.NewGuid(), Amount = 100m, CurrentBalance = 100m, ApplicantName = "A", Status = LoanStatus.Active },
                new Loan { Id = Guid.NewGuid(), Amount = 200m, CurrentBalance = 0m, ApplicantName = "B", Status = LoanStatus.Paid });
            await context.SaveChangesAsync();
            var service = CreateService(context);

            var result = await service.GetAllAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task ApplyPaymentAsync_DeductsFromBalance()
        {
            using var context = CreateContext();
            var loan = new Loan { Id = Guid.NewGuid(), Amount = 1000m, CurrentBalance = 1000m, ApplicantName = "A", Status = LoanStatus.Active };
            context.Loans.Add(loan);
            await context.SaveChangesAsync();
            var service = CreateService(context);

            var result = await service.ApplyPaymentAsync(loan.Id, new PaymentRequest { Amount = 250m });

            result.CurrentBalance.Should().Be(750m);
            result.Status.Should().Be("active");
        }

        [Fact]
        public async Task ApplyPaymentAsync_MarksLoanPaid_WhenBalanceReachesZero()
        {
            using var context = CreateContext();
            var loan = new Loan { Id = Guid.NewGuid(), Amount = 500m, CurrentBalance = 500m, ApplicantName = "A", Status = LoanStatus.Active };
            context.Loans.Add(loan);
            await context.SaveChangesAsync();
            var service = CreateService(context);

            var result = await service.ApplyPaymentAsync(loan.Id, new PaymentRequest { Amount = 500m });

            result.CurrentBalance.Should().Be(0m);
            result.Status.Should().Be("paid");
        }

        [Fact]
        public async Task ApplyPaymentAsync_Throws_WhenPaymentExceedsBalance()
        {
            using var context = CreateContext();
            var loan = new Loan { Id = Guid.NewGuid(), Amount = 500m, CurrentBalance = 300m, ApplicantName = "A", Status = LoanStatus.Active };
            context.Loans.Add(loan);
            await context.SaveChangesAsync();
            var service = CreateService(context);

            var act = () => service.ApplyPaymentAsync(loan.Id, new PaymentRequest { Amount = 400m });

            await act.Should().ThrowAsync<InvalidPaymentException>();
        }

        [Fact]
        public async Task ApplyPaymentAsync_Throws_WhenLoanAlreadyPaid()
        {
            using var context = CreateContext();
            var loan = new Loan { Id = Guid.NewGuid(), Amount = 500m, CurrentBalance = 0m, ApplicantName = "A", Status = LoanStatus.Paid };
            context.Loans.Add(loan);
            await context.SaveChangesAsync();
            var service = CreateService(context);

            var act = () => service.ApplyPaymentAsync(loan.Id, new PaymentRequest { Amount = 50m });

            await act.Should().ThrowAsync<InvalidPaymentException>();
        }

        [Fact]
        public async Task ApplyPaymentAsync_Throws_WhenLoanNotFound()
        {
            using var context = CreateContext();
            var service = CreateService(context);

            var act = () => service.ApplyPaymentAsync(Guid.NewGuid(), new PaymentRequest { Amount = 50m });

            await act.Should().ThrowAsync<LoanNotFoundException>();
        }
    }
}
