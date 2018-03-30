namespace Biz.Morsink.Rest.AspNetCore.OpenApi
{
    public class Parameter
    {
        public string Name { get; set; }
        public string In { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public bool AllowEmpty { get; set; }
        public OrReference<Schema> Schema { get; set; }
    }
}