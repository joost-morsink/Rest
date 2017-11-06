using Biz.Morsink.Identity;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Biz.Morsink.Rest
{
    /// <summary>
    /// This class represents a Rest request, where parsing is delayed until the requested type is known.
    /// </summary>
    public class RestRequest
    {
        private readonly Func<Type, object> bodyParser;
        private readonly CancellationTokenSource cancellationTokenSource;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="capability">The requested capability.</param>
        /// <param name="address">An resource identity value is used to 'address' the capability on some resource.</param>
        /// <param name="parameters">Parameters for the request.</param>
        /// <param name="bodyParser">A function that is capable of parsing the body, given the body type is known.</param>
        /// <param name="metadata">Metadata for the Rest request.</param>
        public RestRequest(string capability, IIdentity address, RestParameterCollection parameters, Func<Type, object> bodyParser, TypeKeyedDictionary metadata)
        {
            Capability = capability;
            Address = address;
            Parameters = parameters ?? RestParameterCollection.Empty;
            this.bodyParser = bodyParser ?? (ty => new object());
            Metadata = metadata ?? TypeKeyedDictionary.Empty;
            cancellationTokenSource = new CancellationTokenSource();
        }
        /// <summary>
        /// Creates a new RestRequest.
        /// </summary>
        /// <param name="capability">The requested capability.</param>
        /// <param name="address">An resource identity value is used to 'address' the capability on some resource.</param>
        /// <param name="parameters">Parameters for the request.</param>
        /// <param name="bodyParser">A function that is capable of parsing the body, given the body type is known.</param>
        /// <param name="metadata">Metadata for the Rest request.</param>
        /// <returns>A new RestRequest.</returns>
        public static RestRequest Create(string capability, IIdentity address, IEnumerable<KeyValuePair<string, string>> parameters = null, Func<Type, object> bodyParser = null, TypeKeyedDictionary metadata = null)
            => new RestRequest(capability, address, RestParameterCollection.Create(parameters), bodyParser, metadata);
        /// <summary>
        /// Creates a new RestRequest.
        /// </summary>
        /// <param name="capability">The requested capability.</param>
        /// <param name="address">An resource identity value is used to 'address' the capability on some resource.</param>
        /// <param name="parameters">Parameters for the request.</param>
        /// <param name="bodyParser">A function that is capable of parsing the body, given the body type is known.</param>
        /// <param name="metadata">Metadata for the Rest request.</param>
        /// <returns>A new RestRequest.</returns>        
        public static RestRequest CreateWithTuples(string capability, IIdentity address, IEnumerable<(string, string)> parameters = null, Func<Type, object> bodyParser = null, TypeKeyedDictionary metadata = null)
            => new RestRequest(capability, address, RestParameterCollection.Create(parameters), bodyParser, metadata);

        /// <summary>
        /// Gets the capability name.
        /// </summary>
        public string Capability { get; }
        /// <summary>
        /// Gets the address resource's identity value.
        /// </summary>
        public IIdentity Address { get; }
        /// <summary>
        /// Gets the parameters for the Rest request. 
        /// The concept of parameters map to query string parameters.
        /// </summary>
        public RestParameterCollection Parameters { get; }
        /// <summary>
        /// Gets the metadata for the Rest request.
        /// The concept of metadata map to HTTP headers.
        /// </summary>
        public TypeKeyedDictionary Metadata { get; }
        /// <summary>
        /// Creates a new RestRequest with added metadata.
        /// </summary>
        /// <typeparam name="X">The type of metadata to include in the request.</typeparam>
        /// <param name="data">An instance of metadata to include in the request.</param>
        /// <returns>A new RestRequest with added metadata.</returns>
        public RestRequest AddMetadata<X>(X data)
            => new RestRequest(Capability, Address, Parameters, bodyParser, Metadata.Set(data));

        /// <summary>
        /// Parses the raw body into an object of type E.
        /// </summary>
        /// <typeparam name="E">The desired body type.</typeparam>
        /// <returns>A typed RestRequest</returns>
        public virtual RestRequest<E> ParseBody<E>()
        {
            var e = (E)bodyParser(typeof(E));
            return new RestRequest<E>(Capability, Address, Parameters, e, bodyParser, Metadata);
        }
        /// <summary>
        /// Virtual member containing the body object, if it was parsed.
        /// </summary>
        public virtual object UntypedBody => null;

        /// <summary>
        /// Gets the cancellation token for this Rest request.
        /// </summary>
        public CancellationToken CancellationToken => cancellationTokenSource.Token;
        /// <summary>
        /// True if cancellation has been requested for this Rest request.
        /// </summary>
        public bool IsCancellationRequested => cancellationTokenSource.IsCancellationRequested;
        /// <summary>
        /// Cancels the Rest request.
        /// </summary>
        public void Cancel()
        {
            cancellationTokenSource.Cancel();
        }
    }
    /// <summary>
    /// This class represents a Rest request
    /// </summary>
    /// <typeparam name="E">The body type of the request.</typeparam>
    public class RestRequest<E> : RestRequest
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="capability">The requested capability.</param>
        /// <param name="address">An resource identity value is used to 'address' the capability on some resource.</param>
        /// <param name="parameters">Parameters for the request.</param>
        /// <param name="bodyParser">A function that is capable of parsing the body, given the body type is known.</param>
        /// <param name="metadata">Metadata for the Rest request.</param>
        public RestRequest(string capability, IIdentity address, RestParameterCollection parameters, E body, Func<Type, object> bodyParser, TypeKeyedDictionary metadata)
            : base(capability, address, parameters, bodyParser, metadata)
        {
            Body = body;
        }
        /// <summary>
        /// Gets the typed body of the request.
        /// </summary>
        public E Body { get; }
        /// <summary>
        /// Gets the untyped body of the request.
        /// </summary>
        public override object UntypedBody => Body;

        /// <summary>
        /// Parses the raw body into an object of type F.
        /// </summary>
        /// <typeparam name="F">The desired body type.</typeparam>
        /// <returns>A typed RestRequest</returns>
        public override RestRequest<F> ParseBody<F>()
            => typeof(F) == typeof(E) ? this as RestRequest<F> : base.ParseBody<F>();
    }
}