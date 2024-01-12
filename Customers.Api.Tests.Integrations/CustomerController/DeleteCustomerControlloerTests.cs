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
    public class DeleteCustomerControlloerTests : IClassFixture<CustomerApiFactory>
    {
        private readonly HttpClient _client;

        private readonly Faker<CustomerRequest> _customerGenerator =
            new Faker<CustomerRequest>()
                .RuleFor(x => x.FullName, faker => faker.Person.FullName)
                .RuleFor(x => x.Email, faker => faker.Person.Email)
                .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGithubUser)
                .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);

        public DeleteCustomerControlloerTests(CustomerApiFactory apiFactory)
        {
            _client = apiFactory.CreateClient();
        }

        [Fact]
        public async Task Delete_ReturnsOk_WhenCustomerExists()
        {
            //Arrange
            var customer = _customerGenerator.Generate();
            var createdResponse = await _client.PostAsJsonAsync("customers", customer);
            var createdCustomer = await createdResponse.Content.ReadFromJsonAsync<CustomerResponse>();

            //Act
            var response = await _client.DeleteAsync($"customers/{createdCustomer!.Id}");

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            //Act
            var response = await _client.GetAsync($"customers/{Guid.NewGuid()}");

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            //assert response body
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            problem.Title.Should().Be("Not Found");
            problem.Status.Should().Be(404);
            
        }
    }
}
