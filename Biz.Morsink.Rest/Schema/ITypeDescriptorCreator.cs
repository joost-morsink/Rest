using System;
using System.Collections.Generic;
using Biz.Morsink.Rest.Serialization;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// The interface for type descriptor creators.
    /// </summary>
    public interface ITypeDescriptorCreator
    {
        /// <summary>
        /// Creates a serializer for some type t.
        /// </summary>
        /// <typeparam name="C">The type of serialization context.</typeparam>
        /// <param name="serializer">The serializer.</param>
        /// <param name="t">The type to create a serializer for.</param>
        /// <returns>A specific serializer for the specified type.</returns>
        Serializer<C>.IForType CreateSerializer<C>(Serializer<C> serializer, Type t) where C : SerializationContext<C>;
        /// <summary>
        /// Gets a TypeDescriptor for a context.
        /// </summary>
        /// <param name="context">A type context.</param>
        /// <returns>A TypeDescriptor for the context.</returns>
        TypeDescriptor GetDescriptor(TypeDescriptorCreator.Context context);
        /// <summary>
        /// Gets a TypeDescriptor for a type.
        /// </summary>
        /// <param name="type">A type.</param>
        /// <returns>A TypeDescriptor for the specified type.</returns>
        TypeDescriptor GetDescriptor(Type type);
        /// <summary>
        /// Gets a TypeDescriptor by name.
        /// </summary>
        /// <param name="name">The name of the TypeDescriptor.</param>
        /// <returns>A TypeDescriptor if one with the specified name could be found, null otherwise.</returns>
        TypeDescriptor GetDescriptorByName(string name);
        /// <summary>
        /// Creates a TypeDescriptor and makes it 'Referable' if it is not a primitive descriptor.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        TypeDescriptor GetReferableDescriptor(TypeDescriptorCreator.Context context);
        /// <summary>
        /// Gets the 'name' for a Type.
        /// The name is used as a key to lookup TypeDescriptors.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The name for a type.</returns>
        string GetTypeName(Type type);
   }
}