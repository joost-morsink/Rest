using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using System;
using System.Linq;

namespace Biz.Morsink.Rest.FSharp
{
    using static FSharp.Names;
    using static FSharp.Utils;
    /// <summary>
    /// This class represents a type descriptor creator kind for F# union types.
    /// An F# union type is a well defined concept within the F# programming language.
    /// </summary>
    public class FSharpUnionDescriptorKind : TypeDescriptorCreator.IKind
    {
        /// <summary>
        /// Singleton property.
        /// </summary>
        public static FSharpUnionDescriptorKind Instance { get; } = new FSharpUnionDescriptorKind();
        private FSharpUnionDescriptorKind() { }
        /// <summary>
        /// Gets a type descriptor for an F# union type.
        /// An F# union type is a well defined concept within the F# programming language.
        /// This method returns null if the context does not represent an F# union tyoe.
        /// </summary>
        public TypeDescriptor GetDescriptor(TypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            if (!IsOfKind(context.Type))
                return null;
            if (context.Type.Namespace == Microsoft_FSharp_Core && context.Type.Name == FSharpOption_1)
            {
                var opt = creator.GetDescriptor(context.Type.GetGenericArguments()[0]);
                return TypeDescriptor.MakeUnion($"Optional<{opt.Name}>", new[]
                    {
                        opt,
                        TypeDescriptor.Null.Instance
                    }, context.Type);
            }
            else
            {
                var utype = UnionType.Create(context.Type);
                if (utype.IsSingleValue)
                    return creator.GetDescriptor(utype.Cases.First().Value.Parameters[0].Type);
                else
                {
                    var typeDescs = utype.Cases.Values.Select(c =>
                    TypeDescriptor.MakeRecord(c.Name,
                        new[] {
                                new PropertyDescriptor<TypeDescriptor>(Tag, TypeDescriptor.MakeValue(TypeDescriptor.Primitive.String.Instance, c.Name),true)
                        }.Concat(
                            c.Parameters.Select(p => new PropertyDescriptor<TypeDescriptor>(p.Name.CasedToPascalCase(), creator.GetDescriptor(p.Type), true))
                            ), null));
                    return TypeDescriptor.MakeUnion(context.Type.ToString(), typeDescs, context.Type);
                }
            }
        }

        public bool IsOfKind(Type type)
            => IsFsharpUnionType(type);
    }
}
