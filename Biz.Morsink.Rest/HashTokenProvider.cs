using Biz.Morsink.DataConvert;
using Biz.Morsink.Rest.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using static Biz.Morsink.DataConvert.DataConverterExt;
using Ex = System.Linq.Expressions.Expression;
namespace Biz.Morsink.Rest
{
    /// <summary>
    /// A token provider based on creating a hash based on property values.
    /// </summary>
    /// <typeparam name="T">The type to provide a hash for.</typeparam>
    public class HashTokenProvider<T> : ITokenProvider<T>
    {
        #region Code generation
        private static Func<T, IDataConverter, string> getToken { get; } = MakeGetToken();

        private static Func<T, IDataConverter, string> MakeGetToken()
        {
            var t = Ex.Parameter(typeof(T), "t");
            var converter = Ex.Parameter(typeof(IDataConverter), "converter");
            var props = typeof(T).GetTypeInfo().Iterate(x => x.BaseType?.GetTypeInfo()).TakeWhile(x => x != null)
                .SelectMany(x => x.DeclaredProperties)
                .Where(p => p.CanRead && !p.GetMethod.IsStatic)
                .GroupBy(x => x.Name)
                .Select(x => x.First())
                .ToArray();
            var sb = Ex.Parameter(typeof(StringBuilder), "sb");

            var block = Ex.Block(new[] { sb },
                Ex.Assign(sb, Ex.New(typeof(StringBuilder))),
                Ex.Call(sb, nameof(StringBuilder.AppendLine), Type.EmptyTypes, Ex.Constant(typeof(T).FullName)),
                Ex.Block(props.Select(p =>
                    Ex.Call(sb, nameof(StringBuilder.AppendLine), Type.EmptyTypes,
                        Ex.Call(
                            Ex.Call(typeof(DataConverterExt), nameof(DataConverterExt.Convert), new[] { p.PropertyType },
                                converter,
                                Ex.Property(t, p)),
                            nameof(Convertible<object>.To), new[] { typeof(string) }, Ex.Default(typeof(string)))))),
                Ex.Call(typeof(HashTokenProvider<T>), nameof(MakeToken), Type.EmptyTypes,
                    Ex.Call(sb, nameof(StringBuilder.ToString), Type.EmptyTypes))
                       );
            var lambda = Ex.Lambda<Func<T, IDataConverter, string>>(block, t, converter);
            return lambda.Compile();
        }
        private static string MakeToken(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            using (var sha = new SHA1Managed())
                return Convert.ToBase64String(sha.ComputeHash(bytes));
        }
        #endregion

        private readonly IDataConverter converter;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="converter"></param>
        public HashTokenProvider(IDataConverter converter = null)
        {
            this.converter = converter ?? DataConverter.Default;
        }

        /// <summary>
        /// Constructs a hash token for the specified item.
        /// </summary>
        /// <param name="item">The item to create a hash token for.</param>
        /// <returns>A strring representation of the token.</returns>
        public string GetTokenFor(T item)
            => getToken(item, converter);

        string ITokenProvider.GetTokenFor(object item)
            => GetTokenFor((T)item);
    }
}
