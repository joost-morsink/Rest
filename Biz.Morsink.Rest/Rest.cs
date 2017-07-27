using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public static class Rest
    {
        public static RestValue<T> Value<T>(T item)
            where T : class
            => new RestValue<T>(item);
        public static RestValue<T>.Builder ValueBuilder<T>(T item)
            where T : class
            => RestValue<T>.Build().WithValue(item);
    }
}
