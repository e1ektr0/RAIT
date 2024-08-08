using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using RAIT.Core.DocumentationGenerator;

namespace RAIT.Core;

public class EmptyResponse
{
}

public class RaitHttpWrapper<TController> where TController : ControllerBase
{
    private readonly HttpClient _client;

    public RaitHttpWrapper(HttpClient client)
    {
        _client = client;
    }

    public async Task<TOutput?> Call<TOutput, TOut>(Expression<Func<TController, Task<TOut>>> tree) where TOutput : TOut
    {
        var methodBody = tree.Body as MethodCallExpression;
        var methodInfo = methodBody!.Method;
        var method = typeof(TController).GetMethod(methodBody.Method.Name);
        var methodInfoCustomAttributes = method!.CustomAttributes;

        var prepareInputParameters = RaitParameterExtractor.PrepareInputParameters(tree);
        RaitDocumentationGenerator.Params<TController>(prepareInputParameters);
        RaitDocumentationGenerator.Method<TController>(methodInfo.Name, prepareInputParameters);

        var rout = RaitRouter.PrepareRout(tree, prepareInputParameters);
        var result = await RaitHttpRequester.HttpRequest<TOutput?>(_client, methodInfoCustomAttributes, rout,
            prepareInputParameters);
        if (result != null)
            RaitDocumentationGenerator.Params<TController>(new List<InputParameter>
            {
                new()
                {
                    Value = result, Type = result.GetType()
                }
            });
        return result;
    }

    public async Task<TOutput?> Call<TOutput>(Expression<Func<TController, Task<TOutput>>> tree)
    {
        var methodBody = tree.Body as MethodCallExpression;
        var methodInfo = methodBody!.Method;
        var method = typeof(TController).GetMethod(methodBody.Method.Name);
        var methodInfoCustomAttributes = method!.CustomAttributes;

        var prepareInputParameters = RaitParameterExtractor.PrepareInputParameters(tree);
        RaitDocumentationGenerator.Params<TController>(prepareInputParameters);
        RaitDocumentationGenerator.Method<TController>(methodInfo.Name, prepareInputParameters);
        var rout = RaitRouter.PrepareRout(tree, prepareInputParameters);
        var result = await RaitHttpRequester.HttpRequest<TOutput?>(_client, methodInfoCustomAttributes, rout,
            prepareInputParameters);
        if (result != null)
            RaitDocumentationGenerator.Params<TController>(new List<InputParameter>
            {
                new()
                {
                    Value = result, Type = result.GetType()
                }
            });
        return result;
    }

    public async Task<TOutput> CallR<TOutput, TOut>(Expression<Func<TController, Task<TOut>>> tree) where TOutput : TOut
    {
        var methodBody = tree.Body as MethodCallExpression;
        var methodInfo = methodBody!.Method;
        var method = typeof(TController).GetMethod(methodBody.Method.Name);
        var methodInfoCustomAttributes = method!.CustomAttributes;

        var prepareInputParameters = RaitParameterExtractor.PrepareInputParameters(tree);
        RaitDocumentationGenerator.Params<TController>(prepareInputParameters);
        RaitDocumentationGenerator.Method<TController>(methodInfo.Name, prepareInputParameters);

        var rout = RaitRouter.PrepareRout(tree, prepareInputParameters);
        var result = await RaitHttpRequester.HttpRequest<TOutput?>(_client, methodInfoCustomAttributes, rout,
            prepareInputParameters);
        if (result == null)
            throw new ArgumentNullException();
        RaitDocumentationGenerator.Params<TController>(new List<InputParameter>
        {
            new()
            {
                Value = result, Type = result.GetType()
            }
        });
        return result;
    }

