using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Utils;
using System.Linq;

namespace Biz.Morsink.Rest.FSharp
{

    using static FSharp.Names;
    using static FSharp.Utils;
    public class FSharpUnionDescriptorKind : TypeDescriptorCreator.IKind
    {
        public static FSharpUnionDescriptorKind Instance { get; } = new FSharpUnionDescriptorKind();
        private FSharpUnionDescriptorKind() { }
        public TypeDescriptor GetDescriptor(TypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            if (IsFsharpUnionType(context.Type))
            {
                if (context.Type.Namespace == Microsoft_FSharp_Core && context.Type.Name == FSharpOption_1)
                {
                    var opt = creator.GetDescriptor(context.Type.GetGenericArguments()[0]);
                    return TypeDescriptor.MakeUnion($"Optional<{opt.Name}>", new[]
                    {
                        opt,
                        TypeDescriptor.Null.Instance
                    });
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
                                )));
                        return TypeDescriptor.MakeUnion(context.Type.ToString(), typeDescs);
                    }
                }
            }
            return null;
        }
    }
}
