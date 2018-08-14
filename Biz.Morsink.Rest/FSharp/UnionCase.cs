using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Biz.Morsink.Rest.Utils;

namespace Biz.Morsink.Rest.FSharp
{
    using static Biz.Morsink.Rest.FSharp.Names;
    /// <summary>
    /// This class describes a case belonging to an F# union type.
    /// </summary>
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
            return new UnionCase(m.ConstructorMethod, m.Tag, q, AdjustName(m.Name), m.CaseClass);
        }
        internal UnionCase(MethodInfo constructorMethod, int tag, IEnumerable<UnionCaseParameter> parameters, string name, Type type)
        {
            ConstructorMethod = constructorMethod;
            Tag = tag;
            Parameters = parameters.ToArray();
            Name = name;
            Type = type;
        }
        /// <summary>
        /// Contains the MethodInfo that can be used to construct an instance of the case.
        /// </summary>
        public MethodInfo ConstructorMethod { get; }
        /// <summary>
        /// Contains the integer Tag used by F#.
        /// </summary>
        public int Tag { get; }
        /// <summary>
        /// Contains a list of parameters for the construction of the case.
        /// </summary>
        public IReadOnlyList<UnionCaseParameter> Parameters { get; }
        /// <summary>
        /// Contains the case's name.
        /// </summary>
        public string Name { get; }
        public Type Type { get; }
    }
}