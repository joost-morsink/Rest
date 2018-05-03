using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biz.Morsink.Rest.FSharp
{
    using static Utils;
    public class UnionType
    {
        public static UnionType Create(Type type)
        {
            if (!IsFsharpUnionType(type))
                throw new ArgumentException("Type is not an F# union type.");
            var tags = GetTags(type);
            var constructorMethods = GetConstructorMethods(type).Select(cm => (cm.Item1, cm.Item2, tags[cm.Item2]));
            return new UnionType(type, constructorMethods.Select(UnionCase.Create));
        }
        internal UnionType(Type type, IEnumerable<UnionCase> cases)
        {
            ForType = type;
            Cases = cases.ToDictionary(c => c.Tag);
            CasesByName = cases.ToDictionary(c => c.Name);
        }

        public Type ForType { get; }
        public IReadOnlyDictionary<int, UnionCase> Cases { get; }
        public IReadOnlyDictionary<string, UnionCase> CasesByName { get; }
    }
}
