using cowin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cowin.Responses
{
    public class CalendarByDistrictEndpointResponse
    {
        public List<Center> centers { get; set; }
    }
}
