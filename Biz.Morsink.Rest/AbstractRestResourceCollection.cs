using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Metadata;
using Biz.Morsink.DataConvert;
using System.Threading;
using System.Reflection;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// Abstract base class for Rest resource collections.
    /// </summary>
    /// <typeparam name="C">The collection type.</typeparam>
    /// <typeparam name="E">The item tyope.</typeparam>
    public abstract class AbstractRestResourceCollection<C, E> : IRestResourceCollection<C, E>
    {
        /// <summary>
        /// Base class for the Collection repository.
        /// </summary>
        public class CollectionRepository : RestRepository<C>
        {
            /// <summary>
            /// Gets a reference to the containing collection.
            /// </summary>
            public AbstractRestResourceCollection<C, E> Source { get; }
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="source">The parent instance.</param>
            public CollectionRepository(AbstractRestResourceCollection<C, E> source)
            {
                Source = source;
                RegisterCapabilities();
            }
            /// <summary>
            /// Registers the repository's capabilities.
            /// </summary>
            protected virtual void RegisterCapabilities()
            {
                var get = RestCapabilityDescriptor.Create(typeof(IRestGet<C, Empty>))
                    .WithMethod(Source.GetType().GetTypeInfo().GetDeclaredMethod(nameof(AbstractRestResourceCollection<C, E>.GetCollection)));
                var post = RestCapabilityDescriptor.Create(typeof(IRestPost<C, Empty, E, E>))
                    .WithMethod(Source.GetType().GetTypeInfo().GetDeclaredMethod(nameof(AbstractRestResourceCollection<C, E>.Post)));

                RegisterSingle(new RestCapability<C>(get, new Get(this)));
                RegisterSingle(new RestCapability<C>(post, new Post(this)));
            }
            /// <summary>
            /// Default 'GET' implementation.
            /// </summary>
            protected class Get : IRestGet<C, Empty>
            {
                protected readonly CollectionRepository repo;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="repo">The parent repository.</param>
                public Get(CollectionRepository repo)
                {
                    this.repo = repo;
                }

                async ValueTask<RestResponse<C>> IRestGet<C, Empty>.Get(IIdentity<C> id, Empty parameters, CancellationToken cancellationToken)
                {
                    var conv = id.Provider.GetConverter(typeof(C), false).Convert(id.Value);
                    var cp = conv.To<CollectionParameters>();
                    if (cp.Limit <= 0 || cp.Skip < 0)
                        return RestResult.BadRequest<C>(new object()).ToResponse();
                    else
                        return Rest.Value(await repo.Source.GetCollection(id)).ToResponse();
                }
            }
            /// <summary>
            /// Default 'POST' implementation.
            /// </summary>
            protected class Post : IRestPost<C, Empty, E, E>
            {
                protected readonly CollectionRepository repo;
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="repo">The parent repository.</param>
                public Post(CollectionRepository repo)
                {
                    this.repo = repo;
                }

                async ValueTask<RestResponse<E>> IRestPost<C, Empty, E, E>.Post(IIdentity<C> target, Empty parameters, E entity, CancellationToken cancellationToken)
                {
                    var res = await repo.Source.Post(entity);

                    if (res == null)
                        return RestResult.BadRequest<E>(null).ToResponse();
                    else if (res is IHasIdentity<E> hid)
                        return Rest.Value(res).ToResponse().WithMetadata(new CreatedResource { Address = hid.Id });
                    else
                        return Rest.Value(res).ToResponse();
                }
            }
        }
        /// <summary>
        /// Base class for the Item repository.
        /// </summary>
        public class ItemRepository : RestRepository<E>
        {
            /// <summary>
            /// Gets a reference to the containing collection.
            /// </summary>
            public AbstractRestResourceCollection<C, E> Source { get; }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="source">The parent instance.</param>
            public ItemRepository(AbstractRestResourceCollection<C, E> source)
            {
                Source = source;
                var get = RestCapabilityDescriptor.Create(typeof(IRestGet<E, Empty>))
                    .WithMethod(source.GetType().GetTypeInfo().GetDeclaredMethod(nameof(AbstractRestResourceCollection<C, E>.Get)));
                var put = RestCapabilityDescriptor.Create(typeof(IRestPut<E, Empty>))
                    .WithMethod(source.GetType().GetTypeInfo().GetDeclaredMethod(nameof(AbstractRestResourceCollection<C, E>.Put)));
                var delete = RestCapabilityDescriptor.Create(typeof(IRestDelete<E, Empty>))
                    .WithMethod(source.GetType().GetTypeInfo().GetDeclaredMethod(nameof(AbstractRestResourceCollection<C, E>.Delete)));

                RegisterSingle(new RestCapability<E>(get, new Get(this)));
                RegisterSingle(new RestCapability<E>(put, new Put(this)));
                RegisterSingle(new RestCapability<E>(delete, new Delete(this)));
            }
            /// <summary>
            /// Default 'GET' implementation.
            /// </summary>
            protected class Get : IRestGet<E, Empty>
            {
                protected readonly ItemRepository repo;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="repo">The parent repository.</param>
                public Get(ItemRepository repo)
                {
                    this.repo = repo;
                }

                async ValueTask<RestResponse<E>> IRestGet<E, Empty>.Get(IIdentity<E> id, Empty parameters, CancellationToken cancellationToken)
                {
                    var res = await repo.Source.Get(id);
                    if (res == null)
                        return RestResult.NotFound<E>().ToResponse();
                    else
                        return Rest.Value(res).ToResponse();
                }
            }
            /// <summary>
            /// Default 'PUT' implementation.
            /// </summary>
            private class Put : IRestPut<E, Empty>
            {
                protected readonly ItemRepository repo;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="repo">The parent repository.</param>
                public Put(ItemRepository repo)
                {
                    this.repo = repo;
                }

                async ValueTask<RestResponse<E>> IRestPut<E, Empty>.Put(IIdentity<E> id, Empty parameters, E entity, CancellationToken cancellationToken)
                {
                    var res = await repo.Source.Put(entity);
                    if (res == null)
                        return RestResult.BadRequest<E>(null).ToResponse();
                    else
                        return Rest.Value(res).ToResponse();
                }
            }
            /// <summary>
            /// Default 'DELETE' implementation.
            /// </summary>
            private class Delete : IRestDelete<E, Empty>
            {
                protected readonly ItemRepository repo;

                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="repo">The parent repository.</param>
                public Delete(ItemRepository repo)
                {
                    this.repo = repo;
                }

                async ValueTask<RestResponse<object>> IRestDelete<E, Empty>.Delete(IIdentity<E> id, Empty parameters, CancellationToken cancellationToken)
                {
                    var res = await repo.Source.Delete(id);

                    if (res)
                        return Rest.Value(new object()).ToResponse();
                    else
                        return RestResult.NotFound<object>().ToResponse();
                }
            }

        }
        /// <summary>
        /// Constructor.
        /// </summary>
        protected AbstractRestResourceCollection()
        {

        }
        /// <summary>
        /// Gets the Collection repository.
        /// </summary>
        public virtual CollectionRepository GetCollectionRepository() => new CollectionRepository(this);
        /// <summary>
        /// Gets the Item repository.
        /// </summary>
        /// <returns></returns>
        public virtual ItemRepository GetItemRepository() => new ItemRepository(this);

        /// <summary>
        /// Delete an entity from the collection.
        /// </summary>
        /// <param name="entityId">The identity value for the entity.</param>
        /// <returns>True if the deletion was succesful. (Asynchronous)</returns>
        public abstract Task<bool> Delete(IIdentity<E> entityId);
        /// <summary>
        /// Retrieve an entity from the collection.
        /// </summary>
        /// <param name="entityId">The identity value for the entity.</param>
        /// <returns>The entity if it was foudn, null otherwise. (Asynchronous)</returns>
        public abstract Task<E> Get(IIdentity<E> entityId);
        /// <summary>
        /// Gets a collection slice from the collection.
        /// </summary>
        /// <param name="collectionId">The identity value for the collection slice.</param>
        /// <returns>The collection slice asynchronously.</returns>
        public abstract Task<C> GetCollection(IIdentity<C> collectionId);
        /// <summary>
        /// Inserts a new entity into the collection.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <returns>The inserted entity.</returns>
        public abstract Task<E> Post(E entity);
        /// <summary>
        /// Updates an entity in the collection.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>The updated entity.</returns>
        public abstract Task<E> Put(E entity);
    }
}
