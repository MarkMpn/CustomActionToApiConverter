using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkMpn.CustomActionToApiConverter
{
    class CustomAction
    {
        public string Name { get; set; }

        public string MessageName { get; set; }

        public string Description { get; set; }

        public string PrimaryEntity { get; set; }

        public List<RequestParameter> RequestParameters { get; set; }

        public List<ResponseParameter> ResponseParameters { get; set; }
    }

    class Parameter
    {
        public string Name { get; set; }

        public Type Type { get; set; }
    }

    class RequestParameter : Parameter
    {
        public bool Required { get; set; }
    }

    class ResponseParameter : Parameter
    {
    }
}
