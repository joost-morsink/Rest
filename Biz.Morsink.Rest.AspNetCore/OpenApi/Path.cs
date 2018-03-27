namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    public class Path
    {
        public string Ref {get;set;}
        public string Summary { get; set; }
        public string Description { get; set; }
        public Operation Get { get; set; }
        public Operation Put { get; set; }
        public Operation Post { get; set; }
        public Operation Patch { get; set; }
        public Operation Delete { get; set; }
        public Operation Options { get; set; }
    }
}