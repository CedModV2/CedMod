// Utf8Json.Resolvers.DynamicCompositeResolver
using System;
using System.Reflection;
using System.Reflection.Emit;
using Utf8Json;
using Utf8Json.Internal.Emit;
using Utf8Json.Resolvers;

public abstract class DynamicCompositeResolver : IJsonFormatterResolver
{
	private const string ModuleName = "Utf8Json.Resolvers.DynamicCompositeResolver";

	private static readonly DynamicAssembly assembly;

	public readonly IJsonFormatter[] formatters;

	public readonly IJsonFormatterResolver[] resolvers;

	static DynamicCompositeResolver()
	{
		assembly = new DynamicAssembly("Utf8Json.Resolvers.DynamicCompositeResolver");
	}

	public static IJsonFormatterResolver Create(IJsonFormatter[] formatters, IJsonFormatterResolver[] resolvers)
	{
		string str = Guid.NewGuid().ToString().Replace("-", "");
		TypeBuilder typeBuilder = assembly.DefineType("DynamicCompositeResolver_" + str, TypeAttributes.Public | TypeAttributes.Sealed, typeof(DynamicCompositeResolver));
		TypeBuilder typeBuilder2 = assembly.DefineType("DynamicCompositeResolverCache_" + str, TypeAttributes.Public | TypeAttributes.Sealed, null);
		GenericTypeParameterBuilder genericTypeParameterBuilder = typeBuilder2.DefineGenericParameters("T")[0];
		FieldBuilder fieldInfo = typeBuilder.DefineField("instance", typeBuilder, FieldAttributes.FamANDAssem | FieldAttributes.Family | FieldAttributes.Static);
		FieldBuilder field = typeBuilder2.DefineField("formatter", typeof(IJsonFormatter<>).MakeGenericType(genericTypeParameterBuilder), FieldAttributes.FamANDAssem | FieldAttributes.Family | FieldAttributes.Static);
		ILGenerator iLGenerator = typeBuilder2.DefineConstructor(MethodAttributes.Static, CallingConventions.Standard, Type.EmptyTypes).GetILGenerator();
		iLGenerator.EmitLdsfld(fieldInfo);
		iLGenerator.EmitCall(typeof(DynamicCompositeResolver).GetMethod("GetFormatterLoop").MakeGenericMethod(genericTypeParameterBuilder));
		iLGenerator.Emit(OpCodes.Stsfld, field);
		iLGenerator.Emit(OpCodes.Ret);
		Type type = typeBuilder2.CreateTypeInfo().AsType();
		ILGenerator iLGenerator2 = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[2]
		{
			typeof(IJsonFormatter[]),
			typeof(IJsonFormatterResolver[])
		}).GetILGenerator();
		iLGenerator2.EmitLdarg(0);
		iLGenerator2.EmitLdarg(1);
		iLGenerator2.EmitLdarg(2);
		iLGenerator2.Emit(OpCodes.Call, typeof(DynamicCompositeResolver).GetConstructors()[0]);
		iLGenerator2.Emit(OpCodes.Ret);
		MethodBuilder methodBuilder = typeBuilder.DefineMethod("GetFormatter", MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Virtual);
		GenericTypeParameterBuilder genericTypeParameterBuilder2 = methodBuilder.DefineGenericParameters("T")[0];
		methodBuilder.SetReturnType(typeof(IJsonFormatter<>).MakeGenericType(genericTypeParameterBuilder2));
		ILGenerator iLGenerator3 = methodBuilder.GetILGenerator();
		FieldInfo field2 = TypeBuilder.GetField(type.MakeGenericType(genericTypeParameterBuilder2), type.GetField("formatter", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField));
		iLGenerator3.EmitLdsfld(field2);
		iLGenerator3.Emit(OpCodes.Ret);
		object obj = Activator.CreateInstance(typeBuilder.CreateTypeInfo().AsType(), new object[2]
		{
			formatters,
			resolvers
		});
		obj.GetType().GetField("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).SetValue(null, obj);
		return (IJsonFormatterResolver)obj;
	}

	public DynamicCompositeResolver(IJsonFormatter[] formatters, IJsonFormatterResolver[] resolvers)
	{
		this.formatters = formatters;
		this.resolvers = resolvers;
	}

	public IJsonFormatter<T> GetFormatterLoop<T>()
	{
		IJsonFormatter[] array = formatters;
		foreach (IJsonFormatter jsonFormatter in array)
		{
			foreach (Type implementedInterface in jsonFormatter.GetType().GetTypeInfo().ImplementedInterfaces)
			{
				TypeInfo typeInfo = implementedInterface.GetTypeInfo();
				if (typeInfo.IsGenericType && typeInfo.GenericTypeArguments[0] == typeof(T))
				{
					return (IJsonFormatter<T>)jsonFormatter;
				}
			}
		}
		IJsonFormatterResolver[] array2 = resolvers;
		for (int i = 0; i < array2.Length; i++)
		{
			IJsonFormatter<T> formatter = array2[i].GetFormatter<T>();
			if (formatter != null)
			{
				return formatter;
			}
		}
		return null;
	}

	public abstract IJsonFormatter<T> GetFormatter<T>();
}
