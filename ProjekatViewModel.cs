using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GimnazijaKM.Models
{
    public class ProjekatViewModel
    {
        public int Id { get; set; }
        public string Naslov { get; set; }
        public string Opis { get; set; }
        public string SlikaGlavna { get; set; }
        public List<string> OstaleSlike { get; set; } = new List<string>();
    }
}