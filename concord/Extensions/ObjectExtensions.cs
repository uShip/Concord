using concord.Output.JsonConverters;
using Newtonsoft.Json;

namespace concord.Extensions
{
    public static class ObjectExtensions
    {
        public static string GetJson_TimespansAsMs<T>(this T value)
        {
            return JsonConvert.SerializeObject(value, new TimeSpanJsonConverter());
        }
    }
}