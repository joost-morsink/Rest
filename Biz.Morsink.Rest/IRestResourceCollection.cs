using Biz.Morsink.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// This interface represents a collection with a restful interface.
    /// </summary>
    /// <typeparam name="C">The collection type.</typeparam>
    /// <typeparam name="E">The entity type.</typeparam>
    public interface IRestResourceCollection<C, E>
    {
        /// <summary>
        /// Gets a collection. (Or collection slice, based on the identity value) 
        /// </summary>
        /// <param name="collectionId">The collection slice's identity value.</param>
        /// <returns></returns>
        Task<C> GetCollection(IIdentity<C> collectionId);
        /// <summary>
        /// Gets an entity from the collection.
        /// </summary>
        /// <param name="entityId">The identity value of the entity.</param>
        /// <returns></returns>
        Task<E> Get(IIdentity<E> entityId);
        /// <summary>
        /// Stores an entity in the collection.
        /// </summary>
        /// <param name="entity">The entity to store.</param>
        /// <returns>The entity as it is contained in the collection after the store operation.</returns>
        Task<E> Put(E entity);
        /// <summary>
        /// Stores a new entity to the collection.
        /// </summary>
        /// <param name="entity">The entity to store.</param>
        /// <returns>The entity as it is contained in the collection after the store operation.</returns>
        Task<E> Post(E entity);
        /// <summary>
        /// Deletes an entity from the collection.
        /// </summary>
        /// <param name="entityId">The entity's identity value.</param>
        /// <returns>
        /// True if the entity was successfully removed, false otherwise. 
        /// Failure may be due to the entity not being found.
        /// </returns>
        Task<bool> Delete(IIdentity<E> entityId);
    }
}
