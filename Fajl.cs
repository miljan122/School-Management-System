using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GimnazijaKM.Models
{
    public class Fajl
    {
        public int Id { get; set; }
      
        public string Naziv { get; set; }
        public string Tip { get; set; }
      
        public string Putanja { get; set; }
        public DateTime Datum { get; set; }
        public string Naslov { get; set; }
    }
}