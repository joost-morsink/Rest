using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.FSharp
{
    public static class Names
    {
        public const string CompilationMappingAttribute = nameof(CompilationMappingAttribute);
        public const string SourceConstructFlags = nameof(SourceConstructFlags);
        public const string SumType = nameof(SumType);
        public const string UnionCase = nameof(UnionCase);
        public const string Tags = nameof(Tags);
        public const string Tag = nameof(Tag);
        public const string SequenceNumber = nameof(SequenceNumber);
        public const string VariantNumber = nameof(VariantNumber);

        public static readonly string Microsoft_FSharp_Core = nameof(Microsoft_FSharp_Core).Replace('_', '.');
        public static readonly string FSharpOption_1 = nameof(FSharpOption_1).Replace('_', '`');

    }
}
