# RAIT: Strongly Typed REST API Testing Library

RAIT (REST API Integration Testing) is an open-source library designed to simplify and streamline the process of testing REST APIs. By leveraging expression trees, RAIT allows you to make strongly typed action calls that are translated into HTTP requests. This ensures type safety and reduces the likelihood of errors in your tests.

> [!IMPORTANT]
> **`RAIT` operates via `HttpClient` and works seamlessly with `WebApplicationFactory< T >`.**

## Features:
- **Strongly Typed Action Calls**: Use C# expression trees to define your API calls.
- **Type Safety**: Compile-time checking of your API calls to ensure correctness.
- **Ease of Use**: Simple and intuitive API for making HTTP requests.
- **Enhanced Navigation**: Since method calls are strongly typed, your IDE can assist in navigating directly to the action from the test. This makes it easy to move between your tests and the corresponding actions.
- **Test Coverage Visibility**: Your IDE will indicate that an action is covered by a test because the action will not be marked as unused. This helps you quickly understand which actions are tested and which are not.
-  Possibility to move to the library gradually, without throwing away existing tests
- **Experimental Feature**: Test to Examples Documentation: Automatically generate example values for Swagger documentation based on your test code. This feature extracts XML documentation and adds example values for models used in your tests.
- **Experimental Feature**: Ardalis.ApiEndpoints (only for net8.0)

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

2. **Create an Instance of HttpClient**
You can use `WebApplicationFactory` from `Microsoft.AspNetCore.Mvc.Testing` to create an instance of `HttpClient`.

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

### **Serialization Settings**
By default, RAIT uses `System.Text.Json` for serialization and deserialization of HTTP request and response bodies. If your server uses custom `SerializerSettings` or you prefer to use `Newtonsoft.Json`, you need to register RAIT in the dependency injection (DI) container and configure it accordingly.

#### **Custom Serialization Configuration**
1.**Register RAIT in DI**
First, add RAIT to the services collection in your test environment setup. This ensures that RAIT is properly registered and can be configured with custom serialization settings.
```csharp
private void PrepareEnv(IWebHostBuilder builder)
{
    builder.UseEnvironment("Test");
    builder.ConfigureTestServices(services => services.AddRait());
}
```
2.**Configure RAIT with Custom Serialization Settings**
After registering RAIT, call the `ConfigureRait` extension method on the service provider to apply your custom serialization settings.
```csharp
[SetUp]
public void Setup()
{
    _application = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(PrepareEnv);

    _defaultClient = _application.CreateDefaultClient();
    _application.Services.ConfigureRait();
}
```

### Experimental Feature: Test to Examples Documentation
RAIT includes an experimental feature that generates example values for Swagger documentation based on your test code. This feature extracts XML documentation and adds example values for models used in your tests.

#### How to Enable Test to Examples Documentation
1. **Add XML Documentation to Swagger Configuration**
Modify your Swagger configuration to include RAIT XML documentation:
```csharp
builder.Services.AddSwaggerGen(swaggerGenOptions =>
{
   // your swagger configuration code here 

    swaggerGenOptions.IncludeRaitXml(); //call IncludeRaitXml extension
});
```

2. Enable RAIT Documentation Configuration in Test Setup
Call  the `ConfigureRait` extension method on the service provider in your test setup with `true` parameter:
```csharp
[SetUp]
public void Setup()
{
    _application = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(PrepareEnv);

    _defaultClient = _application.CreateDefaultClient();
    _application.Services.ConfigureRait(true);
}
```

3. **Mark Generated XML as Copy to Output Directory**
Ensure that the generated XML documentation is copied to the output directory. You can do this by modifying your `.csproj` file.

This will generate example values for your models and place the generated documentation in the `RAIT` folder within your API project.


## Contributing
We welcome contributions to RAIT! If you find a bug or have a feature request, please open an issue on GitHub. If you would like to contribute code, please fork the repository and submit a pull request.

## License
RAIT is licensed under the MIT License.
