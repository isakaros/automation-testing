using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;

namespace Customers.Api.Tests.Integrations.CustomerController
{
    //[CollectionDefinition("CustomerApi Collection")]
    public class CreateCustomerControllerTests: IClassFixture<CustomerApiFactory>, IAsyncLifetime
    {
        private readonly HttpClient _client;

        private readonly Faker<CustomerRequest> _customerGenerator =
            new Faker<CustomerRequest>()
                .RuleFor(x => x.FullName, faker => faker.Person.FullName)
                .RuleFor(x => x.Email, faker => faker.Person.Email)
                .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGithubUser)
                .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);

        public CreateCustomerControllerTests(CustomerApiFactory apiFactory)
        {
            _client = apiFactory.CreateClient();
        }

        private readonly List<Guid> _createdIds = new();

        [Fact]
        public async Task Create_CreateUser_WhenDataIsValid()
        {
            //Arrange
            var customer = _customerGenerator.Generate();

            //Act
            var response = await _client.PostAsJsonAsync("customers", customer);

            //Assert
            var customerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
            customerResponse.Should().BeEquivalentTo(customer);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location!.ToString().Should().Be($"http://localhost/customers/{customerResponse!.Id}");

            _createdIds.Add(customerResponse.Id);
        }

        [Fact]
        public async Task Create_ReturnsValidationError_WhenEmailIsInvalid()
        {
            //Arrange
            const string invalidEmail = "dadasdasda";
            var customer = _customerGenerator.Clone()
                .RuleFor(x => x.Email, invalidEmail).Generate();

            //Act
            var response = await _client.PostAsJsonAsync("customers", customer);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            error!.Status.Should().Be(400);
            error.Title.Should().Contain("One or more");
            error.Errors["Email"][0].Should().Be($"{invalidEmail} is not a valid email address");
        }

        [Fact]
        public async Task Create_ReturnsValidationError_WhenGitHubUserIsInvalid()
        {
            //Arrange
            const string invalidGitHubUser = "dadasdasda";
            var customer = _customerGenerator.Clone()
                .RuleFor(x => x.GitHubUsername, invalidGitHubUser).Generate();

            //Act
            var response = await _client.PostAsJsonAsync("customers", customer);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            error!.Status.Should().Be(400);
            error.Title.Should().Contain("One or more");
            error.Errors["Customer"][0].Should().Contain($"{invalidGitHubUser}");
        }

        [Fact]
        public async Task Create_ReturnsInternalServerError_WhenGutHubIsThrottled()
        {
            //Arrange
            var customer = _customerGenerator.Clone()
                .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ThrottledGithubUser).Generate();

            //Act
            var response = await _client.PostAsJsonAsync("customers", customer);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact(Skip = "test")]
        public async Task Create_ReturnsCreated_WhenCustomerIsCreated()
        {
            //Arrange
            var customer = _customerGenerator.Generate();

            //Act
            var response = await _client.PostAsJsonAsync("customers", customer);

            //Assert
            var customerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
            customerResponse.Should().BeEquivalentTo(customer);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            _createdIds.Add(customerResponse.Id);
        }

        [Fact(Skip = "test")]
        public async Task Test()
        {
            await Task.Delay(5000);
        }

        public Task InitializeAsync() => Task.CompletedTask;


        public async Task DisposeAsync()
        {
            foreach (var createdId in _createdIds)
            {
                await _client.DeleteAsync($"customers/{createdId}");
            }
        }

        //[MemberData(nameof(Data))]
        /*public static IEnumerable<object[]> Data { get; } = new[]
        {
            new []{"09EF07F3-DA1E-48EA-B707-D1E2DBEF8039"},
            new []{"19EF07F3-DA1E-48EA-B707-D1E2DBEF8039"},
            new []{"29EF07F3-DA1E-48EA-B707-D1E2DBEF8039"}

        };*/

        //[ClassData(typeof(ClassData))]
        /*public class ClassData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { "09EF07F3-DA1E-48EA-B707-D1E2DBEF8039" };
                yield return new object[] { "19EF07F3-DA1E-48EA-B707-D1E2DBEF8039" };
                yield return new object[] { "29EF07F3-DA1E-48EA-B707-D1E2DBEF8039" };
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }*/
    }
}
