using System;
using System.Runtime.Caching;
using DASN.WebApp.Models.DataModels;

namespace DASN.WebApp.Services
{
    public class ResourceProtectionService
    {
        public string GetKey(string token, string region) => $"{region}{token}";

        public string GetToken<T>(T resourceId, ApplicationUser user, TimeSpan? expire = null)
        {
            var token = Guid.NewGuid().ToString();
            MemoryCache.Default.Set(
                key: GetKey(token, user.Id),
                value: resourceId,
                absoluteExpiration:  DateTimeOffset.Now.Add(expire ?? TimeSpan.MaxValue)
            );
            return token;
        }

        public T GetResourceId<T>(string token, ApplicationUser user)
        {
            var key = GetKey(token, user.Id);
            var value = MemoryCache.Default.Get(key: key);
            if (value == null) return default(T);
            MemoryCache.Default.Remove(key);
            return (T)value;
        }


        public string GetToken<T>(T resourceId, TimeSpan? expire = null)
        {
            var token = Guid.NewGuid().ToString();
            MemoryCache.Default.Set(
                key: GetKey(token, string.Empty),
                value: resourceId,
                absoluteExpiration: DateTimeOffset.Now.Add(expire ?? TimeSpan.MaxValue)
            );
            return token;
        }

        public T GetResourceId<T>(string token)
        {
            var key = GetKey(token, string.Empty);
            var value = MemoryCache.Default.Get(key: key);
            if (value == null) return default(T);
            MemoryCache.Default.Remove(key);
            return (T)value;
        } 
    }
}