using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GimnazijaKM.Models
{
    public class IndexViewModel
    {
        public List<Vesti> Vesti { get; set; }
        public List<Dogadjaji> Events { get; set; }

        public Slider Slider { get; set; }
    }
}