using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Biz.Morsink.Rest.FSharp
{
    using Biz.Morsink.Rest.Utils;
    using static Biz.Morsink.Rest.FSharp.Names;
    public class UnionCase
    {
        internal struct CreateParameters
        {
            public CreateParameters(MethodInfo constructorMethod, int tag, string name, Type caseClass, bool isStruct)
            {
                ConstructorMethod = constructorMethod;
                Tag = tag;
                Name = name;
                CaseClass = caseClass;
                IsStruct = isStruct;
            }
            public MethodInfo ConstructorMethod { get; }
            public int Tag { get; }
            public string Name { get; }
            public Type CaseClass { get; }
            public bool IsStruct { get; }
        }

        private static IEnumerable<UnionCaseParameter> GetParameters(CreateParameters m, Func<Attribute, bool> propertyFilter)
        {
            var q = from par in m.ConstructorMethod.GetParameters().Select((p, i) => (p, i))
                    join cprop in (from cprop in m.CaseClass.GetProperties()
                                   let attr = cprop.GetCustomAttributes().FirstOrDefault(a => a.GetType().Name == CompilationMappingAttribute)
                                   where attr != null && propertyFilter(attr)
                                   select (cprop, (int)attr.GetType().GetProperty(SequenceNumber).GetValue(attr)))
                                   on par.i equals cprop.Item2 into cprop
                    select new UnionCaseParameter(par.p.ParameterType, AdjustName(par.p.Name), cprop.Select(p => p.cprop).FirstOrDefault());
            return q;
        }
        private static string AdjustName(string str) => str.CasedToPascalCase();
        
        internal static UnionCase Create(CreateParameters m)
        {
            var q = GetParameters(m, a => !m.IsStruct || (int)a.GetType().GetProperty(VariantNumber).GetValue(a) == m.Tag);
            return new UnionCase(m.Tag, q, AdjustName(m.Name));
        }
        internal UnionCase(int tag, IEnumerable<UnionCaseParameter> parameters, string name)
        {
            Tag = tag;
            Parameters = parameters.ToArray();
            Name = name;
        }
        public int Tag { get; }
        public IReadOnlyList<UnionCaseParameter> Parameters { get; }
        public string Name { get; }
    }
}