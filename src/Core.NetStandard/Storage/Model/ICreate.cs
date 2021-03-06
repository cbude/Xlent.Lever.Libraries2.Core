﻿using System.Threading.Tasks;

namespace Xlent.Lever.Libraries2.Core.Storage.Model
{
    /// <summary>
    /// Can create items of type <see cref="IStorableItem{TId}"/>.
    /// </summary>
    /// <typeparam name="TStorable">The type of objects to create in persistant storage.</typeparam>
    /// <typeparam name="TId">The type for the <see cref="IStorableItem{TId}.Id"/> property.</typeparam>
    public interface ICreate<TStorable, in TId>
        where TStorable : IStorableItem<TId>
    {
        /// <summary>
        /// Creates a new item in storage and returns the final result.
        /// </summary>
        /// <param name="item">The item to store.</param>
        /// <returns>The new item as it was saved, including a optional updated <see cref="IOptimisticConcurrencyControlByETag.ETag"/>.</returns>
        /// <remarks>
        /// The notes about <see cref="IOptimisticConcurrencyControlByETag.ETag"/> are only valid if the <see cref="IStorableItem{TId}"/> type implements <seealso cref="IOptimisticConcurrencyControlByETag"/>.
        /// </remarks>
        /// <seealso cref="IOptimisticConcurrencyControlByETag"/>
        Task<TStorable> CreateAsync(TStorable item);
    }
}
