using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public interface IRestResourceCollection<C, E>
    {
        C GetCollection(IIdentity<C> collectionId);
        E Get(IIdentity<E> entityId);
        E Put(E entity);
        E Post(E entity);
        bool Delete(IIdentity<E> entityId);
    }
}
