using System;
using System.Collections.Generic;
using Biz.Morsink.Rest.Serialization;

namespace Biz.Morsink.Rest.Schema
{
    public interface ITypeDescriptorCreator
    {
        Serializer<C>.IForType CreateSerializer<C>(Serializer<C> serializer, Type t) where C : SerializationContext<C>;
        TypeDescriptor GetDescriptor(TypeDescriptorCreator.Context context);
        TypeDescriptor GetDescriptor(Type type);
        TypeDescriptor GetDescriptorByName(string name);
        TypeDescriptor GetReferableDescriptor(TypeDescriptorCreator.Context context);
        string GetTypeName(Type type);
   }
}