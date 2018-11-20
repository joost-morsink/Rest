namespace Biz.Morsink.Rest
{
    public class Embedding
    {
        public static Embedding Create(string reltype, object obj)
            => new Embedding(reltype, obj);
        public Embedding(string reltype, object @object)
        {
            Reltype = reltype;
            Object = @object;
        }
        public string Reltype { get;  }
        public object Object { get;  }
    }
}