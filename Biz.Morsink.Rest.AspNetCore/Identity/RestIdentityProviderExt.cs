using Biz.Morsink.Identity;

namespace Biz.Morsink.Rest.AspNetCore
{
    public static class RestIdentityProviderExt
    {
        /// <summary>
        /// Tries to translate a general IIdentity&lt;object&gt; into a more specific type.
        /// </summary>
        /// <param name="objectId">The input identity value.</param>
        /// <param name="nullOnFailure">If no match is found, this boolean indicates whether to return a null or the original input identity value.</param>
        /// <returns>An identity value.</returns>
        public static IIdentity Parse(this IRestIdentityProvider provider, IIdentity<object> objectId, bool nullOnFailure)
            => provider.Parse(provider.Translate(objectId).Value.ToString(), nullOnFailure);
        /// <summary>
        /// Tries to translate a general IIdentity&lt;object&gt; into a more specific type.
        /// </summary>
        /// <typeparam name="T">The resource type to parse the value for.</typeparam>
        /// <param name="objectId">The input identity value.</param>
        /// <returns>An identity value, null if the match is unsuccessful.</returns>
        public static IIdentity<T> Parse<T>(this IRestIdentityProvider provider, IIdentity<object> objectId)
            => provider.Parse<T>(provider.Translate(objectId).Value.ToString());
        /// <summary>
        /// Converts any identity value for a known type into a pathstring.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string ToPath(this IRestIdentityProvider provider, IIdentity id)
        {
            var generalIdVal = provider.ToGeneralIdentity(id)?.Value;
            if (generalIdVal is RestPath restPath)
                return restPath.PathString;
            else
                return generalIdVal?.ToString();
        }
    }
}