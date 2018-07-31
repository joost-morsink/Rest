using Biz.Morsink.Rest.AspNetCore;

namespace Biz.Morsink.Rest.HttpConverter.Json.Test
{
    public class TestIdentityProvider : RestIdentityProvider
    {
        public TestIdentityProvider()
        {
            BuildEntry(typeof(SerializationTest.Person)).WithPath("/person/*").Add();
            BuildEntry(typeof(SerializationTest.Organization)).WithPath("/org/*").Add();
            BuildEntry(typeof(SerializationTest.Country)).WithPath("/country/*").Add();
        }

    }
}
