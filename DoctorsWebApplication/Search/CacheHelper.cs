using Newtonsoft.Json;
using System.Text;
using System.Text.Json;

namespace DoctorsWebApplication.Search
{
    public class CacheHelper
    {
        public CacheHelper()
        {
            
        }

        public byte[] SerializeObject(object value) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
        public T? DeserializeObject<T>(byte[] value) => JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(value));

    }
}
