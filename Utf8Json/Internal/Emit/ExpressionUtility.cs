// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.Emit.ExpressionUtility
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Utf8Json.Internal.Emit
{
  internal static class ExpressionUtility
  {
    private static MethodInfo GetMethodInfoCore(LambdaExpression expression)
    {
      if (expression == null)
        throw new ArgumentNullException(nameof (expression));
      return (expression.Body as MethodCallExpression).Method;
    }

    public static MethodInfo GetMethodInfo<T>(Expression<Func<T>> expression)
    {
      return ExpressionUtility.GetMethodInfoCore((LambdaExpression) expression);
    }

    public static MethodInfo GetMethodInfo(Expression<Action> expression)
    {
      return ExpressionUtility.GetMethodInfoCore((LambdaExpression) expression);
    }

    public static MethodInfo GetMethodInfo<T, TR>(Expression<Func<T, TR>> expression)
    {
      return ExpressionUtility.GetMethodInfoCore((LambdaExpression) expression);
    }

    public static MethodInfo GetMethodInfo<T>(Expression<Action<T>> expression)
    {
      return ExpressionUtility.GetMethodInfoCore((LambdaExpression) expression);
    }

    public static MethodInfo GetMethodInfo<TArg1, TArg2>(
      Expression<Action<TArg1, TArg2>> expression)
    {
      return ExpressionUtility.GetMethodInfoCore((LambdaExpression) expression);
    }

    public static MethodInfo GetMethodInfo<T, TArg1, TR>(
      Expression<Func<T, TArg1, TR>> expression)
    {
      return ExpressionUtility.GetMethodInfoCore((LambdaExpression) expression);
    }

    private static MemberInfo GetMemberInfoCore<T>(Expression<T> source)
    {
      if (source == null)
        throw new ArgumentNullException(nameof (source));
      return (source.Body as MemberExpression).Member;
    }

    public static PropertyInfo GetPropertyInfo<T, TR>(Expression<Func<T, TR>> expression)
    {
      return ExpressionUtility.GetMemberInfoCore<Func<T, TR>>(expression) as PropertyInfo;
    }

    public static FieldInfo GetFieldInfo<T, TR>(Expression<Func<T, TR>> expression)
    {
      return ExpressionUtility.GetMemberInfoCore<Func<T, TR>>(expression) as FieldInfo;
    }
  }
}
