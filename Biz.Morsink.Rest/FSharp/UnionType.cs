using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.FSharp
{
    using static Utils;
    /// <summary>
    /// This class describes an F# union type.
    /// </summary>
    public class UnionType
    {
        /// <summary>
        /// Creates a UnionType instance based on some type.
        /// The type does need to be an F# union type.
        /// </summary>
        /// <param name="type">An F# union type.</param>
        /// <returns>A UnionType instance describing the type.</returns>
        public static UnionType Create(Type type)
        {
            if (!IsFsharpUnionType(type))
                throw new ArgumentException("Type is not an F# union type.");
            var tags = GetTags(type);
            var classes = GetCaseClasses(type);
            var createParameters = GetConstructorMethods(type).Select(cm => new UnionCase.CreateParameters(cm.Item1, cm.Item2, tags[cm.Item2], classes.TryGetValue(cm.Item2, out var caseType) ? caseType : type, type.IsValueType));
            return new UnionType(type, createParameters.Select(UnionCase.Create));
        }
        internal UnionType(Type type, IEnumerable<UnionCase> cases)
        {
            ForType = type;
            Cases = cases.ToDictionary(c => c.Tag);
            CasesByName = cases.ToDictionary(c => c.Name);
        }
        /// <summary>
        /// Contains the type of the F# union type.
        /// </summary>
        public Type ForType { get; }
        /// <summary>
        /// Contains a collection of the cases indexed by the integer tag.
        /// </summary>
        public IReadOnlyDictionary<int, UnionCase> Cases { get; }
        /// <summary>
        /// Contains a collection of the cases indexed by the case name.
        /// </summary>
        public IReadOnlyDictionary<string, UnionCase> CasesByName { get; }
    }
}
