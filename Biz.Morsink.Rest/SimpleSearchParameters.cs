using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest
{
    public class SimpleSearchParameters
    {
        public SimpleSearchParameters(string q)
        {
            Q = q;
        }

        public string Q { get; }
    }
}
