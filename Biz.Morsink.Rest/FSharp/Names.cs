using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.FSharp
{
    /// <summary>
    /// Some names found in the F# core library.
    /// These names can be used for reflection purposes.
    /// </summary>
    public static class Names
    {
        /// <summary>
        /// CompilationMappingAttribute
        /// </summary>
        public const string CompilationMappingAttribute = nameof(CompilationMappingAttribute);
        /// <summary>
        /// SourceConstructFlags
        /// </summary>
        public const string SourceConstructFlags = nameof(SourceConstructFlags);
        /// <summary>
        /// SumType
        /// </summary>
        public const string SumType = nameof(SumType);
        /// <summary>
        /// UnionCase
        /// </summary>
        public const string UnionCase = nameof(UnionCase);
        /// <summary>
        /// Tags
        /// </summary>
        public const string Tags = nameof(Tags);
        /// <summary>
        /// Tag
        /// </summary>
        public const string Tag = nameof(Tag);
        /// <summary>
        /// SequenceNumber
        /// </summary>
        public const string SequenceNumber = nameof(SequenceNumber);
        /// <summary>
        /// VariantNumber
        /// </summary>
        public const string VariantNumber = nameof(VariantNumber);
        /// <summary>
        /// Microsoft.FSharp.Core
        /// </summary>
        public static readonly string Microsoft_FSharp_Core = nameof(Microsoft_FSharp_Core).Replace('_', '.');
        /// <summary>
        /// Microsoft.FSharp.Collections
        /// </summary>
        public static readonly string Microsoft_FSharp_Collections = nameof(Microsoft_FSharp_Collections).Replace('_', '.');
        /// <summary>
        /// FSharpOption`1
        /// </summary>
        public static readonly string FSharpOption_1 = nameof(FSharpOption_1).Replace('_', '`');
        /// <summary>
        /// FSharpList`1
        /// </summary>
        public static readonly string FSharpList_1 = nameof(FSharpList_1).Replace('_', '`');
        public const string ListModule = nameof(ListModule);
        public const string OfSeq = nameof(OfSeq);
    }
}
