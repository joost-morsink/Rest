using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    public class Document
    {
        public string openApi { get; set; }
        public Info Info { get; set; }
        public List<Server> Servers { get; set; } = new List<Server>();
        public Dictionary<string, Path> Paths { get; set; } = new Dictionary<string, Path>();
        public Dictionary<string, Component> Components { get; set; } = new Dictionary<string, Component>();
        

    }
}
