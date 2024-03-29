﻿using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;

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

    public async Task<TOutput?> Call<TOutput>(Expression<Func<TController, Task<TOutput>>> tree)
    {
        var methodBody = tree.Body as MethodCallExpression;
        var methodInfo = methodBody!.Method;

        var prepareInputParameters = RaitParameterExtractor.PrepareInputParameters(tree);
        var rout = RaitRouter.PrepareRout(tree, prepareInputParameters);
        return await RaitHttpRequester.HttpRequest<TOutput?>(_client, methodInfo.CustomAttributes, rout,
            prepareInputParameters);
    }
    
    public async Task<TOutput> CallR<TOutput>(Expression<Func<TController, Task<TOutput>>> tree)
    {
        var methodBody = tree.Body as MethodCallExpression;
        var methodInfo = methodBody!.Method;

        var prepareInputParameters = RaitParameterExtractor.PrepareInputParameters(tree);
        var rout = RaitRouter.PrepareRout(tree, prepareInputParameters);
        var output = await RaitHttpRequester.HttpRequest<TOutput?>(_client, methodInfo.CustomAttributes, rout,
            prepareInputParameters);
        if (output == null)
            throw new ArgumentNullException();
        return output;
    }
    public async Task Call(Expression<Func<TController, Task>> tree)
    {
        var methodBody = tree.Body as MethodCallExpression;
        var methodInfo = methodBody!.Method;

        var prepareInputParameters = RaitParameterExtractor.PrepareInputParameters(tree);
        var rout = RaitRouter.PrepareRout(tree, prepareInputParameters);
        await RaitHttpRequester.HttpRequest(_client, methodInfo.CustomAttributes, rout,
            prepareInputParameters, typeof(EmptyResponse));
    }
    
    public async Task CallR(Expression<Func<TController, Task>> tree)
    {
        var methodBody = tree.Body as MethodCallExpression;
        var methodInfo = methodBody!.Method;

        var prepareInputParameters = RaitParameterExtractor.PrepareInputParameters(tree);
        var rout = RaitRouter.PrepareRout(tree, prepareInputParameters);
        await RaitHttpRequester.HttpRequest(_client, methodInfo.CustomAttributes, rout,
            prepareInputParameters, typeof(EmptyResponse));
    }
}