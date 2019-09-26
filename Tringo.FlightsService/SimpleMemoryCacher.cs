using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tringo.FlightsService
{
    public interface ISimpleMemoryCacher
    {
        TEntity GetFromCache<TEntity>(string key, Func<TEntity> valueFactory, TimeSpan span);

        Task<T> GetFromCacheAsync<T>(string key, Func<Task<T>> factory, TimeSpan expirationSpan);
    }

    public class SimpleMemoryCacher : ISimpleMemoryCacher
    {
        private readonly IMemoryCache _memoryCache;

        public SimpleMemoryCacher(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public TEntity GetFromCache<TEntity>(string key, Func<TEntity> valueFactory, TimeSpan span)
        {
            var newValue = new Lazy<TEntity>(valueFactory);
            var value = _memoryCache.GetOrCreate(key, cacheEntry =>
            {
                cacheEntry.SetAbsoluteExpiration(DateTimeOffset.Now.AddSeconds((int)span.TotalSeconds));
                return newValue;
            });
            return (value ?? newValue).Value; // Lazy<T> handles the locking itself
        }

        public Task<T> GetFromCacheAsync<T>(
                string key, Func<Task<T>> factory, TimeSpan expirationSpan)
        {
            return _memoryCache.GetOrCreateAsync(key, async cacheEntry =>
            {
                var cts = new CancellationTokenSource();
                cacheEntry.AddExpirationToken(new CancellationChangeToken(cts.Token));
                var value = await factory().ConfigureAwait(false);
                cts.CancelAfter(expirationSpan);
                return value;
            });
        }
    }
}
