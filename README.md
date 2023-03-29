# RAIT

Dotnet rest api integration test library

## Nuget:
```
> dotnet add package RAIT
``` 

## Usage
```csharp
var model = new Model
{
    Id = 10
};
var responseModel = await _httpClient.Rait<RaitTestController>().Call(n => n.Post(model));
```
![Usage](https://cdn.discordapp.com/attachments/985879181856481325/1090707824872534076/image.png)


Example:
https://github.com/e1ektr0/Library/blob/master/Library.API.Test/AuthTest.cs


## How it working
![How it working](https://cdn.discordapp.com/attachments/985879181856481325/1090709345261592606/QYpQg53F15wAAAABJRU5ErkJggg.png)

## Todo:
1. Support more routing features
2. Improve serialization
3. Support form data models
4. Refactor
5. Better description
