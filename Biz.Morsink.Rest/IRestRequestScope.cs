using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// This interface defines operations for the scope of a single Rest request.
    /// </summary>
    public interface IRestRequestScope
    {
        /// <summary>
        /// Sets an item in the scope.
        /// </summary>
        /// <typeparam name="T">The type of the scope item.</typeparam>
        /// <param name="item">The scope item to set in the scope.</param>
        void SetScopeItem<T>(T item);
        /// <summary>
        /// Tries to get an item from the scope.
        /// </summary>
        /// <typeparam name="T">The type of the scope item.</typeparam>
        /// <param name="result">Output variable containing the item that was found.</param>
        /// <returns>True if the item was found, false otherwise.</returns>
        bool TryGetScopeItem<T>(out T result);
        /// <summary>
        /// Tries to remove an item from the scope.
        /// </summary>
        /// <typeparam name="T">The type of the scope item.</typeparam>
        /// <param name="result">Output variable containing the item that was found and removed.</param>
        /// <returns>True if the item was found and removed, false otherwise.</returns>
        bool TryRemoveScopeItem<T>(out T result);
    }
}
