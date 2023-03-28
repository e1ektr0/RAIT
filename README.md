# RAIT

Dotnet rest api integration test library

## Nuget:
```
> dotnet add package RAIT
``` 

## Usage
```csharp
    var responseModel = await _httpClient.Rait<RaitTestController>()
        .Call(n => n.Post(new Model
        {
            Id = 10
        }));
```
![Usage](https://cdn.discordapp.com/attachments/985879181856481325/1090371173730230272/image.png)

Alternative:
```csharp
    _raitHttpWrapper = new RaitHttpWrapper<RaitTestController>(_defaultClient);
    var responseModel = await _raitHttpWrapper.Call(n => n.Post(model));
```
![Usage](https://cdn.discordapp.com/attachments/449268423638122498/1056599500338249768/image.png)

Example:
https://github.com/e1ektr0/Library/blob/master/Library.API.Test/AuthTest.cs




## Explanation
![How it working](https://cdn.discordapp.com/attachments/449268423638122498/1056522060089798726/j8l3q3k3L7DXQAAAABJRU5ErkJggg.png)

## Todo:
1. Support more routing features
2. Improve serialization
3. Support form data models