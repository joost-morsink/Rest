using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    public class TypeDescriptorTypeRepresentation : SimpleTypeRepresentation<TypeDescriptor, string>
    {
        private readonly Lazy<ITypeDescriptorCreator> typeDescriptorCreator;
        public TypeDescriptorTypeRepresentation(Lazy<ITypeDescriptorCreator> typeDescriptorCreator)
        {
            this.typeDescriptorCreator = typeDescriptorCreator;
        }
        public static TypeDescriptorTypeRepresentation Create(IServiceProvider serviceProvider)
        {
            return new TypeDescriptorTypeRepresentation(new Lazy<ITypeDescriptorCreator>(() => (ITypeDescriptorCreator)serviceProvider.GetService(typeof(ITypeDescriptorCreator))));
        }
        public override TypeDescriptor GetRepresentable(string representation)
        {
            return typeDescriptorCreator.Value.GetDescriptor(Type.GetType(representation));
        }

        public override string GetRepresentation(TypeDescriptor item)
        {
            return item.AssociatedType.AssemblyQualifiedName;
        }
    }
    //public class TypeDescriptorTypeRepresentation : TaggedUnionTypeRepresentation<TypeDescriptor, TypeDescriptorTypeRepresentation.Representation>
    //{
    //    public class Representation : TaggedUnionRepresentationType
    //    {
    //        public Representation() : base(typeof(TypeDescriptor),
    //            ("Any", typeof(TypeDescriptor.Any)),
    //            ("Array", typeof(TypeDescriptor.Array)),
    //            ("Dictionary", typeof(TypeDescriptor.Dictionary)),
    //            ("Intersection", typeof(TypeDescriptor.Intersection)),
    //            ("Null", typeof(TypeDescriptor.Null)),
    //            ("Record", typeof(TypeDescriptor.Record)),
    //            ("Referable", typeof(TypeDescriptor.Referable)),
    //            ("Reference", typeof(TypeDescriptor.Reference)),
    //            ("Union", typeof(TypeDescriptor.Union)),
    //            ("Value", typeof(TypeDescriptor.Value)),
    //            ("Boolean", typeof(TypeDescriptor.Primitive.Boolean)),
    //            ("DateTime", typeof(TypeDescriptor.Primitive.DateTime)),
    //            ("Float", typeof(TypeDescriptor.Primitive.Numeric.Float)),
    //            ("Integral", typeof(TypeDescriptor.Primitive.Numeric.Integral)),
    //            ("String", typeof(TypeDescriptor.Primitive.String)))
    //        { }
    //    }
    //}
}
