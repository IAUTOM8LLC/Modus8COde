using Newtonsoft.Json;
using Xunit;

namespace IAutoM8.Tests.Common
{
    public class AssertEx
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public static void EqualSerialization(object expectedResult, object actual)
        {
            Assert.Equal(
                JsonConvert.SerializeObject(expectedResult, SerializerSettings),
                JsonConvert.SerializeObject(actual, SerializerSettings));
        }
    }
}
