using Microsoft.Extensions.Caching.Memory;

namespace Gateway.Infrastructure.Services
{
    public interface IAppMemoryCache
    {
        bool TryGetValue<TItem>(object key, out TItem? value);
        IAppCacheEntry CreateEntry(object key);
    }

    public interface IAppCacheEntry : IDisposable
    {
        object Key { get; }
        object? Value { get; set; }

        TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
        DateTimeOffset? AbsoluteExpiration { get; set; }
        TimeSpan? SlidingExpiration { get; set; }
    }

    public sealed class AppMemoryCache : IAppMemoryCache
    {
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _inner;

        public AppMemoryCache(Microsoft.Extensions.Caching.Memory.IMemoryCache inner)
        {
            _inner = inner;
        }

        public bool TryGetValue<TItem>(object key, out TItem? value)
        {
            // This will bind to the framework extension and infer TItem correctly.
            return _inner.TryGetValue(key, out value);
        }

        public IAppCacheEntry CreateEntry(object key)
        {
            var entry = _inner.CreateEntry(key);
            return new AppCacheEntry(entry);
        }

        private sealed class AppCacheEntry : IAppCacheEntry
        {
            private readonly ICacheEntry _innerEntry;

            public AppCacheEntry(ICacheEntry innerEntry)
            {
                _innerEntry = innerEntry;
            }

            public object Key => _innerEntry.Key;

            public object? Value
            {
                get => _innerEntry.Value;
                set => _innerEntry.Value = value;
            }

            public TimeSpan? AbsoluteExpirationRelativeToNow
            {
                get => _innerEntry.AbsoluteExpirationRelativeToNow;
                set => _innerEntry.AbsoluteExpirationRelativeToNow = value;
            }

            public DateTimeOffset? AbsoluteExpiration
            {
                get => _innerEntry.AbsoluteExpiration;
                set => _innerEntry.AbsoluteExpiration = value;
            }

            public TimeSpan? SlidingExpiration
            {
                get => _innerEntry.SlidingExpiration;
                set => _innerEntry.SlidingExpiration = value;
            }

            public void Dispose() => _innerEntry.Dispose();
        }
    }
}