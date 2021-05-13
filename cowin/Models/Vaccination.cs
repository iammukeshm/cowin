using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cowin.Models
{
    public class Vaccination
    {
        public int total { get; set; }
        public int today { get; set; }
        public int tot_dose_1 { get; set; }
        public int tot_dose_2 { get; set; }
    }
}
