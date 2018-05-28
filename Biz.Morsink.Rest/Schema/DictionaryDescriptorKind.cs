using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Biz.Morsink.Rest.Schema
{
    /// <summary>
    /// This class represents a type descriptor creator kind for dictionaries.
    /// A dictionary type implements IDictionary&lt;string, T&gt; from some T.
    /// </summary>
    public class DictionaryDescriptorKind : TypeDescriptorCreator.IKind
    {
        /// <summary>
        /// Singleton property.
        /// </summary>
        public static DictionaryDescriptorKind Instance { get; } = new DictionaryDescriptorKind();
        private DictionaryDescriptorKind() { }
        /// <summary>
        /// Gets a type descriptor for a dictionary type.
        /// A dictionary type implements IDictionary&lt;string, T&gt; from some T.
        /// This method returns null if the context does not represent a dictionary tyoe.
        /// </summary>
        public TypeDescriptor GetDescriptor(TypeDescriptorCreator creator, TypeDescriptorCreator.Context context)
        {
            var gendict = context.Type.GetTypeInfo().ImplementedInterfaces
                .Where(i => i.GetTypeInfo().GetGenericArguments().Length == 2
                   && i.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                .Select(i => i.GetGenericArguments())
                .FirstOrDefault();
            if (gendict != null && gendict[0] == typeof(string))
                return new TypeDescriptor.Dictionary(context.Type.ToString(), creator.GetDescriptor(context.WithType(gendict[1]).WithCutoff(null)));
            else if (typeof(IDictionary).IsAssignableFrom(context.Type))
                return new TypeDescriptor.Dictionary(context.Type.ToString(), TypeDescriptor.MakeAny());
            else
                return null;
        }
    }
}
