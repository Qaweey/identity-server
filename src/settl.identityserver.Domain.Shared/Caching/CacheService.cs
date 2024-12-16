using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using System;
using System.Text;

namespace settl.identityserver.Domain.Shared.Caching
{
    public interface ICacheService
    {
        string Get(string key);

        void Set(string key, string value);

        void Remove(string key);
    }

    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public string Get(string key)
        {
            try
            {
                var value = _cache.GetString(key);

                return value ?? default;
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return null;
            }
        }

        public void Remove(string key)
        {
            Log.Information($"Removing {key} from cache");
            _cache.Remove(key);
        }

        public void Set(string key, string value)
        {
            try
            {
                _cache.SetString(key, value);
                Log.Information($"{key} set successfully");
            }
            catch (Exception ex)
            {
                Log.Error($"{key} not set successfully - {value}");
                Log.Error(ex, ex.Message);
            }
        }
    }
}