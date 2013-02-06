using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO;

namespace MsbuildLauncher
{
    public static class JsonUtil
    {
        public static T Parse<T>(string text)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            var bytes = Encoding.UTF8.GetBytes(text);
            using (var st = new MemoryStream(bytes)) {
                var obj = serializer.ReadObject(st);
                return (T) obj;
            }
        }
    }
}
