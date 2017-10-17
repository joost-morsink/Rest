using Biz.Morsink.Identity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public class RestCollection<T>
    {
        public RestCollection(IIdentity id, IEnumerable<T> items, int count, int? limit = null, int skip = 0)
        {
            Id = id;
            Items = items;
            Count = count;
            Limit = limit;
            Skip = skip;
        }
        public IIdentity Id { get; }
        public IEnumerable<T> Items { get; }
        public int Count { get; }
        public int? Limit { get; }
        public int Skip { get; }

    }
}
