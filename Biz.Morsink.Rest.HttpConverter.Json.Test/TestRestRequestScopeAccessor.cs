using Biz.Morsink.Rest.Utils;

namespace Biz.Morsink.Rest.HttpConverter.Json.Test
{
    public class TestRestRequestScopeAccessor : IRestRequestScopeAccessor
    {
        public TestRestRequestScopeAccessor()
        {
            Scope = new RestRequestScope();
        }
        public IRestRequestScope Scope { get; }
        private class RestRequestScope : IRestRequestScope
        {
            private TypeKeyedDictionary dict;

            public RestRequestScope()
            {
                dict = TypeKeyedDictionary.Empty;
            }
            public void SetScopeItem<T>(T item)
            {
                dict = dict.Set(item);
            }

            public bool TryGetScopeItem<T>(out T result)
                => dict.TryGet(out result);
            public bool TryRemoveScopeItem<T>(out T result)
            {
                if (TryGetScopeItem(out result))
                {
                    dict = dict.Set(default(T));
                    return true;
                }
                else
                    return false;
            }
        }
    }
}
