using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.FSharp
{
    using static Names;
    public static class Utils
    {
        public static bool IsFsharpUnionType(Type type)
        {
            return type.GetCustomAttributes().Where(a => a.GetType().Name == CompilationMappingAttribute)
                .Select(a => new { a, Flags = a.GetType().GetProperty(SourceConstructFlags).GetValue(a, null)?.ToString() })
                .Where(a => a.Flags == SumType)
                .Any();
        }
        private static void ThrowOnNonFSharpUnionType(Type type)
        {
            if (!IsFsharpUnionType(type))
                throw new ArgumentException("Type is not an F# union type.");
        }
        public static Dictionary<int, string> GetTags(Type type)
        {
            ThrowOnNonFSharpUnionType(type);
            return type.GetNestedType(Tags)
                .GetFields()
                .ToDictionary(f => (int)f.GetValue(null), f => f.Name);
        }
        public static Dictionary<string, int> GetTagsReverse(Type type)
        {
            ThrowOnNonFSharpUnionType(type);
            return type.GetNestedType(Tags)
                .GetFields()
                .ToDictionary(f => f.Name, f => (int)f.GetValue(null));
        }
        public static IEnumerable<(MethodInfo, int)> GetConstructorMethods(Type type)
        {
            ThrowOnNonFSharpUnionType(type);
            var constructorMethods = type.GetMethods()
                .Select(mi => new { Method = mi, Attribute = mi.GetCustomAttributes().FirstOrDefault(a => a.GetType().Name == CompilationMappingAttribute) })
                .Where(m => m.Attribute != null)
                .Select(m => (m.Method, (int)m.Attribute.GetType().GetProperty(SequenceNumber).GetValue(m.Attribute)))
                .OrderBy(m => m.Item2);

            return constructorMethods;
        }
        public static Dictionary<int, Type> GetCaseClasses(Type type)
        {
            ThrowOnNonFSharpUnionType(type);
            var cases = from nt in type.GetNestedTypes()
                        let sequence = (from p in nt.GetProperties()
                                        from a in p.GetCustomAttributes()
                                        where a.GetType().Name == CompilationMappingAttribute
                                        select a.GetType().GetProperty(VariantNumber).GetValue(a)
                                        ).Distinct().FirstOrDefault()
                        where sequence != null
                        select new KeyValuePair<int, Type>((int)sequence, nt);
            return cases.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
