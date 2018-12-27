namespace Biz.Morsink.Rest
{
    /// <summary>
    /// This class encapsulates an embedded object.
    /// </summary>
    public class Embedding
    {
        /// <summary>
        /// Static creation method.
        /// Simply calls the constructor.
        /// </summary>
        /// <param name="reltype">The relation type for the embedded object.</param>
        /// <param name="obj">The embedded object.</param>
        /// <returns></returns>
        public static Embedding Create(string reltype, object obj)
            => new Embedding(reltype, obj);
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="reltype">The relation type for the embedded object.</param>
        /// <param name="object">The embedded object.</param>
        public Embedding(string reltype, object @object)
        {
            Reltype = reltype;
            Object = @object;
        }
        /// <summary>
        /// Contains the relation type for the embedded object.
        /// </summary>
        public string Reltype { get;  }
        /// <summary>
        /// Contains the embedded object.
        /// </summary>
        public object Object { get;  }
    }
}