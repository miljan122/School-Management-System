using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;


namespace GimnazijaKM.Models
{
    public class RasporedViewModel
    {

        public int ID { get; set; }
        [Required(ErrorMessage = "Унесите наслов.")]
        public string Naslov { get; set; }

        [Required(ErrorMessage = "Изаберите прву слику.")]
        public HttpPostedFileBase Slika1 { get; set; }

        [Required(ErrorMessage = "Изаберите другу слику.")]
        public HttpPostedFileBase Slika2 { get; set; }
    }
}