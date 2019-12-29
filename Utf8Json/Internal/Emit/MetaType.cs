// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.Emit.MetaType
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Utf8Json.Internal.Emit
{
  internal class MetaType
  {
    public Type Type { get; private set; }

    public bool IsClass { get; private set; }

    public bool IsStruct
    {
      get
      {
        return !this.IsClass;
      }
    }

    public bool IsConcreteClass { get; private set; }

    public ConstructorInfo BestmatchConstructor { get; internal set; }

    public MetaMember[] ConstructorParameters { get; internal set; }

    public MetaMember[] Members { get; internal set; }

    public MetaType(Type type, Func<string, string> nameMutetor, bool allowPrivate)
    {
      TypeInfo typeInfo = IntrospectionExtensions.GetTypeInfo(type);
      bool flag = ((Type) typeInfo).IsClass || ((Type) typeInfo).IsInterface || ((Type) typeInfo).IsAbstract;
      this.Type = type;
      Dictionary<string, MetaMember> source1 = new Dictionary<string, MetaMember>();
      foreach (PropertyInfo allProperty in type.GetAllProperties())
      {
        if (allProperty.GetIndexParameters().Length == 0 && CustomAttributeExtensions.GetCustomAttribute<IgnoreDataMemberAttribute>((MemberInfo) allProperty, true) == null)
        {
          DataMemberAttribute customAttribute = (DataMemberAttribute) CustomAttributeExtensions.GetCustomAttribute<DataMemberAttribute>((MemberInfo) allProperty, true);
          string name = customAttribute == null || customAttribute.Name == null ? nameMutetor(allProperty.Name) : customAttribute.Name;
          MetaMember metaMember = new MetaMember(allProperty, name, allowPrivate);
          if (metaMember.IsReadable || metaMember.IsWritable)
          {
            if (source1.ContainsKey(metaMember.Name))
              throw new InvalidOperationException("same (custom)name is in type. Type:" + type.Name + " Name:" + metaMember.Name);
            source1.Add(metaMember.Name, metaMember);
          }
        }
      }
      foreach (FieldInfo allField in type.GetAllFields())
      {
        if (CustomAttributeExtensions.GetCustomAttribute<IgnoreDataMemberAttribute>((MemberInfo) allField, true) == null && CustomAttributeExtensions.GetCustomAttribute<CompilerGeneratedAttribute>((MemberInfo) allField, true) == null && (!allField.IsStatic && !allField.Name.StartsWith("<")))
        {
          DataMemberAttribute customAttribute = (DataMemberAttribute) CustomAttributeExtensions.GetCustomAttribute<DataMemberAttribute>((MemberInfo) allField, true);
          string name = customAttribute == null || customAttribute.Name == null ? nameMutetor(allField.Name) : customAttribute.Name;
          MetaMember metaMember = new MetaMember(allField, name, allowPrivate);
          if (metaMember.IsReadable || metaMember.IsWritable)
          {
            if (source1.ContainsKey(metaMember.Name))
              throw new InvalidOperationException("same (custom)name is in type. Type:" + type.Name + " Name:" + metaMember.Name);
            source1.Add(metaMember.Name, metaMember);
          }
        }
      }
      ConstructorInfo ctor = typeInfo.get_DeclaredConstructors().Where<ConstructorInfo>((Func<ConstructorInfo, bool>) (x => x.IsPublic)).SingleOrDefault<ConstructorInfo>((Func<ConstructorInfo, bool>) (x => CustomAttributeExtensions.GetCustomAttribute<SerializationConstructorAttribute>((MemberInfo) x, false) != null));
      List<MetaMember> metaMemberList = new List<MetaMember>();
      IEnumerator<ConstructorInfo> ctorEnumerator = (IEnumerator<ConstructorInfo>) null;
      if (ctor == (ConstructorInfo) null)
      {
        ctorEnumerator = typeInfo.get_DeclaredConstructors().Where<ConstructorInfo>((Func<ConstructorInfo, bool>) (x => x.IsPublic)).OrderByDescending<ConstructorInfo, int>((Func<ConstructorInfo, int>) (x => x.GetParameters().Length)).GetEnumerator();
        if (ctorEnumerator.MoveNext())
          ctor = ctorEnumerator.Current;
      }
      if (ctor != (ConstructorInfo) null)
      {
        ILookup<string, KeyValuePair<string, MetaMember>> lookup = source1.ToLookup<KeyValuePair<string, MetaMember>, string, KeyValuePair<string, MetaMember>>((Func<KeyValuePair<string, MetaMember>, string>) (x => x.Key), (Func<KeyValuePair<string, MetaMember>, KeyValuePair<string, MetaMember>>) (x => x), (IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
        do
        {
          metaMemberList.Clear();
          int num = 0;
          foreach (ParameterInfo parameter in ctor.GetParameters())
          {
            IEnumerable<KeyValuePair<string, MetaMember>> source2 = lookup[parameter.Name];
            switch (source2.Count<KeyValuePair<string, MetaMember>>())
            {
              case 0:
                ctor = (ConstructorInfo) null;
                break;
              case 1:
                MetaMember metaMember = source2.First<KeyValuePair<string, MetaMember>>().Value;
                if (parameter.ParameterType == metaMember.Type && metaMember.IsReadable)
                {
                  metaMemberList.Add(metaMember);
                  ++num;
                  break;
                }
                ctor = (ConstructorInfo) null;
                break;
              default:
                if (ctorEnumerator != null)
                {
                  ctor = (ConstructorInfo) null;
                  break;
                }
                throw new InvalidOperationException("duplicate matched constructor parameter name:" + type.FullName + " parameterName:" + parameter.Name + " paramterType:" + parameter.ParameterType.Name);
            }
          }
        }
        while (MetaType.TryGetNextConstructor(ctorEnumerator, ref ctor));
      }
      this.IsClass = flag;
      this.IsConcreteClass = flag && (!((Type) typeInfo).IsAbstract && !((Type) typeInfo).IsInterface);
      this.BestmatchConstructor = ctor;
      this.ConstructorParameters = metaMemberList.ToArray();
      this.Members = source1.Values.ToArray<MetaMember>();
    }

    private static bool TryGetNextConstructor(
      IEnumerator<ConstructorInfo> ctorEnumerator,
      ref ConstructorInfo ctor)
    {
      if (ctorEnumerator == null || ctor != (ConstructorInfo) null)
        return false;
      if (ctorEnumerator.MoveNext())
      {
        ctor = ctorEnumerator.Current;
        return true;
      }
      ctor = (ConstructorInfo) null;
      return false;
    }
  }
}
