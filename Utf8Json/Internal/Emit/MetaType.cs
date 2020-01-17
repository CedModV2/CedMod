// Utf8Json.Internal.Emit.MetaType
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Utf8Json;
using Utf8Json.Internal;
using Utf8Json.Internal.Emit;

internal class MetaType
{
	public Type Type
	{
		get;
		private set;
	}

	public bool IsClass
	{
		get;
		private set;
	}

	public bool IsStruct => !IsClass;

	public bool IsConcreteClass
	{
		get;
		private set;
	}

	public ConstructorInfo BestmatchConstructor
	{
		get;
		internal set;
	}

	public MetaMember[] ConstructorParameters
	{
		get;
		internal set;
	}

	public MetaMember[] Members
	{
		get;
		internal set;
	}

	public MetaType(Type type, Func<string, string> nameMutetor, bool allowPrivate)
	{
		TypeInfo typeInfo = type.GetTypeInfo();
		bool flag = typeInfo.IsClass || typeInfo.IsInterface || typeInfo.IsAbstract;
		Type = type;
		Dictionary<string, MetaMember> dictionary = new Dictionary<string, MetaMember>();
		foreach (PropertyInfo allProperty in type.GetAllProperties())
		{
			if (allProperty.GetIndexParameters().Length == 0 && allProperty.GetCustomAttribute<IgnoreDataMemberAttribute>(inherit: true) == null)
			{
				DataMemberAttribute customAttribute = allProperty.GetCustomAttribute<DataMemberAttribute>(inherit: true);
				string name = (customAttribute != null && customAttribute.Name != null) ? customAttribute.Name : nameMutetor(allProperty.Name);
				MetaMember metaMember = new MetaMember(allProperty, name, allowPrivate);
				if (metaMember.IsReadable || metaMember.IsWritable)
				{
					if (dictionary.ContainsKey(metaMember.Name))
					{
						throw new InvalidOperationException("same (custom)name is in type. Type:" + type.Name + " Name:" + metaMember.Name);
					}
					dictionary.Add(metaMember.Name, metaMember);
				}
			}
		}
		foreach (FieldInfo allField in type.GetAllFields())
		{
			if (allField.GetCustomAttribute<IgnoreDataMemberAttribute>(inherit: true) == null && allField.GetCustomAttribute<CompilerGeneratedAttribute>(inherit: true) == null && !allField.IsStatic && !allField.Name.StartsWith("<"))
			{
				DataMemberAttribute customAttribute2 = allField.GetCustomAttribute<DataMemberAttribute>(inherit: true);
				string name2 = (customAttribute2 != null && customAttribute2.Name != null) ? customAttribute2.Name : nameMutetor(allField.Name);
				MetaMember metaMember2 = new MetaMember(allField, name2, allowPrivate);
				if (metaMember2.IsReadable || metaMember2.IsWritable)
				{
					if (dictionary.ContainsKey(metaMember2.Name))
					{
						throw new InvalidOperationException("same (custom)name is in type. Type:" + type.Name + " Name:" + metaMember2.Name);
					}
					dictionary.Add(metaMember2.Name, metaMember2);
				}
			}
		}
		ConstructorInfo ctor = typeInfo.DeclaredConstructors.Where((ConstructorInfo x) => x.IsPublic).SingleOrDefault((ConstructorInfo x) => x.GetCustomAttribute<SerializationConstructorAttribute>(inherit: false) != null);
		List<MetaMember> list = new List<MetaMember>();
		IEnumerator<ConstructorInfo> enumerator3 = null;
		if (ctor == null)
		{
			enumerator3 = (from x in typeInfo.DeclaredConstructors
						   where x.IsPublic
						   orderby x.GetParameters().Length descending
						   select x).GetEnumerator();
			if (enumerator3.MoveNext())
			{
				ctor = enumerator3.Current;
			}
		}
		if (ctor != null)
		{
			ILookup<string, KeyValuePair<string, MetaMember>> lookup = dictionary.ToLookup((KeyValuePair<string, MetaMember> x) => x.Key, (KeyValuePair<string, MetaMember> x) => x, StringComparer.OrdinalIgnoreCase);
			do
			{
				list.Clear();
				int num = 0;
				ParameterInfo[] parameters = ctor.GetParameters();
				foreach (ParameterInfo parameterInfo in parameters)
				{
					IEnumerable<KeyValuePair<string, MetaMember>> source = lookup[parameterInfo.Name];
					switch (source.Count())
					{
						default:
							if (enumerator3 != null)
							{
								ctor = null;
								break;
							}
							throw new InvalidOperationException("duplicate matched constructor parameter name:" + type.FullName + " parameterName:" + parameterInfo.Name + " paramterType:" + parameterInfo.ParameterType.Name);
						case 1:
							{
								MetaMember value = source.First().Value;
								if (parameterInfo.ParameterType == value.Type && value.IsReadable)
								{
									list.Add(value);
									num++;
								}
								else
								{
									ctor = null;
								}
								break;
							}
						case 0:
							ctor = null;
							break;
					}
				}
			}
			while (TryGetNextConstructor(enumerator3, ref ctor));
		}
		IsClass = flag;
		IsConcreteClass = (flag && !typeInfo.IsAbstract && !typeInfo.IsInterface);
		BestmatchConstructor = ctor;
		ConstructorParameters = list.ToArray();
		Members = dictionary.Values.ToArray();
	}

	private static bool TryGetNextConstructor(IEnumerator<ConstructorInfo> ctorEnumerator, ref ConstructorInfo ctor)
	{
		if (ctorEnumerator == null || ctor != null)
		{
			return false;
		}
		if (ctorEnumerator.MoveNext())
		{
			ctor = ctorEnumerator.Current;
			return true;
		}
		ctor = null;
		return false;
	}
}
