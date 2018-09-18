using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Biz.Morsink.Rest.Utils;
using Biz.Morsink.Rest.Serialization;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// A class that helps construct TypeDescriptor objects for CLR types.
    /// </summary>
    public class TypeDescriptorCreator
    {
        private class MultipleKinds : IKindPipeline
        {
            private IKind[] kinds;

            public MultipleKinds(IEnumerable<IKind> kinds)
            {
                this.kinds = kinds.ToArray();
            }

            public TypeDescriptor GetDescriptor(ITypeDescriptorCreator creator, Context context)
            {
                TypeDescriptor result = null;
                for (int i = 0; i < kinds.Length && result == null; i++)
                    result = kinds[i].GetDescriptor(creator, context);
                return result;
            }

            public Serializer<C>.IForType GetSerializer<C>(Serializer<C> serializer, Type type) where C : SerializationContext<C>
                => kinds.AsEnumerable().Select(kind => kind.GetSerializer(serializer, type)).Where(s => s != null).FirstOrDefault();

            public bool IsOfKind(Type type)
                => kinds.Any(k => k.IsOfKind(type));
        }
        /// <summary>
        /// This interface specifies the contract for a single kind of TypeDescriptor creator.
        /// The kinds are processed in an IKindPipeline in order, which can be injected.
        /// </summary>
        public interface IKind
        {
            /// <summary>
            /// Gets a TypeDescriptor for some type in some context.
            /// </summary>
            /// <param name="creator">The TypeDescriptorCreator instance that is processing the request.</param>
            /// <param name="context">The type context for generating a descriptor.</param>
            /// <returns>
            /// If the context matches this kind it should return a TypeDescriptor for the context. 
            /// If the context does not match, this method should return null.
            /// </returns>
            TypeDescriptor GetDescriptor(ITypeDescriptorCreator creator, Context context);
            /// <summary>
            /// Checks if some type is of this kind.
            /// </summary>
            /// <param name="type">The type to check.</param>
            /// <returns>True if the specified type is of this kind.</returns>
            bool IsOfKind(Type type);
            /// <summary>
            /// Gets a serializer for a certain type.
            /// </summary>
            /// <typeparam name="C">The serialization context type.</typeparam>
            /// <param name="serializer">The parent serializer.</param>
            /// <param name="type">The type to create a serializer for.</param>
            /// <returns></returns>
            Serializer<C>.IForType GetSerializer<C>(Serializer<C> serializer, Type type)
                where C : SerializationContext<C>;
        }
        /// <summary>
        /// The interface for a kind pipeline.
        /// </summary>
        public interface IKindPipeline : IKind { }
        /// <summary>
        /// Creates a kind pipeline, based on an oredered sequence of kinds.
        /// The resulting pipeline will try each kind in order.
        /// </summary>
        /// <param name="kinds">The kinds to try.</param>
        /// <returns>A kind pipeline.</returns>
        public static IKindPipeline CreateKindPipeline(IEnumerable<IKind> kinds)
            => new MultipleKinds(kinds);
        /// <summary>
        /// This class represents the context for the request of a TypeDescriptor.
        /// This class is immutable.
        /// </summary>
        public class Context
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="type">The type a descriptor is being requested for.</param>
            /// <param name="cutoff">The type to function as a cutoff for reflection.</param>
            /// <param name="enclosing">A collection of enclosing type descriptor definitions.</param>
            public Context(Type type, Type cutoff = null, ImmutableStack<Type> enclosing = null)
            {
                Type = type;
                Cutoff = cutoff;
                Enclosing = enclosing ?? ImmutableStack<Type>.Empty;
            }
            /// <summary>
            /// Contains the type a descriptor is being requested for.
            /// </summary>
            public Type Type { get; }
            /// <summary>
            /// Contains the (base) type to funcvtion as a cutoff for reflection.
            /// This allows descriptors to be constructed per inheritance step.
            /// </summary>
            public Type Cutoff { get; }
            /// <summary>
            /// Contains a collection of enclosing type descriptor definitions.
            /// This prevents recursive definitions to result in infinite type descriptors or stack overflows.
            /// </summary>
            public ImmutableStack<Type> Enclosing { get; }

            /// <summary>
            /// Creates a new Context with a different Type.
            /// </summary>
            /// <param name="type">The new Type.</param>
            /// <returns>A new context.</returns>
            public Context WithType(Type type)
                => new Context(type, Cutoff, Enclosing);
            /// <summary>
            /// Creates a new Context with a different Cutoff type.
            /// </summary>
            /// <param name="cutoff">The new cutoff type.</param>
            /// <returns>A new context.</returns>
            public Context WithCutoff(Type cutoff)
                => new Context(Type, cutoff, Enclosing);
            /// <summary>
            /// Creates a new Context with a different Enclosing types stack.
            /// </summary>
            /// <param name="enclosing">A new enclosing types stack.</param>
            /// <returns>A new collection.</returns>
            public Context WithEnclosing(ImmutableStack<Type> enclosing)
                => new Context(Type, Cutoff, enclosing);
            /// <summary>
            /// Creates a new Context by pushing a new type on the Enclosing stack.
            /// </summary>
            /// <param name="type">The type to push on the stack.</param>
            /// <returns>A new context.</returns>
            public Context PushEnclosing(Type type)
                => new Context(Type, Cutoff, Enclosing.Push(type));
            /// <summary>
            /// Creates a new Context by popping a type off the Enclosing stack.
            /// </summary>
            /// <returns>A new context.</returns>
            public Context PopEnclosing()
                => new Context(Type, Cutoff, Enclosing.Pop());
        }
        internal static bool IsPrimitiveTypeDescriptor(TypeDescriptor desc)
        {
            if (desc is TypeDescriptor.Primitive || desc is TypeDescriptor.Null || desc is TypeDescriptor.Referable
                || desc is TypeDescriptor.Reference || desc is TypeDescriptor.Value)
                return true;
            else if (desc is TypeDescriptor.Union u)
                return u.Options.All(IsPrimitiveTypeDescriptor);
            else if (desc is TypeDescriptor.Intersection i)
                return i.Parts.All(IsPrimitiveTypeDescriptor);
            else if (desc is TypeDescriptor.Array a)
                return IsPrimitiveTypeDescriptor(a.ElementType);
            else
                return false;
        }
    }
}
