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
        public static Dictionary<int, string> GetTags(Type type)
        {
            return type.GetNestedType(Tags)
                ?.GetFields()
                .ToDictionary(f => (int)f.GetValue(null), f => f.Name);
        }
        public static IEnumerable<(MethodInfo, int)> GetConstructorMethods(Type type)
        {
            var constructorMethods = type.GetMethods()
                .Select(mi => new { Method = mi, Attribute = mi.GetCustomAttributes().FirstOrDefault(a => a.GetType().Name == CompilationMappingAttribute) })
                .Where(m => m.Attribute != null)
                .Select(m => (m.Method, (int)m.Attribute.GetType().GetProperty(SequenceNumber).GetValue(m.Attribute)))
                .OrderBy(m => m.Item2);

            return constructorMethods;
        }
    }
}
