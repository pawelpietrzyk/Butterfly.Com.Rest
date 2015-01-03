using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace butterfly.com.rest.client
{
    [DataContract]
    public class RestHandler
    {
        [DataMember]
        public RestHandlerEnum Handler { get; set; }

        [DataMember]
        public string Name { get; set; }        
    }

    public enum RestHandlerEnum
    {
        Salary        
    }
}
