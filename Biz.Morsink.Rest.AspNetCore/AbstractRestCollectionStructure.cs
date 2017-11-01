using Biz.Morsink.Rest.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore
{
    public abstract class AbstractRestCollectionStructure<C, E> : AbstractRestResourceCollection<C, E>
        where C : class
        where E : class
    {
        protected abstract AbstractStructure GetStructure();
        public abstract class AbstractStructure : IAspRestStructure
        {
            public IRestPathMapping CollectionPathMapping =>
                new RestPathMapping(typeof(C), BasePath + "?*", wildcardType: WildcardType);
            public IRestPathMapping ItemPathMapping =>
                new RestPathMapping(typeof(E), BasePath + "/*");

            public virtual IEnumerable<(Type,Func<object, IRestRepository>)> Repositories
            {
                get
                {
                    yield return (typeof(C), r => ((AbstractRestCollectionStructure<C, E>)r).GetCollectionRepository());
                    yield return (typeof(E), r => ((AbstractRestCollectionStructure<C, E>)r).GetItemRepository());
                }
            }

            public virtual IEnumerable<IRestPathMapping> PathMappings
            {
                get
                {
                    yield return CollectionPathMapping;
                    yield return ItemPathMapping;
                }
            }

            public abstract string BasePath { get; }
            public abstract Type WildcardType { get; }
            public abstract Type RootType { get; }
        }
    }
}
