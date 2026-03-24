using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;



namespace GimnazijaKM.Models
{
    public class FajlFormaViewModel
    {
        public Fajlovi NoviFajl { get; set; }
        public List<Fajlovi> SviFajlovi { get; set; }

        public IPagedList<Fajlovi> PagedFajlovi { get; set; }

    }
}