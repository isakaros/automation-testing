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
    public class GetCustomerControllerTests: IClassFixture<CustomerApiFactory>
    {
        private readonly HttpClient _client;

        private readonly Faker<CustomerRequest> _customerGenerator =
            new Faker<CustomerRequest>()
                .RuleFor(x => x.FullName, faker => faker.Person.FullName)
                .RuleFor(x => x.Email, faker => faker.Person.Email)
                .RuleFor(x => x.GitHubUsername, CustomerApiFactory.ValidGithubUser)
                .RuleFor(x => x.DateOfBirth, faker => faker.Person.DateOfBirth.Date);

        public GetCustomerControllerTests(CustomerApiFactory apiFactory)
        {
            _client = apiFactory.CreateClient();
        }

        [Fact]
        public async Task Get_ReturnsCustomer_WhenCustomerExists()
        {
            //Arrange
            var customer = _customerGenerator.Generate();
            var createdResponse = await _client.PostAsJsonAsync("customers", customer);
            var createdCustomer = await createdResponse.Content.ReadFromJsonAsync<CustomerResponse>();

            //Act
            var response = await _client.GetAsync($"customers/{createdCustomer!.Id}");

            //Assert
            var retrievedCustomer = await response.Content.ReadFromJsonAsync<CustomerResponse>();
            retrievedCustomer.Should().BeEquivalentTo(createdCustomer);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenCustomerDoesNotExist()
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
