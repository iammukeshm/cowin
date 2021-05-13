using cowin.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cowin.Responses
{
    public class StateEndpointResponse
    {
        public List<State> states { get; set; }
    }
}
