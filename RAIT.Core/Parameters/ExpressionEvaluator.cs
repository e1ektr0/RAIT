using System.Linq.Expressions;

namespace RAIT.Core;

/// <summary>
/// Evaluates expression trees to extract runtime values from method call arguments.
/// </summary>
internal static class ExpressionEvaluator
{
    internal static object? Evaluate(Expression expression)
    {
        return expression switch
        {
            ConstantExpression constantExpr => constantExpr.Value,
            MemberExpression memberExpr => EvaluateMember(memberExpr),
            UnaryExpression unaryExpr => EvaluateUnary(unaryExpr),
            MemberInitExpression memberInitExpr => EvaluateMemberInit(memberInitExpr),
            NewExpression newExpr => EvaluateNew(newExpr),
            MethodCallExpression methodCallExpr => EvaluateMethodCall(methodCallExpr),
            _ => CompileAndInvoke(expression)
        };
    }

    private static object? EvaluateMember(MemberExpression memberExpr)
    {
        return CompileAndInvoke(memberExpr);
    }

    private static object? EvaluateUnary(UnaryExpression unaryExpr)
    {
        return CompileAndInvoke(unaryExpr);
    }

    private static object? EvaluateMemberInit(MemberInitExpression memberInitExpr)
    {
        return CompileAndInvoke(memberInitExpr);
    }

    private static object? EvaluateNew(NewExpression newExpr)
    {
        return CompileAndInvoke(newExpr);
    }

    private static object? EvaluateMethodCall(MethodCallExpression methodCallExpr)
    {
        return CompileAndInvoke(methodCallExpr);
    }

    private static object? CompileAndInvoke(Expression expression)
    {
        var convertedExpression = Expression.Convert(expression, typeof(object));
        var lambda = Expression.Lambda<Func<object>>(convertedExpression);
        var compiled = lambda.Compile();
        return compiled();
    }
}
