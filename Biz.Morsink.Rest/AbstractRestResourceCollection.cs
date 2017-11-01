using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Biz.Morsink.Identity;

namespace Biz.Morsink.Rest
{
    public abstract class AbstractRestResourceCollection<C, E> : IRestResourceCollection<C, E>
        where C : class
        where E : class
    {
        public class CollectionRepository : RestRepository<C>
        {
            private readonly IRestResourceCollection<C, E> source;

            public CollectionRepository(AbstractRestResourceCollection<C, E> source)
            {
                this.source = source;
                Register(new Get(this));
                Register(new Post(this));
            }
            private class Get : IRestGet<C, NoParameters>
            {
                private readonly CollectionRepository repo;

                public Get(CollectionRepository repo)
                {
                    this.repo = repo;
                }

                async ValueTask<RestResponse<C>> IRestGet<C, NoParameters>.Get(IIdentity<C> id, NoParameters parameters)
                    => Rest.Value(await repo.source.GetCollection(id)).ToResponse();
            }
            private class Post : IRestPost<C,NoParameters, E, E>
            {
                private readonly CollectionRepository repo;

                public Post(CollectionRepository repo)
                {
                    this.repo = repo;
                }

                async ValueTask<RestResponse<E>> IRestPost<C, NoParameters, E, E>.Post(IIdentity<C> target, NoParameters parameters, E entity)
                {
                    var res = await repo.source.Post(entity);
                    if (res == null)
                        return RestResult.BadRequest<E>(null).ToResponse();
                    else
                        return Rest.Value(res).ToResponse();
                }
            }
        }
        public class ItemRepository : RestRepository<E>
        {
            private readonly IRestResourceCollection<C, E> source;

            public ItemRepository(AbstractRestResourceCollection<C, E> source)
            {
                this.source = source;
                Register(new Get(this));
                Register(new Put(this));
                Register(new Delete(this));
            }
            private class Get : IRestGet<E, NoParameters>
            {
                private readonly ItemRepository repo;

                public Get(ItemRepository repo)
                {
                    this.repo = repo;
                }

                async ValueTask<RestResponse<E>> IRestGet<E, NoParameters>.Get(IIdentity<E> id, NoParameters parameters)
                {
                    var res = await repo.source.Get(id);
                    if (res == null)
                        return RestResult.NotFound<E>().ToResponse();
                    else
                        return Rest.Value(res).ToResponse();
                }
            }

            private class Put : IRestPut<E, NoParameters>
            {
                private readonly ItemRepository repo;
                public Put(ItemRepository repo)
                {
                    this.repo = repo;
                }

                async ValueTask<RestResponse<E>> IRestPut<E, NoParameters>.Put(IIdentity<E> id, NoParameters parameters, E entity)
                {
                    var res = await repo.source.Put(entity);
                    if (res == null)
                        return RestResult.BadRequest<E>(null).ToResponse();
                    else
                        return Rest.Value(res).ToResponse();
                }
            }
            private class Delete : IRestDelete<E, NoParameters>
            {
                private readonly ItemRepository repo;
                public Delete(ItemRepository repo)
                {
                    this.repo = repo;
                }

                async ValueTask<RestResponse<object>> IRestDelete<E, NoParameters>.Delete(IIdentity<E> id, NoParameters parameters)
                {
                    var res = await repo.source.Delete(id);

                    if (res)
                        return Rest.Value(new object()).ToResponse();
                    else
                        return RestResult.NotFound<object>().ToResponse();
                }
            }

        }

        protected AbstractRestResourceCollection()
        {

        }
        
        public virtual CollectionRepository GetCollectionRepository() => new CollectionRepository(this);
        public virtual ItemRepository GetItemRepository() => new ItemRepository(this);

        public abstract Task<bool> Delete(IIdentity<E> entityId);
        public abstract Task<E> Get(IIdentity<E> entityId);
        public abstract Task<C> GetCollection(IIdentity<C> collectionId);
        public abstract Task<E> Post(E entity);
        public abstract Task<E> Put(E entity);
    }
}
