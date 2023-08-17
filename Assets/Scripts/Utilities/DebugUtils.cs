using System;
using System.Linq.Expressions;
using UnityEngine;

public static class DebugUtils
{
    public static void LogExpression<T>(Expression<Func<T>> expression)
    {
        string exprBody = expression.Body.ToString();
        string exprValue = expression.Compile().Invoke().ToString();

        string format = "{0}: {1}";

        Debug.LogFormat(format, exprBody, exprValue);
    }

    public static void LogAssertionExpression(Expression<Func<bool>> expression)
    {
        string exprBody = expression.Body.ToString();

        bool exprResult = expression.Compile().Invoke();

        string exprValue = exprResult.ToString();

        string format = "{0}: {1}";

        if (exprResult)
        {
            Debug.LogFormat(format, exprBody, exprValue);
        }
        else
        {
            Debug.LogErrorFormat(format, exprBody, exprValue);
        }
    }
}