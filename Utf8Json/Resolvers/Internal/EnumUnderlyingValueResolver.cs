// Utf8Json.Resolvers.Internal.EnumUnderlyingValueResolver
using System;
using System.Reflection;
using Utf8Json;
using Utf8Json.Formatters;
using Utf8Json.Internal;
using Utf8Json.Resolvers.Internal;

internal sealed class EnumUnderlyingValueResolver : IJsonFormatterResolver
{
    private static class FormatterCache<T>
    {
        public static readonly IJsonFormatter<T> formatter;

        static FormatterCache()
        {
            TypeInfo typeInfo = typeof(T).GetTypeInfo();
            if (typeInfo.IsNullable())
            {
                typeInfo = typeInfo.GenericTypeArguments[0].GetTypeInfo();
                if (typeInfo.IsEnum)
                {
                    object formatterDynamic = Instance.GetFormatterDynamic(typeInfo.AsType());
                    if (formatterDynamic != null)
                    {
                        formatter = (IJsonFormatter<T>)Activator.CreateInstance(typeof(StaticNullableFormatter<>).MakeGenericType(typeInfo.AsType()), formatterDynamic);
                    }
                }
            }
            else if (typeof(T).IsEnum)
            {
                formatter = new EnumFormatter<T>(serializeByName: false);
            }
        }
    }

    public static readonly IJsonFormatterResolver Instance = new EnumUnderlyingValueResolver();

    private EnumUnderlyingValueResolver()
    {
    }

    public IJsonFormatter<T> GetFormatter<T>()
    {
        return FormatterCache<T>.formatter;
    }
}