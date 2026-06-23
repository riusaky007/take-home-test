using System.Net;
using System.Net.Http.Json;
using Fundo.Applications.WebApi.Dtos;
using FluentAssertions;
using Xunit;

namespace Fundo.Services.Tests.Integration
{
    public class LoanManagementControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public LoanManagementControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task GetAll_ReturnsSeededLoans()
        {
            var response = await _client.GetAsync("/loans");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var loans = await response.Content.ReadFromJsonAsync<List<LoanResponse>>();
            loans.Should().NotBeNull();
            loans!.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_ForUnknownId()
        {
            var response = await _client.GetAsync($"/loans/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_ThenGetById_ReturnsCreatedLoan()
        {
            var createResponse = await _client.PostAsJsonAsync("/loans", new CreateLoanRequest
            {
                Amount = 1200m,
                ApplicantName = "Maria Silva"
            });

            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await createResponse.Content.ReadFromJsonAsync<LoanResponse>();
            created.Should().NotBeNull();
            created!.CurrentBalance.Should().Be(1200m);
            created.Status.Should().Be("active");

            var getResponse = await _client.GetAsync($"/loans/{created.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_ForInvalidPayload()
        {
            var response = await _client.PostAsJsonAsync("/loans", new CreateLoanRequest
            {
                Amount = 0m,
                ApplicantName = ""
            });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Payment_DeductsBalance_AndMarksPaidWhenZero()
        {
            var create = await _client.PostAsJsonAsync("/loans", new CreateLoanRequest
            {
                Amount = 500m,
                ApplicantName = "Payment Tester"
            });
            var loan = await create.Content.ReadFromJsonAsync<LoanResponse>();

            var partial = await _client.PostAsJsonAsync($"/loans/{loan!.Id}/payment", new PaymentRequest { Amount = 200m });
            partial.StatusCode.Should().Be(HttpStatusCode.OK);
            var afterPartial = await partial.Content.ReadFromJsonAsync<LoanResponse>();
            afterPartial!.CurrentBalance.Should().Be(300m);
            afterPartial.Status.Should().Be("active");

            var final = await _client.PostAsJsonAsync($"/loans/{loan.Id}/payment", new PaymentRequest { Amount = 300m });
            final.StatusCode.Should().Be(HttpStatusCode.OK);
            var afterFinal = await final.Content.ReadFromJsonAsync<LoanResponse>();
            afterFinal!.CurrentBalance.Should().Be(0m);
            afterFinal.Status.Should().Be("paid");
        }

        [Fact]
        public async Task Payment_ReturnsBadRequest_WhenExceedingBalance()
        {
            var create = await _client.PostAsJsonAsync("/loans", new CreateLoanRequest
            {
                Amount = 100m,
                ApplicantName = "Overpay Tester"
            });
            var loan = await create.Content.ReadFromJsonAsync<LoanResponse>();

            var response = await _client.PostAsJsonAsync($"/loans/{loan!.Id}/payment", new PaymentRequest { Amount = 500m });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Payment_ReturnsNotFound_ForUnknownLoan()
        {
            var response = await _client.PostAsJsonAsync($"/loans/{Guid.NewGuid()}/payment", new PaymentRequest { Amount = 10m });

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
