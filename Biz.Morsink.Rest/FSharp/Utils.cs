using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.FSharp
{
    using static Names;
    /// <summary>
    /// Utility class for F# types.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Tests whether the argument is an F# union type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the argument is an F# union type, false otherwise.</returns>
        public static bool IsFsharpUnionType(Type type)
        {
            return type.GetCustomAttributes().Where(a => a.GetType().Name == CompilationMappingAttribute)
                .Select(a => new { a, Flags = a.GetType().GetProperty(SourceConstructFlags).GetValue(a, null)?.ToString() })
                .Where(a => a.Flags == SumType)
                .Any();
        }
        /// <summary>
        /// Gets the actual union type for some union or case type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>The actual union type for some union or case type.</returns>
        public static Type GetFsharpUnionType(Type type)
        {
            ThrowOnNonFSharpUnionType(type);
            return inner(type);

            Type inner(Type t)
            {
                if (IsFsharpUnionType(t.BaseType))
                    return GetFsharpUnionType(t.BaseType);
                else
                    return t;
            }
        }
        private static void ThrowOnNonFSharpUnionType(Type type)
        {
            if (!IsFsharpUnionType(type))
                throw new ArgumentException("Type is not an F# union type.");
        }
        private static IEnumerable<(int, string)> GetTagCollection(Type type)
        {
            var tags = type.GetNestedType(Tags);
            if (tags != null)
                return tags.GetFields().Select(f => ((int)f.GetValue(null), f.Name));
            else
                return GetConstructorMethods(type).Select(cm => (cm.Item2, cm.Item1.Name.Substring(cm.Item1.Name.StartsWith("New") ? 3 : 0)));
        }
        /// <summary>
        /// Gets a dictionary of tag integers mapping to tag names for some F# union type.
        /// </summary>
        /// <param name="type">An F# union type.</param>
        /// <returns>A dictionary of tag integers mapping to tag names for some F# union type.</returns>
        public static Dictionary<int, string> GetTags(Type type)
        {
            ThrowOnNonFSharpUnionType(type);
            return GetTagCollection(type)
                .ToDictionary(f => f.Item1, f => f.Item2);
        }
        /// <summary>
        /// Gets a dictionary of tag names mapping to tag integers for some F# union type.
        /// </summary>
        /// <param name="type">An F# union type.</param>
        /// <returns>A dictionary of tag names mapping to tag integers for some F# union type.</returns>
        public static Dictionary<string, int> GetTagsReverse(Type type)
        {
            ThrowOnNonFSharpUnionType(type);
            return GetTagCollection(type)
                .ToDictionary(f => f.Item2, f => f.Item1);
        }
        /// <summary>
        /// Gets a collection of constructor methods for some F# union type.
        /// </summary>
        /// <param name="type">An F# union type.</param>
        /// <returns>A collection of constructor methods paired with the tag number.</returns>
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
        /// <summary>
        /// Gets a dictionary of tag integers mapping to the case types, if they exist. 
        /// A case without parameters might not exist as a case class.
        /// </summary>
        /// <param name="type">An F# union type.</param>
        /// <returns>A dictionary of tag integers mapping to the case types, if they exist. </returns>
        public static Dictionary<int, Type> GetCaseClasses(Type type)
        {
            ThrowOnNonFSharpUnionType(type);
            var cases = from nestedType in type.GetNestedTypes()
                        let sequence = (from p in nestedType.GetProperties()
                                        from a in p.GetCustomAttributes()
                                        where a.GetType().Name == CompilationMappingAttribute
                                        select a.GetType().GetProperty(VariantNumber).GetValue(a)
                                        ).Distinct().FirstOrDefault()
                        where sequence != null
                        select (sequence:(int)sequence, nestedType);
            return cases.ToDictionary(kvp => kvp.sequence, kvp => kvp.nestedType);
        }
    }
}
