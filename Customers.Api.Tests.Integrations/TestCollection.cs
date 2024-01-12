using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Customers.Api.Tests.Integrations.CustomerController
{
    [CollectionDefinition("CustomerApi Collection")]
    public class TestCollection : ICollectionFixture<WebApplicationFactory<IApiMarker>>
    {
    }
}
