using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Web;

namespace butterfly.com.rest
{
    public class Serializer
    {
        public static void Serialize(object obj, Stream outputStream)
        {
            DataContractJsonSerializer json = new DataContractJsonSerializer(obj.GetType());
            json.WriteObject(outputStream, obj);
        }
        public static object Deserialize(Stream inputStream, Type type)
        {
            DataContractJsonSerializer json = new DataContractJsonSerializer(type);
            return json.ReadObject(inputStream);
        }
    }
}