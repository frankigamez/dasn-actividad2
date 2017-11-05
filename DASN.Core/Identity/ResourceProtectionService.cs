using System;
using System.Runtime.Caching;
using DASN.Core.Models.DataModels;

namespace DASN.Core.Identity
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
                absoluteExpiration:  DateTimeOffset.Now.Add(expire ?? TimeSpan.FromMinutes(5))
            );
            return token;
        }

        public T GetResourceId<T>(string token, ApplicationUser user)
        {
            var value = MemoryCache.Default.Get(
                key: GetKey(token, user.Id));

            if (value == null)
                return default(T);           
            return (T)value;
        }
    }
}