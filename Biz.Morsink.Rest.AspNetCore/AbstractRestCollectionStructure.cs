using Biz.Morsink.Identity;
using Biz.Morsink.Rest.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    /// <summary>
    /// Abstract Root type for a collection structure of Rest components.
    /// </summary>
    /// <typeparam name="C">The Collection type.</typeparam>
    /// <typeparam name="E">The item type.</typeparam>
    /// <typeparam name="I">The type whose instances contain descriptive information about instances of the resource type.</typeparam>
    public abstract class AbstractRestCollectionStructure<C, E, I> : AbstractRestResourceCollection<C, E>
        where C : RestCollection<E, I>
        where I : IHasIdentity<E>
    {
        /// <summary>
        /// Gets the structure corresponding to this collection.
        /// </summary>
        protected abstract AbstractStructure GetStructure();

        /// <summary>
        /// Base class for collection structures.
        /// </summary>
        public abstract class AbstractStructure : IRestStructure
        {
            /// <summary>
            /// Gets the collection path mapping.
            /// </summary>
            public IRestPathMapping CollectionPathMapping =>
                new RestPathMapping(typeof(C), BasePath + "?*", wildcardTypes: WildcardTypes);
            /// <summary>
            /// Gets the item path mapping.
            /// </summary>
            public IRestPathMapping ItemPathMapping =>
                new RestPathMapping(typeof(E), BasePath + "/*");

            /// <summary>
            /// Gets all the path mappings.
            /// The default implementation returns path mappings for collection and item.
            /// </summary>
            public virtual IEnumerable<IRestPathMapping> PathMappings
            {
                get
                {
                    yield return CollectionPathMapping;
                    yield return ItemPathMapping;
                }
            }

            /// <summary>
            /// Returns the base path for this Rest structure.
            /// </summary>
            public abstract string BasePath { get; }
            /// <summary>
            /// Returns the wildcard type for retrieving collections.
            /// </summary>
            public abstract Type[] WildcardTypes { get; }
            /// <summary>
            /// Returns the root type for the structure.
            /// The default implementation returns the class the structure is nested in.
            /// </summary>
            public virtual Type RootType => GetType().DeclaringType;

            /// <summary>
            /// Registers all the components of the structure in a service collection.
            /// </summary>
            /// <param name="serviceCollection">The service collection to register all the components in.</param>
            /// <param name="lifetime">The lifetime scope of the root type.</param>
            public virtual void RegisterComponents(IServiceCollection serviceCollection, ServiceLifetime lifetime)
            {
                serviceCollection.Add(new ServiceDescriptor(RootType, RootType, lifetime));
                foreach (var mapping in PathMappings)
                    serviceCollection.AddSingleton(mapping);
                serviceCollection.AddScoped<IRestRepository>(
                    sp => ((AbstractRestCollectionStructure<C, E, I>)sp.GetService(RootType)).GetCollectionRepository());
                serviceCollection.AddScoped<IRestRepository>(
                    sp => ((AbstractRestCollectionStructure<C, E, I>)sp.GetService(RootType)).GetItemRepository());
                serviceCollection.AddScoped<IRestRepository<C>>(
                    sp => ((AbstractRestCollectionStructure<C, E, I>)sp.GetService(RootType)).GetCollectionRepository());
                serviceCollection.AddScoped<IRestRepository<E>>(
                    sp => ((AbstractRestCollectionStructure<C, E, I>)sp.GetService(RootType)).GetItemRepository());

                serviceCollection.AddScoped<IDynamicLinkProvider<C>, RestCollectionLinks<C, E, I>>();
            }
        }

    }
    /// <summary>
    /// Abstract Root type for a collection structure of Rest components.
    /// The descriptive type has been set to the item type itself.
    /// </summary>
    /// <typeparam name="C">The Collection type.</typeparam>
    /// <typeparam name="E">The item type.</typeparam>
    public abstract class AbstractRestCollectionStructure<C, E> : AbstractRestCollectionStructure<C, E, E>
        where C : RestCollection<E, E>
        where E : IHasIdentity<E>
    {
    }
}