    public async Task<TOutput> CallR<TOutput>(Expression<Func<TController, Task<TOutput>>> tree)
    {
        var methodBody = tree.Body as MethodCallExpression;
        var methodInfo = methodBody!.Method;

        var method = typeof(TController).GetMethod(methodBody.Method.Name);
        var methodInfoCustomAttributes = method!.CustomAttributes;

        var prepareInputParameters = RaitParameterExtractor.PrepareInputParameters(tree);
        RaitDocumentationGenerator.Params<TController>(prepareInputParameters);
        RaitDocumentationGenerator.Method<TController>(methodInfo.Name, prepareInputParameters);

        var rout = RaitRouter.PrepareRout(tree, prepareInputParameters);
        var result = await RaitHttpRequester.HttpRequest<TOutput?>(_client, methodInfoCustomAttributes, rout,
            prepareInputParameters);
        if (result == null)
            throw new ArgumentNullException();
        RaitDocumentationGenerator.Params<TController>(new List<InputParameter>
        {
            new()
            {
                Value = result, Type = result.GetType()
            }
        });
        return result;
    }

    public async Task Call(Expression<Func<TController, Task>> tree)
    {
        var methodBody = tree.Body as MethodCallExpression;
        var methodInfo = methodBody!.Method;
        var method = typeof(TController).GetMethod(methodBody.Method.Name);
        var methodInfoCustomAttributes = method!.CustomAttributes;

        var prepareInputParameters = RaitParameterExtractor.PrepareInputParameters(tree);
        RaitDocumentationGenerator.Params<TController>(prepareInputParameters);
        RaitDocumentationGenerator.Method<TController>(methodInfo.Name, prepareInputParameters);

        var rout = RaitRouter.PrepareRout(tree, prepareInputParameters);
        var result = await RaitHttpRequester.HttpRequest(_client, methodInfoCustomAttributes, rout,
            prepareInputParameters, typeof(EmptyResponse));
        if (result != null)
            RaitDocumentationGenerator.Params<TController>(new List<InputParameter>
            {
                new()
                {
                    Value = result, Type = result.GetType()
                }
            });
    }

    public async Task CallR(Expression<Func<TController, Task>> tree)
    {
        var methodBody = tree.Body as MethodCallExpression;
        var methodInfo = methodBody!.Method;
        var method = typeof(TController).GetMethod(methodBody.Method.Name);
        var methodInfoCustomAttributes = method!.CustomAttributes;

        var prepareInputParameters = RaitParameterExtractor.PrepareInputParameters(tree);
        RaitDocumentationGenerator.Params<TController>(prepareInputParameters);
        RaitDocumentationGenerator.Method<TController>(methodInfo.Name, prepareInputParameters);

        var rout = RaitRouter.PrepareRout(tree, prepareInputParameters);
        var result = await RaitHttpRequester.HttpRequest(_client, methodInfoCustomAttributes, rout,
            prepareInputParameters, typeof(EmptyResponse));
        if (result != null)
            RaitDocumentationGenerator.Params<TController>(new List<InputParameter>
            {
                new()
                {
                    Value = result, Type = result.GetType()
                }
            });
    }

    public async Task<string> CallWithoutDeserialization(Expression<Func<TController, Task>> tree)
    {
        var methodBody = tree.Body as MethodCallExpression;
        var methodInfo = methodBody!.Method;
        var method = typeof(TController).GetMethod(methodBody.Method.Name);
        var methodInfoCustomAttributes = method!.CustomAttributes;

        var prepareInputParameters = RaitParameterExtractor.PrepareInputParameters(tree);
        RaitDocumentationGenerator.Params<TController>(prepareInputParameters);
        RaitDocumentationGenerator.Method<TController>(methodInfo.Name, prepareInputParameters);

        var rout = RaitRouter.PrepareRout(tree, prepareInputParameters);
        var httpRequestWithoutDeserialization = await RaitHttpRequester.HttpRequestWithoutDeserialization(_client,
            methodInfoCustomAttributes, rout,
            prepareInputParameters);
        return httpRequestWithoutDeserialization;
    }
}