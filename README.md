# RAIT: Strongly Typed REST API Testing Library

RAIT (REST API Integration Testing) is an open-source library designed to simplify and streamline the process of testing REST APIs. By leveraging expression trees, RAIT allows you to make strongly typed action calls that are translated into HTTP requests. This ensures type safety and reduces the likelihood of errors in your tests.

## Features:
- **Strongly Typed Action Calls**: Use C# expression trees to define your API calls.
- **Type Safety**: Compile-time checking of your API calls to ensure correctness.
- **Ease of Use**: Simple and intuitive API for making HTTP requests.
- **Enhanced Navigation**: Since method calls are strongly typed, your IDE can assist in navigating directly to the action from the test. This makes it easy to move between your tests and the corresponding actions.
- **Test Coverage Visibility**: Your IDE will indicate that an action is covered by a test because the action will not be marked as unused. This helps you quickly understand which actions are tested and which are not.

## Getting Started:
### Installation
To install RAIT, you can add it to your project via NuGet Package Manager
```
> Install-Package RAIT
``` 

###  Basic Usage
Below is a step-by-step guide on how to use RAIT to test your REST APIs.

1.**Define Your API Controller**
First, you need to define your API controller with the actions you want to test. For example:

```csharp
public class RaitTestController
{
   [HttpPost]
   public async Task<IActionResult> Post([FromBody] YourModel model)
   {
       // Your action logic here
   }
}
```

2. **Create an Instance of HttpClient Using WebApplicationFactory**
It is recommended to use `WebApplicationFactory` from `Microsoft.AspNetCore.Mvc.Testing` to create an instance of `HttpClient`. This ensures that your tests run in a realistic environment similar to production.

```csharp
using Microsoft.AspNetCore.Mvc.Testing;

public class YourTestClass : IClassFixture<WebApplicationFactory<Startup>>
{
   private readonly HttpClient _httpClient;

   public YourTestClass(WebApplicationFactory<Startup> factory)
   {
       _httpClient = factory.CreateClient();
   }

   // Your test methods here
}
```

3. **Make a Strongly Typed API Call**
Use the `Rait` extension method to make a strongly typed API call. The following code demonstrates how to call the `Post` action of the `RaitTestController`:

```csharp
var responseModel = await _httpClient.Rait<RaitTestController>().Call(n => n.Post(model));
```
- `_httpClient`: An instance of `HttpClient`.
- `Rait<RaitTestController>()`: Specifies the controller you want to test.
- `Call(n => n.Post(model))`: Defines the action you want to call and passes the necessary parameters.

### Example
Here is a complete example demonstrating how to use RAIT in a test:

```csharp
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class RaitTests : IClassFixture<WebApplicationFactory<Startup>>
{
    private readonly HttpClient _httpClient;

    public RaitTests(WebApplicationFactory<Startup> factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task TestPostAction()
    {
        var model = new YourModel
        {
            // Initialize your model properties here
        };

        var responseModel = await _httpClient.Rait<RaitTestController>().Call(n => n.Post(model));

        // Assert the response
        Assert.NotNull(responseModel);
        // Additional assertions based on your requirements
    }
}
```
## Contributing
We welcome contributions to RAIT! If you find a bug or have a feature request, please open an issue on GitHub. If you would like to contribute code, please fork the repository and submit a pull request.

## License
RAIT is licensed under the MIT License.
