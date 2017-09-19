﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Biz.Morsink.Rest.Utils
{
    /// <summary>
    /// An immutable keyed collection of typed objects.
    /// Every element is 'keyed' by a single type (statically at registration), that the element is assignable to.
    /// </summary>
    public class TypeKeyedDictionary
    {
        /// <summary>
        /// Gets the empty TypeKeyedDictionary.
        /// </summary>
        public static TypeKeyedDictionary Empty { get; } = new TypeKeyedDictionary(ImmutableDictionary<Type, object>.Empty);
        private readonly ImmutableDictionary<Type, object> objects;
        private TypeKeyedDictionary(ImmutableDictionary<Type, object> objects)
        {
            this.objects = objects;
        }
        /// <summary>
        /// Adds an object for some type.
        /// </summary>
        /// <typeparam name="T">The type key of the value.</typeparam>
        /// <param name="obj">The value to store.</param>
        /// <returns>A new TypeKeyedDictionary with the modification as specified by this function call.</returns>
        public TypeKeyedDictionary Add<T>(T obj)
            => new TypeKeyedDictionary(objects.Add(typeof(T), obj));
        /// <summary>
        /// Tries to get the value for a key T.
        /// </summary>
        /// <typeparam name="T">The key to search for.</typeparam>
        /// <param name="value">The value found for key T, default(T) otherwise.</param>
        /// <returns>True if the key was found.</returns>
        public bool TryGet<T>(out T value)
        {
            if (objects.TryGetValue(typeof(T), out var val))
            {
                value = (T)val;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }
        /// <summary>
        /// Executes an action if the key is present in the TypeKeyedDictionary.
        /// </summary>
        /// <typeparam name="T">The key to search for.</typeparam>
        /// <param name="act">The action to execute if the key is found.</param>
        public void Execute<T>(Action<T> act)
        {
            if (TryGet(out T t))
                act(t);
        }
    }
}