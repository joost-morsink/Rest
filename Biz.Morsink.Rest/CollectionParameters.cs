using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public class CollectionParameters
    {
        public CollectionParameters(int? limit = null, int skip = 0)
        {
            Limit = limit;
            Skip = skip;
        }
        public int? Limit { get; }
        public int Skip { get; }
    }
 
}
