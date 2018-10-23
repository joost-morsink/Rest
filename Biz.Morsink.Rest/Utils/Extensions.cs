using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Jobs;
using Biz.Morsink.Rest.Schema;
using Biz.Morsink.Rest.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Biz.Morsink.Rest.Utils
{
    /// <summary>
    /// Extension methods for Rest classes.
    /// </summary>
    public static class Extensions
    {

        /// <summary>
        /// Determines if a Link is allowed to be opened by a principal according to an authorization provider.
        /// </summary>
        /// <param name="link">The Link to check.</param>
        /// <param name="provider">The authorization provider that will determine if access is allowed.</param>
        /// <param name="principal">The principal requesting access to the link.</param>
        /// <returns>True if access is allowed.</returns>
        public static bool IsAllowedBy(this Link link, IAuthorizationProvider provider, ClaimsPrincipal principal)
            => provider == null || provider.IsAllowed(principal, link.Target, GetCapabilityString(link.Capability));

        /// <summary>
        /// Determines if a Link is allowed to be opened by a principal according to an authorization provider.
        /// </summary>
        /// <param name="link">The Link to check.</param>
        /// <param name="provider">The authorization provider that will determine if access is allowed.</param>
        /// <param name="user">The user requesting access to the link.</param>
        /// <returns>True if access is allowed.</returns>        
        public static bool IsAllowedBy(this Link link, IAuthorizationProvider provider, IUser user)
            => provider == null || provider.IsAllowed(user?.Principal, link.Target, GetCapabilityString(link.Capability));

        public static string GetCapabilityString(this Link link)
            => link.Capability.GetTypeInfo().GetCustomAttribute<CapabilityAttribute>().Name;

        private static string GetCapabilityString(Type capability)
            => capability.GetTypeInfo().GetCustomAttribute<CapabilityAttribute>().Name;

        /// <summary>
        /// Gets a job controller from the job store.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="id">The identity value for the job controller.</param>
        /// <returns>A RestJobController instance if it was found, null otherwise.</returns>
        public static ValueTask<RestJobController> GetController(this IRestJobStore store, IIdentity<RestJobController> id)
            => store.GetController(id as IIdentity<RestJob, RestJobController>);

        /// <summary>
        /// Gets a rest documentation string for a collection of RestDocumentationAttributes.
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns>A rest documentation string.</returns>
        public static string GetRestDocumentation(this IEnumerable<RestDocumentationAttribute> attributes)
        {
            var docs = from a in attributes
                       where a.Format == "text/plain" || a.Format == "text/markdown"
                       select a.Documentation;
            if (docs.Any())
                return string.Join(Environment.NewLine, docs);
            else
                return null;
        }
        /// <summary>
        /// Gets a rest documentation string for the MethodInfo.
        /// </summary>
        /// <param name="method">The MethodInfo.</param>
        /// <returns>A rest documentation string.</returns>
        public static string GetRestDocumentation(this MethodInfo method)
            => method.GetCustomAttributes<RestDocumentationAttribute>().GetRestDocumentation();
        /// <summary>
        /// Gets a rest documentation string for the ParameterInfo.
        /// </summary>
        /// <param name="parameter">The ParameterInfo.</param>
        /// <returns>A rest documentation string.</returns>
        public static string GetRestDocumentation(this ParameterInfo parameter)
            => parameter.GetCustomAttributes<RestDocumentationAttribute>().GetRestDocumentation();
        /// <summary>
        /// Gets a rest documentation string for the PropertyInfo.
        /// </summary>
        /// <param name="property">The PropertyInfo.</param>
        /// <returns>A rest documentation string.</returns>
        public static string GetRestDocumentation(this PropertyInfo property)
            => property.GetCustomAttributes<RestDocumentationAttribute>().GetRestDocumentation();
        /// <summary>
        /// Gets a rest documentation string for the Type.
        /// </summary>
        /// <param name="type">The Type.</param>
        /// <returns>A rest documentation string.</returns>
        public static string GetRestDocumentation(this Type type)
            => type.GetCustomAttributes<RestDocumentationAttribute>().GetRestDocumentation();

        /// <summary>
        /// Gets a collection of RestMetaDataAttributes for the MethodInfo.
        /// </summary>
        /// <param name="method">The MethodInfo.</param>
        /// <returns>A collection of RestMetaDataAttributes.</returns>
        public static IEnumerable<RestMetaDataAttribute> GetRestMetaDataAttributes(this MethodInfo method)
            => method.GetCustomAttributes<RestMetaDataAttribute>();
        /// <summary>
        /// Gets a collection of RestMetaDataInAttributes for the MethodInfo.
        /// </summary>
        /// <param name="method">The MethodInfo.</param>
        /// <returns>A collection of RestMetaDataInAttributes.</returns>
        public static IEnumerable<RestMetaDataInAttribute> GetRestMetaDataInAttributes(this MethodInfo method)
            => method.GetCustomAttributes<RestMetaDataInAttribute>();
        /// <summary>
        /// Gets a collection of RestMetaDataOutAttributes for the MethodInfo.
        /// </summary>
        /// <param name="method">The MethodInfo.</param>
        /// <returns>A collection of RestMetaDataOutAttributes.</returns>
        public static IEnumerable<RestMetaDataOutAttribute> GetRestMetaDataOutAttributes(this MethodInfo method)
            => method.GetCustomAttributes<RestMetaDataOutAttribute>();

        /// <summary>
        /// Checks whether the MethodInfo has any RestMetaDataInAttributes for some type.
        /// </summary>
        /// <typeparam name="T">The type to check for in the in-attributes.</typeparam>
        /// <param name="method">The MethodInfo.</param>
        /// <returns>True if the MethodInfo has any RestMetaDataInAttributes for type T.</returns>
        public static bool HasMetaDataInAttribute<T>(this MethodInfo method)
            => method.GetRestMetaDataInAttributes().Any(a => a.Type == typeof(T));
        /// <summary>
        /// Checks whether the MethodInfo has any RestMetaDataOutAttributes for some type.
        /// </summary>
        /// <typeparam name="T">The type to check for in the out-attributes.</typeparam>
        /// <param name="method">The MethodInfo.</param>
        /// <returns>True if the MethodInfo has any RestMetaDataOutAttributes for type T.</returns>
        public static bool HasMetaDataOutAttribute<T>(this MethodInfo method)
            => method.GetRestMetaDataOutAttributes().Any(a => a.Type == typeof(T));

        /// <summary>
        /// Gets all PropertyInfo objects for parameter-types specified with the RestParameterAttribute.
        /// </summary>
        /// <param name="method">The MethodInfo.</param>
        /// <returns>A collection of PropertyInfo objects.</returns>
        public static IEnumerable<PropertyInfo> GetRestParameterProperties(this MethodInfo method)
            => method.GetCustomAttributes<RestParameterAttribute>()
                .SelectMany(a => a.Type.GetProperties());

        /// <summary>
        /// Checks whether a property is considered 'required' by an API.
        /// </summary>
        /// <param name="pi">The PropertyInfo.</param>
        /// <returns>True if the property is considered 'required'.</returns>
        public static bool IsRequired(this PropertyInfo pi)
        {
            return pi.CanWrite && pi.CanRead && pi.GetCustomAttributes<RequiredAttribute>().Any()
                || !pi.CanWrite && pi.CanRead
                    && pi.DeclaringType.GetConstructors()
                        .Where(c => !c.IsStatic)
                        .SelectMany(c => c.GetParameters())
                        .Where(p => string.Equals(p.Name, pi.Name, StringComparison.InvariantCultureIgnoreCase))
                        .SelectMany(p => p.GetCustomAttributes<OptionalAttribute>())
                        .Any();
        }
        /// <summary>
        /// Constructs a new Rest Value with a different set of links.
        /// </summary>
        /// <param name="restValue">The original Rest Value.</param>
        /// <param name="links">The set of links to put on the new Rest Value.</param>
        /// <returns>A new Rest Value with the specified set of links.</returns>
        public static IRestValue WithLinks(this IRestValue restValue, IEnumerable<Link> links)
            => restValue.Manipulate(_ => links, rv => rv.Embeddings);
        /// <summary>
        /// Constructs a new Rest Value with an added set of links.
        /// </summary>
        /// <param name="restValue">The original Rest Value.</param>
        /// <param name="links">The set of links to add to the new Rest Value.</param>
        /// <returns>A new Rest Value with the specified set of links added.</returns>
        public static IRestValue AddLinks(this IRestValue restValue, IEnumerable<Link> links)
            => restValue.Manipulate(rv => rv.Links.Concat(links), rv => rv.Embeddings);
        /// <summary>
        /// Constructs a new Rest Value with an added link.
        /// </summary>
        /// <param name="restValue">The original Rest Value.</param>
        /// <param name="links">The link to add to the new Rest Value.</param>
        /// <returns>A new Rest Value with the specified link added.</returns>
        public static IRestValue AddLink(this IRestValue restValue, Link link)
            => restValue.Manipulate(rv => rv.Links.Append(link), rv => rv.Embeddings);
        /// <summary>
        /// Constructs a new Rest Value with a different set of embeddings.
        /// </summary>
        /// <param name="restValue">The original Rest Value.</param>
        /// <param name="embeddings">The set of embeddings to put on the new Rest Value.</param>
        /// <returns>A new Rest Value with the specified set of embeddings.</returns>
        public static IRestValue WithEmbeddings(this IRestValue restValue, IEnumerable<object> embeddings)
            => restValue.Manipulate(rv => rv.Links, _ => embeddings);
        /// <summary>
        /// Constructs a new Rest Value with a different set of links.
        /// </summary>
        /// <typeparam name="T">The type of the Rest Value's underlying value.</typeparam>
        /// <param name="restValue">The original Rest Value.</param>
        /// <param name="links">The set of links to put on the new Rest Value.</param>
        /// <returns>A new Rest Value with the specified set of links.</returns>
        public static IRestValue<T> WithLinks<T>(this IRestValue<T> restValue, IEnumerable<Link> links)
            => restValue.Manipulate(_ => links, rv => rv.Embeddings);
        /// <summary>
        /// Constructs a new Rest Value with an added set of links.
        /// </summary>
        /// <typeparam name="T">The type of the Rest Value's underlying value.</typeparam>
        /// <param name="restValue">The original Rest Value.</param>
        /// <param name="links">The set of links to add to the new Rest Value.</param>
        /// <returns>A new Rest Value with the specified set of links added.</returns>
        public static IRestValue<T> AddLinks<T>(this IRestValue<T> restValue, IEnumerable<Link> links)
            => restValue.Manipulate(rv => rv.Links.Concat(links), rv => rv.Embeddings);
        /// <summary>
        /// Constructs a new Rest Value with an added link.
        /// </summary>
        /// <typeparam name="T">The type of the Rest Value's underlying value.</typeparam>
        /// <param name="restValue">The original Rest Value.</param>
        /// <param name="links">The link to add to the new Rest Value.</param>
        /// <returns>A new Rest Value with the specified link added.</returns>
        public static IRestValue<T> AddLink<T>(this IRestValue<T> restValue, Link link)
            => restValue.Manipulate(rv => rv.Links.Append(link), rv => rv.Embeddings);
        /// <summary>
        /// Constructs a new Rest Value with a different set of embeddings.
        /// </summary>
        /// <typeparam name="T">The type of the Rest Value's underlying value.</typeparam>
        /// <param name="restValue">The original Rest Value.</param>
        /// <param name="embeddings">The set of embeddings to put on the new Rest Value.</param>
        /// <returns>A new Rest Value with the specified set of embeddings.</returns>
        public static IRestValue<T> WithEmbeddings<T>(this IRestValue<T> restValue, IEnumerable<object> embeddings)
            => restValue.Manipulate(rv => rv.Links, _ => embeddings);
        /// <summary>
        /// Constructs a new Rest Value with a different set of links.
        /// </summary>
        /// <typeparam name="T">The type of the Rest Value's underlying value.</typeparam>
        /// <param name="restValue">The original Rest Value.</param>
        /// <param name="links">The set of links to put on the new Rest Value.</param>
        /// <returns>A new Rest Value with the specified set of links.</returns>
        public static RestValue<T> WithLinks<T>(this RestValue<T> restValue, IEnumerable<Link> links)
            => restValue.Manipulate(_ => links, rv => rv.Embeddings);
        /// <summary>
        /// Constructs a new Rest Value with a different set of embeddings.
        /// </summary>
        /// <typeparam name="T">The type of the Rest Value's underlying value.</typeparam>
        /// <param name="restValue">The original Rest Value.</param>
        /// <param name="embeddings">The set of embeddings to put on the new Rest Value.</param>
        /// <returns>A new Rest Value with the specified set of embeddings.</returns>
        public static RestValue<T> WithEmbeddings<T>(this RestValue<T> restValue, IEnumerable<object> embeddings)
            => restValue.Manipulate(rv => rv.Links, _ => embeddings);
        /// <summary>
        /// Makes a rest value ready to accept lazily evaluated modifications.
        /// </summary>
        /// <typeparam name="T">The type of the Rest Value's underlying value.</typeparam>
        /// <param name="restValue">The original Rest Value.</param>
        /// <returns>A lazy variant of the specified Rest value.</returns>
        public static LazyRestValue<T> ToLazy<T>(this IRestValue<T> restValue)
            => restValue as LazyRestValue<T> ?? new LazyRestValue<T>(() => restValue.Value, () => restValue.Links, () => restValue.Embeddings);
        /// <summary>
        /// Converts a Lazy of some Rest value to a LazyRestValue.
        /// </summary>
        /// <typeparam name="T">The type of the Rest Value's underlying value.</typeparam>
        /// <param name="lazyRestValue">A Lazy of some Rest value.</param>
        /// <returns>A lazy variant of the specified Rest value.</returns>
        public static LazyRestValue<T> ExtractLazy<T>(this Lazy<IRestValue<T>> lazyRestValue)
            => new LazyRestValue<T>(() => lazyRestValue.Value.Value, () => lazyRestValue.Value.Links, () => lazyRestValue.Value.Embeddings);
        /// <summary>
        /// Converts the value to a successful RestResult.
        /// </summary>
        /// <typeparam name="T">The type of the Rest Value's underlying value.</typeparam>
        /// <param name="src">The rest value to convert.</param>
        /// <returns>A successful RestResult.</returns>
        public static RestResult<T>.Success ToResult<T>(this IRestValue<T> src)
            => new RestResult<T>.Success(src);
        /// <summary>
        /// Converts the value to a successful RestResponse.
        /// </summary>
        /// <typeparam name="T">The type of the Rest Value's underlying value.</typeparam>
        /// <param name="src">The rest value to convert.</param>
        /// <param name="metadata">Optional metadata collection for the response.</param>
        /// <returns>A successful RestResponse.</returns>
        public static RestResponse<T> ToResponse<T>(this IRestValue<T> src, TypeKeyedDictionary metadata = null)
            => src.ToResult().ToResponse(metadata);
        /// <summary>
        /// Converts the value to a ValueTask containing a successful RestResponse.
        /// </summary>
        /// <typeparam name="T">The type of the Rest Value's underlying value.</typeparam>
        /// <param name="src">The rest value to convert.</param>
        /// <param name="metadata">Optional metadata collection for the response.</param>
        /// <returns>A successful RestResponse wrapped in a ValueTask.</returns>
        public static ValueTask<RestResponse<T>> ToResponseAsync<T>(this IRestValue<T> src, TypeKeyedDictionary metadata = null)
            => src.ToResponse(metadata).ToAsync();

        /// <summary>
        /// Executes an action if the response has a success result.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="response">The response.</param>
        /// <param name="act">The action.</param>
        public static void OnSuccess<T>(this RestResponse<T> response, Action<T> act)
        {
            if (response.Result is RestResult<T>.Success success)
                act(success.Value);
        }

        /// <summary>
        /// Checks whether a scope item exists.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="scope">The scope.</param>
        /// <returns>True if the item exists in the scope.</returns>
        public static bool HasScopeItem<T>(this IRestRequestScope scope)
            => scope.TryGetScopeItem<T>(out var _);
        /// <summary>
        /// Gets an item from the scope, and adds one if it is not present yet.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="scope">The scope.</param>
        /// <param name="default">The default value.</param>
        /// <returns>A scope item of type T.</returns>
        public static T GetOrAddScopeItem<T>(this IRestRequestScope scope, T @default)
        {
            if (scope.TryGetScopeItem<T>(out var res))
                return res;
            scope.SetScopeItem(@default);
            return @default;
        }
        /// <summary>
        /// Gets an item from the scope, and throws an exception if it cannot find it.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="scope">The scope.</param>
        /// <returns>A scope item of type T.</returns>
        public static T GetScopeItem<T>(this IRestRequestScope scope)
            => scope.TryGetScopeItem<T>(out var result) ? result : throw new IndexOutOfRangeException();
        /// <summary>
        /// Gets an item from the scope, and adds one if it is not present yet.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="scope">The scope.</param>
        /// <param name="default">The default value.</param>
        /// <returns>A scope item of type T.</returns>
        public static T GetOrAddScopeItem<T>(this IRestRequestScope scope, Func<T> @default)
        {
            if (scope.TryGetScopeItem<T>(out var res))
                return res;
            res = @default();
            scope.SetScopeItem(res);
            return res;
        }
        /// <summary>
        /// Modifies an item in the scope.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="scope">The scope.</param>
        /// <param name="modifier">A modifier function.</param>
        /// <param name="default">The default value.</param>
        public static void ModifyScopeItem<T>(this IRestRequestScope scope, Func<T, T> modifier, T @default = default)
        {
            if (scope.TryGetScopeItem<T>(out var t))
                scope.SetScopeItem(modifier(t));
            else
                scope.SetScopeItem(modifier(@default));
        }
        /// <summary>
        /// Removes an item from the scope.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="scope">The scope.</param>
        public static void RemoveScopeItem<T>(this IRestRequestScope scope)
            => scope.TryRemoveScopeItem<T>(out var _);

        /// <summary>
        /// Allocates a Runner. 
        /// A runner is able to run a piece of code with a scope item, and restores the old situation after the run.
        /// </summary>
        /// <typeparam name="T">The type of the scope item.</typeparam>
        /// <param name="scope">The scope.</param>
        /// <param name="item">The scope item.</param>
        /// <returns>A runner.</returns>
        public static Runner<T> With<T>(this IRestRequestScope scope, T item)
            => new Runner<T>(scope, item);

        /// <summary>
        /// Allocates a Runner. 
        /// A runner is able to run a piece of code with a scope item, and restores the old situation after the run.
        /// </summary>
        /// <typeparam name="T">The type of the scope item.</typeparam>
        /// <param name="scope">The scope.</param>
        /// <param name="f">A function to modify the scope item.</param>
        /// <param name="default">A default value to pass to the function.</param>
        /// <returns>A runner.</returns>
        public static Runner<T> With<T>(this IRestRequestScope scope, Func<T, T> f, T @default = default)
            => new Runner<T>(scope, f(scope.TryGetScopeItem<T>(out var res) ? res : @default));
        /// <summary>
        /// A Runner is able to run a piece of code with a scope item, and restores the old situation after the run.
        /// </summary>
        /// <typeparam name="T">The type of the scope item.</typeparam>
        public struct Runner<T>
        {
            private readonly IRestRequestScope scope;
            private readonly T item;

            internal Runner(IRestRequestScope scope, T item)
            {
                this.scope = scope;
                this.item = item;
            }

            /// <summary>
            /// Runs a function with the specified scope item.
            /// </summary>
            /// <typeparam name="R">The return type of the function.</typeparam>
            /// <param name="f">The function.</param>
            /// <returns>The function's result.</returns>
            public R Run<R>(Func<R> f)
            {
                if (scope.TryGetScopeItem<T>(out var old))
                {
                    try
                    {
                        scope.SetScopeItem(item);
                        return f();
                    }
                    finally
                    {
                        scope.SetScopeItem(old);
                    }
                }
                else
                {
                    try
                    {
                        scope.SetScopeItem(item);
                        return f();
                    }
                    finally
                    {
                        scope.RemoveScopeItem<T>();
                    }
                }
            }
            /// <summary>
            /// Runs an action with the specified scope item.
            /// </summary>
            /// <param name="act">The action.</param>
            public void Run(Action act)
                => Run(() => { act(); return 0; });
        }
        public static IEnumerable<SValidation.Message> Validate(this ITypeDescriptorCreator typeDescriptorCreator, SItem item, TypeDescriptor desc)
            => item.Validate(desc, typeDescriptorCreator, DataConvert.DataConverter.Default);
    }
}
