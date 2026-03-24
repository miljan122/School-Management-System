using GimnazijaKM.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace GimnazijaKM.Controllers
{
    public class HomeController : Controller
    {

        GimnazijaDBEntities db = new GimnazijaDBEntities();
        public ActionResult Index()
        {
            var vesti = db.Vesti.ToList();
            var events = db.Dogadjaji.ToList();

            // Uzmi prvi (ili poslednji) slider ako ih ima više
            var slider = db.Slider.OrderByDescending(s => s.SliderID).FirstOrDefault();

            var model = new IndexViewModel
            {
                Vesti = vesti,
                Events = events,
                Slider = slider
            };

            return View(model);
        }


        public ActionResult KreirajDogadjaj()
        {
            var model = new EventViewModel
            {
                NoviDogadjaj = new Dogadjaji(),
                SviDogadjaji = db.Dogadjaji.ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult KreirajDogadjaj(EventViewModel model, HttpPostedFileBase SlikaUpload)
        {
            if (ModelState.IsValid)
            {
                if (SlikaUpload != null && SlikaUpload.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(SlikaUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/slike"), fileName);
                    SlikaUpload.SaveAs(path);

                    model.NoviDogadjaj.Slika = fileName;
                }

                db.Dogadjaji.Add(model.NoviDogadjaj);
                db.SaveChanges();
                return RedirectToAction("KreirajDogadjaj");
            }

            model.SviDogadjaji = db.Dogadjaji.ToList();
            return View(model);
        }



        public ActionResult DetaljiDogadjaja(int id)
        {
            var dogadjaj = db.Dogadjaji.Find(id);
            if (dogadjaj == null)
            {
                return HttpNotFound();
            }

            return View(dogadjaj);
        }

        // GET: Edit
        public ActionResult AzurirajDogadjaj(int id)
        {
            var eventItem = db.Dogadjaji.Find(id);
            if (eventItem == null)
            {
                return HttpNotFound();
            }
            return View(eventItem);  // Vraćamo događaj u formi za editovanje
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AzurirajDogadjaj(Dogadjaji model, HttpPostedFileBase SlikaUpload)
        {
            if (ModelState.IsValid)
            {
                var dogadjaj = db.Dogadjaji.Find(model.Id);
                if (dogadjaj == null) return HttpNotFound();

                dogadjaj.Ime = model.Ime;
                dogadjaj.Datum = model.Datum;
                dogadjaj.Opis = model.Opis;

                if (SlikaUpload != null && SlikaUpload.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(SlikaUpload.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/slike"), fileName);
                    SlikaUpload.SaveAs(path);

                    dogadjaj.Slika = fileName;
                }

                db.SaveChanges();
                return RedirectToAction("KreirajDogadjaj");
            }

            return View(model);
        }


        public ActionResult IzbrisiDogadjaj(int id)
        {
            var eventItem = db.Dogadjaji.Find(id);
            if (eventItem == null)
            {
                return HttpNotFound();
            }
            return View(eventItem);  // Vraćamo podatke o događaju koji treba da se obriše
        }

        // POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var dogadjaj = db.Dogadjaji.Find(id);
            if (dogadjaj == null) return HttpNotFound();

            db.Dogadjaji.Remove(dogadjaj);
            db.SaveChanges();

            return RedirectToAction("KreirajDogadjaj");
        }


        public ActionResult DodajFajl()
        {
            var model = new FajlFormaViewModel
            {
                NoviFajl = new Fajlovi(),
                SviFajlovi = db.Fajlovi.ToList() // Učitavaš listu svih fajlova
            };

            return View(model); // Vraćaš model sa svim fajlovima
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DodajFajl(FajlFormaViewModel model, HttpPostedFileBase FajlUpload)
        {
            if (ModelState.IsValid)
            {
                if (FajlUpload != null && FajlUpload.ContentLength > 0)
                {
                    string folderPath = Server.MapPath("~/Content/fajlovi");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath); // Kreiraj direktorijum ako ne postoji
                    }

                    var fileName = Path.GetFileName(FajlUpload.FileName);
                    var fileExtension = Path.GetExtension(fileName);
                    var path = Path.Combine(folderPath, fileName);

                    try
                    {
                        // Sačuvaj fajl na serveru
                        FajlUpload.SaveAs(path);

                        // Kreiraj novi objekat Fajlovi sa podacima iz forme
                        var noviFajl = new Fajlovi
                        {
                            Naziv = model.NoviFajl.Naziv,
                            Tip = fileExtension,
                            Putanja = "~/Content/fajlovi/" + fileName, // Relativna putanja koja ide u bazu
                            Datum = DateTime.Now
                        };

                        // Dodaj fajl u bazu
                        db.Fajlovi.Add(noviFajl);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        // Logovanje grešaka
                        ModelState.AddModelError("", "Došlo je do greške prilikom snimanja fajla: " + ex.Message);
                    }
                }
            }

            // Po uspešnom dodavanju fajla, učitaj ponovo sve fajlove
            model.SviFajlovi = db.Fajlovi.ToList();
            return View(model); // Vraća model sa svim fajlovima
        }


        // GET: AzurirajFajl
        [HttpGet]
        public ActionResult AzurirajFajl(int id)
        {
            var fajl = db.Fajlovi.Find(id);
            if (fajl == null)
            {
                return HttpNotFound();
            }

            var model = new FajlFormaViewModel
            {
                NoviFajl = fajl
            };

            return View(model);
        }


        // POST: AzurirajFajl
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AzurirajFajl(FajlFormaViewModel model, HttpPostedFileBase FajlUpload)
        {
            var fajlIzBaze = db.Fajlovi.Find(model.NoviFajl.Id);
            if (fajlIzBaze == null)
            {
                return HttpNotFound();
            }

            // Ažuriranje tekstualnih polja
            fajlIzBaze.Naziv = model.NoviFajl.Naziv;
            fajlIzBaze.Tip = model.NoviFajl.Tip;

            // Ako je fajl poslat, snimi ga
            if (FajlUpload != null && FajlUpload.ContentLength > 0)
            {
                string fajlIme = Path.GetFileName(FajlUpload.FileName);
                string putanja = Path.Combine(Server.MapPath("~/Content/fajlovi/"), fajlIme);
                FajlUpload.SaveAs(putanja);

                // Ažuriraj info o fajlu u bazi
                fajlIzBaze.Putanja = "~/Content/fajlovi/" + fajlIme;
            }

            db.SaveChanges();

            return RedirectToAction("DodajFajl"); // Ili gde želiš
        }


        // GET: IzbrisiFajl
        public ActionResult IzbrisiFajl(int id)
        {
            var fajl = db.Fajlovi.Find(id);
            if (fajl == null)
            {
                return HttpNotFound();
            }

            return View(fajl);
        }

        // POST: IzbrisiFajl
        [HttpPost, ActionName("IzbrisiFajl")]
        [ValidateAntiForgeryToken]
        public ActionResult PotvrdiBrisanjeFajla(int id)
        {
            var fajl = db.Fajlovi.Find(id);
            if (fajl != null)
            {
                // Obriši fajl sa diska
                var fajlPath = Server.MapPath(fajl.Putanja);
                if (System.IO.File.Exists(fajlPath))
                {
                    System.IO.File.Delete(fajlPath);
                }

                // Obrisi fajl iz baze podataka
                db.Fajlovi.Remove(fajl);
                db.SaveChanges();
            }

            return RedirectToAction("DodajFajl");
        }



        public ActionResult Dokumenta(int? page)
        {
            int pageSize = 15;
            int pageNumber = page ?? 1;

            var sviFajlovi = db.Fajlovi.OrderByDescending(f => f.Datum);
            var pagedFajlovi = sviFajlovi.ToPagedList(pageNumber, pageSize);

            var model = new FajlFormaViewModel
            {
                NoviFajl = new Fajlovi(),
                SviFajlovi = sviFajlovi.ToList(), // ako ti treba negde drugde
                PagedFajlovi = pagedFajlovi
            };

            return View(model);
        }




        public ActionResult Maturanti(int page = 1, int pageSize = 6)
        {
            var totalMaturanti = db.Maturanti.Count();
            var sviMaturanti = db.Maturanti
                                 .OrderBy(m => m.MaturantID) // zameni po potrebi
                                 .Skip((page - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalMaturanti / pageSize);

            return View(sviMaturanti);
        }


        [HttpGet]
        public ActionResult Login()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }


        [HttpPost]
        public ActionResult Login(tbl_Admin ad)
        {
            tbl_Admin ada = db.tbl_Admin.Where(x => x.KorisnicnoIme == ad.KorisnicnoIme && x.Lozinka == ad.Lozinka).SingleOrDefault();

            if (ada != null)
            {
                Session["ID"] = ad.ID;
                return RedirectToAction("Administracija");
            }
            else
            {
                ViewBag.msg = "Pogresili ste Korisnicko ime ili lozinku !";
                return View();
            }



        }




        [HttpGet]
        public ActionResult KreirajVest()
        {
            ViewBag.VestiLista = db.Vesti.OrderByDescending(x => x.KreiranoU).ToList();
            return View(new Vesti());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult KreirajVest(Vesti model, HttpPostedFileBase Slika1, HttpPostedFileBase Slika2, HttpPostedFileBase Slika3)
        {
            if (ModelState.IsValid)
            {
                model.KreiranoU = DateTime.Now;

                if (Slika1 != null && Slika1.ContentLength > 0)
                {
                    var slika1Path = Path.Combine(Server.MapPath("~/Content/slike"), Path.GetFileName(Slika1.FileName));
                    Slika1.SaveAs(slika1Path);
                    model.Slika1 = "/Content/slike/" + Path.GetFileName(Slika1.FileName);
                }

                if (Slika2 != null && Slika2.ContentLength > 0)
                {
                    var slika2Path = Path.Combine(Server.MapPath("~/Content/slike"), Path.GetFileName(Slika2.FileName));
                    Slika2.SaveAs(slika2Path);
                    model.Slika2 = "/Content/slike/" + Path.GetFileName(Slika2.FileName);
                }

                if (Slika3 != null && Slika3.ContentLength > 0)
                {
                    var slika3Path = Path.Combine(Server.MapPath("~/Content/slike"), Path.GetFileName(Slika3.FileName));
                    Slika3.SaveAs(slika3Path);
                    model.Slika3 = "/Content/slike/" + Path.GetFileName(Slika3.FileName);
                }

                try
                {
                    db.Vesti.Add(model);
                    db.SaveChanges();
                    return RedirectToAction("KreirajVest");
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var validationError in ex.EntityValidationErrors)
                    {
                        foreach (var error in validationError.ValidationErrors)
                        {
                            System.Diagnostics.Debug.WriteLine($"Property: {error.PropertyName} Error: {error.ErrorMessage}");
                        }
                    }
                    throw;
                }
            }
            else
            {
                // Ako ModelState nije validan, ispisi sve greske u Output window
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine("ModelState Error: " + error.ErrorMessage);
                    }
                }
            }

            ViewBag.VestiLista = db.Vesti.OrderByDescending(x => x.KreiranoU).ToList();
            return View(model);
        }





        public ActionResult Administracija()
        {
            return View();
        }



        public ActionResult Ekskurzija()
        {
            var ekskurzije = db.Ekskurzije.ToList();
            return View(ekskurzije);
        }

        public ActionResult PrikazEkskurzije(int id)
        {
            var eks = db.Ekskurzije.Find(id);
            if (eks == null)
                return HttpNotFound();

            return View("PrikazEkskurzije", eks);
        }


        public ActionResult MostMatematike()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Timovi()
        {
            var timovi = db.Tim.Include("Clans").ToList();
            return View(timovi);
        }
        // GET: MenjajTim/UbaciTim
        public ActionResult UbaciTim()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UbaciTim([Bind(Include = "Naziv,BojaKartice")] Tim tim)
        {
            if (ModelState.IsValid)
            {
                db.Tim.Add(tim);
                db.SaveChanges();
                return RedirectToAction("UbaciClana", new { timId = tim.TimId });
            }
            return View(tim);
        }

        public ActionResult UbaciClana(int timId)
        {
            var tim = db.Tim.Find(timId);
            if (tim == null) return HttpNotFound();

            ViewBag.TimNaziv = tim.Naziv;

            return View(new Clan { TimId = timId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UbaciClana(Clan clan)
        {
            if (ModelState.IsValid)
            {
                db.Clan.Add(clan);
                db.SaveChanges();
                return RedirectToAction("UbaciClana", new { timId = clan.TimId });
            }

            var tim = db.Tim.Find(clan.TimId);
            ViewBag.TimNaziv = tim?.Naziv;

            return View(clan);
        }

        public ActionResult ListaTimovaAdmin()
        {
            var timovi = db.Tim.Include("Clans").ToList();
            return View(timovi);
        }
        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult ObrisiClana(int id)
        {
            var clan = db.Clan.Find(id);
            if (clan == null) return HttpNotFound();

            db.Clan.Remove(clan);
            db.SaveChanges();

            return RedirectToAction("ListaTimovaAdmin");
        }


        [HttpPost]
        // [ValidateAntiForgeryToken]
        public ActionResult ObrisiTim(int id)
        {
            var tim = db.Tim.Include("Clans").FirstOrDefault(t => t.TimId == id);
            if (tim == null) return HttpNotFound();

            foreach (var clan in tim.Clans.ToList())
            {
                db.Clan.Remove(clan);
            }

            db.Tim.Remove(tim);
            db.SaveChanges();

            return RedirectToAction("ListaTimovaAdmin");
        }





        [HttpGet]
        public ActionResult Projekti()
        {
            var projekti = db.Projekti.OrderByDescending(p => p.CreatedAt).ToList();
            return View(projekti);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Projekti(Projekti projekat, HttpPostedFileBase Slika1, HttpPostedFileBase Slika2, HttpPostedFileBase Slika3, HttpPostedFileBase Slika4)
        {
            if (ModelState.IsValid)
            {
                projekat.Slika1Path = SaveImage(Slika1);
                projekat.Slika2Path = SaveImage(Slika2);
                projekat.Slika3Path = SaveImage(Slika3);
                projekat.Slika4Path = SaveImage(Slika4);

                projekat.CreatedAt = DateTime.Now;

                db.Projekti.Add(projekat);
                db.SaveChanges();

                var projekti = db.Projekti.OrderByDescending(p => p.CreatedAt).ToList();
                return View(projekti);
            }

            var sviProjekti = db.Projekti.OrderByDescending(p => p.CreatedAt).ToList();
            return View(sviProjekti);
        }

        private string SaveImage(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var folder = Server.MapPath("~/Content/slike/");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var path = Path.Combine(folder, fileName);
                file.SaveAs(path);

                return "/Content/slike/" + fileName;
            }
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }




        // GET: Projekti/Edit/5
        public ActionResult AzurirajProjekat(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var projekat = db.Projekti.Find(id);
            if (projekat == null)
                return HttpNotFound();

            return View(projekat);
        }

        // POST: Projekti/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AzurirajProjekat(Projekti projekat, HttpPostedFileBase Slika1, HttpPostedFileBase Slika2, HttpPostedFileBase Slika3, HttpPostedFileBase Slika4)
        {
            if (ModelState.IsValid)
            {
                var postojeći = db.Projekti.Find(projekat.Id);
                if (postojeći == null)
                    return HttpNotFound();

                // Ажурирање текстова
                postojeći.Tekst1 = projekat.Tekst1;
                postojeći.Tekst2 = projekat.Tekst2;
                postojeći.Tekst3 = projekat.Tekst3;
                postojeći.Tekst4 = projekat.Tekst4;
                postojeći.Tekst5 = projekat.Tekst5;

                // Ако је учитана нова слика, замени постојећу
                if (Slika1 != null && Slika1.ContentLength > 0)
                    postojeći.Slika1Path = SaveImage(Slika1);
                if (Slika2 != null && Slika2.ContentLength > 0)
                    postojeći.Slika2Path = SaveImage(Slika2);
                if (Slika3 != null && Slika3.ContentLength > 0)
                    postojeći.Slika3Path = SaveImage(Slika3);
                if (Slika4 != null && Slika4.ContentLength > 0)
                    postojeći.Slika4Path = SaveImage(Slika4);

                db.SaveChanges();
                return RedirectToAction("Projekti");
            }

            return View(projekat);
        }

        // GET: Projekti/Delete/5
        public ActionResult IzbrisiProjekat(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var projekat = db.Projekti.Find(id);
            if (projekat == null)
                return HttpNotFound();

            return View(projekat);
        }

        // POST: Projekti/Delete/5
        [HttpPost, ActionName("IzbrisiProjekat")]
        [ValidateAntiForgeryToken]
        public ActionResult IzbrisiPotvrda(int id)
        {
            var projekat = db.Projekti.Find(id);
            if (projekat != null)
            {
                db.Projekti.Remove(projekat);
                db.SaveChanges();
            }
            return RedirectToAction("Projekti");
        }




        public ActionResult DodajJavneNabavke()
        {
            var sveNabavke = db.JavneNabavke.OrderByDescending(x => x.DatumDodavanja).ToList();
            ViewBag.Nabavke = sveNabavke;
            return View();
        }

        // POST: DodajJavneNabavke
        [HttpPost]

        public ActionResult DodajJavneNabavke(string opis, HttpPostedFileBase fajl)
        {
            if (fajl != null && fajl.ContentLength > 0 && Path.GetExtension(fajl.FileName).ToLower() == ".pdf")
            {
                var fileName = Path.GetFileName(fajl.FileName);
                var uploadFolder = Server.MapPath("~/Content/slike");

                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                var fullPath = Path.Combine(uploadFolder, fileName);
                fajl.SaveAs(fullPath);

                var relativePath = "/Content/slike/" + fileName; // važno: kosa crta između

                var novaNabavka = new JavneNabavke
                {
                    Opis = opis,
                    FilePath = relativePath,
                    DatumDodavanja = DateTime.Now
                };

                db.JavneNabavke.Add(novaNabavka);
                db.SaveChanges();

                TempData["Poruka"] = "Uspešno sačuvano.";
                return RedirectToAction("DodajJavneNabavke");
            }

            TempData["Poruka"] = "Greška! Dodajte PDF fajl.";
            return RedirectToAction("DodajJavneNabavke");
        }

        // GET: IzmeniJavnu
        public ActionResult IzmeniJavnu(int id)
        {
            var nabavka = db.JavneNabavke.FirstOrDefault(x => x.Id == id);
            if (nabavka == null)
                return HttpNotFound();

            return View(nabavka);
        }

        // POST: IzmeniJavnu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IzmeniJavnu(int id, string Opis, HttpPostedFileBase fajl)
        {
            var nabavka = db.JavneNabavke.FirstOrDefault(x => x.Id == id);
            if (nabavka == null)
                return HttpNotFound();

            nabavka.Opis = Opis;

            if (fajl != null && fajl.ContentLength > 0 && Path.GetExtension(fajl.FileName).ToLower() == ".pdf")
            {
                var stariFajl = Server.MapPath(nabavka.FilePath);
                if (System.IO.File.Exists(stariFajl))
                    System.IO.File.Delete(stariFajl);

                var fileName = Path.GetFileName(fajl.FileName);
                var novaPutanja = Path.Combine(Server.MapPath("~/Content/slike"), fileName);
                fajl.SaveAs(novaPutanja);

                nabavka.FilePath = "/Content/slike/" + fileName; // kosa crta između foldera i fajla
            }

            db.SaveChanges();

            TempData["Poruka"] = "Javna nabavka je uspešno izmenjena.";
            return RedirectToAction("DodajJavneNabavke");
        }

        // POST: ObrisiJavnuNabavku
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ObrisiJavnuNabavku(int id)
        {
            var nabavka = db.JavneNabavke.Find(id);
            if (nabavka != null)
            {
                var putanjaFajla = Server.MapPath(nabavka.FilePath);
                if (System.IO.File.Exists(putanjaFajla))
                    System.IO.File.Delete(putanjaFajla);

                db.JavneNabavke.Remove(nabavka);
                db.SaveChanges();

                TempData["Poruka"] = "Јавна набавка је успешно обрисана.";
            }
            else
            {
                TempData["Poruka"] = "Јавна набавка није пронађена.";
            }

            return RedirectToAction("DodajJavneNabavke");
        }

        // GET: PreuzmiPdf
        public ActionResult PreuzmiPdf(int id)
        {
            var nabavka = db.JavneNabavke.Find(id);
            if (nabavka == null)
                return HttpNotFound();

            string filePath = Server.MapPath(nabavka.FilePath);

            if (!System.IO.File.Exists(filePath))
                return HttpNotFound($"Fajl ne postoji na serveru: {filePath}");

            return File(filePath, "application/pdf", Path.GetFileName(filePath));
        }

       

        public ActionResult JavnaNabavka()
        {
            var nabavke = db.JavneNabavke.OrderByDescending(x => x.DatumDodavanja).ToList();
            return View(nabavke);
        }



        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = TempData["Message"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(ContactMessage model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new GimnazijaDBEntities())
                {
                    model.Poslato = DateTime.Now;
                    db.ContactMessages.Add(model);
                    db.SaveChanges();
                }

                TempData["Message"] = "Порука је успешно послата!";
                return Redirect(Url.Action("Contact") + "#contactForm");
            }

            ViewBag.Message = "Дошло је до грешке при слању поруке.";
            return View(model);
        }

        public ActionResult VidiPoruku()
        {
            using (var db = new GimnazijaDBEntities())
            {
                var poruke = db.ContactMessages.ToList();
                return View(poruke);
            }
        }


        [HttpPost]
        public ActionResult IzbrisiPoruku(int id)
        {
            using (var db = new GimnazijaDBEntities())
            {
                var poruka = db.ContactMessages.Find(id);
                if (poruka != null)
                {
                    db.ContactMessages.Remove(poruka);
                    db.SaveChanges();
                    TempData["Message"] = "Порука је успешно обрисана.";
                }
                else
                {
                    TempData["Message"] = "Порука није пронађена.";
                }
            }
            return RedirectToAction("VidiPoruku");
        }

        public ActionResult DodajLetopis()
        {
            // Praviš listu svih zapisa iz tabele Letopis (DbSet Letopis)
            var letopisi = db.Letopis.OrderByDescending(l => l.DatumUnosa).ToList();
            return View(letopisi);
        }

        [HttpPost]
        public ActionResult DodajLetopis(string naslov, string opis, HttpPostedFileBase pdfFajl)
        {
            string putanja = null;

            if (pdfFajl != null && pdfFajl.ContentType == "application/pdf")
            {
                var imeFajla = Path.GetFileName(pdfFajl.FileName);
                var fizickaPutanja = Path.Combine(Server.MapPath("~/Content/slike/"), imeFajla);
                pdfFajl.SaveAs(fizickaPutanja);
                putanja = "/Content/slike/" + imeFajla;
            }

            var novi = new Letopi
            {
                Naslov = naslov,
                Opis = opis,
                PdfPutanja = putanja,
                DatumUnosa = DateTime.Now
            };

            db.Letopis.Add(novi);  // Ovde koristiš DbSet Letopis, ne LetopiSet ili LetopisSet
            db.SaveChanges();

            // Vrati osvežen prikaz sa svim zapisima
            var letopisi = db.Letopis.OrderByDescending(l => l.DatumUnosa).ToList();
            return View(letopisi);
        }


        // GET: Prikaz forme za izmenu
        public ActionResult IzmeniLetopis(int id)
        {
            var letopis = db.Letopis.Find(id);
            if (letopis == null)
                return HttpNotFound();

            return View(letopis);
        }

        // POST: Sačuvaj izmenu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IzmeniLetopis(Letopi model, HttpPostedFileBase pdfFajl)
        {
            if (ModelState.IsValid)
            {
                var letopisUBazi = db.Letopis.Find(model.Id);
                if (letopisUBazi == null)
                    return HttpNotFound();

                letopisUBazi.Naslov = model.Naslov;
                letopisUBazi.Opis = model.Opis;

                // Ako je uploadovan novi PDF, snimi ga i zameni putanju
                if (pdfFajl != null && pdfFajl.ContentType == "application/pdf")
                {
                    var imeFajla = Path.GetFileName(pdfFajl.FileName);
                    var fizickaPutanja = Path.Combine(Server.MapPath("~/Content/slike/"), imeFajla);
                    pdfFajl.SaveAs(fizickaPutanja);
                    letopisUBazi.PdfPutanja = "/Content/slike/" + imeFajla;
                }

                db.SaveChanges();

                return RedirectToAction("DodajLetopis");
            }
            return View(model);
        }

        // GET: Potvrda za brisanje
        public ActionResult ObrisiLetopis(int id)
        {
            var letopis = db.Letopis.Find(id);
            if (letopis == null)
                return HttpNotFound();

            return View(letopis);
        }

        // POST: Brisanje
        [HttpPost, ActionName("ObrisiLetopis")]
        [ValidateAntiForgeryToken]
        public ActionResult ObrisiLetopisPotvrda(int id)
        {
            var letopis = db.Letopis.Find(id);
            if (letopis == null)
                return HttpNotFound();

            db.Letopis.Remove(letopis);
            db.SaveChanges();

            return RedirectToAction("DodajLetopis");
        }

        public PartialViewResult LetopisiMeni()
        {
            var letopisi = db.Letopis
                .OrderByDescending(l => l.DatumUnosa)
                .ToList();

            return PartialView("_LetopisiMeni", letopisi);
        }





        public ActionResult DetaljiLetopisa(int id)
        {
            var letopis = db.Letopis.FirstOrDefault(l => l.Id == id);
            if (letopis == null)
                return HttpNotFound();

            return View(letopis);
        }


















        // GET: Home/Edit/5
        public ActionResult Azuriraj(int id)
        {
            var vest = db.Vesti.Find(id);
            if (vest == null)
            {
                return HttpNotFound();
            }
            return View(vest);
        }

        // POST: Home/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Azuriraj(Vesti model, HttpPostedFileBase Slika1, HttpPostedFileBase Slika2, HttpPostedFileBase Slika3)
        {
            if (ModelState.IsValid)
            {
                var vest = db.Vesti.Find(model.Id);
                if (vest != null)
                {
                    vest.Naslov = model.Naslov;
                    vest.KratakOpis = model.KratakOpis;
                    vest.PunOpis = model.PunOpis;
                    vest.Autor = model.Autor;
                    vest.Kategorija = model.Kategorija;
                    vest.Tag = model.Tag;
                    vest.Objavljeno = model.Objavljeno;
                    vest.KreiranoU = model.KreiranoU;

                    // Ažuriranje slika
                    if (Slika1 != null && Slika1.ContentLength > 0)
                    {
                        // Sačuvaj sliku 1
                        var slika1Path = Path.Combine(Server.MapPath("~/Content/slike"), Path.GetFileName(Slika1.FileName));
                        Slika1.SaveAs(slika1Path);
                        vest.Slika1 = "/Content/slike/" + Path.GetFileName(Slika1.FileName);
                        Debug.WriteLine("Slika 1 putanja: " + vest.Slika1);
                    }

                    if (Slika2 != null && Slika2.ContentLength > 0)
                    {
                        // Sačuvaj sliku 2
                        var slika2Path = Path.Combine(Server.MapPath("~/Content/slike"), Path.GetFileName(Slika2.FileName));
                        Slika2.SaveAs(slika2Path);
                        vest.Slika2 = "/Content/slike/" + Path.GetFileName(Slika2.FileName);
                        Debug.WriteLine("Slika 2 putanja: " + vest.Slika2);
                    }

                    if (Slika3 != null && Slika3.ContentLength > 0)
                    {
                        // Sačuvaj sliku 3
                        var slika3Path = Path.Combine(Server.MapPath("~/Content/slike"), Path.GetFileName(Slika3.FileName));
                        Slika3.SaveAs(slika3Path);
                        vest.Slika3 = "/Content/slike/" + Path.GetFileName(Slika3.FileName);
                        Debug.WriteLine("Slika 3 putanja: " + vest.Slika3);
                    }

                    // Snimanje promena u bazu
                    db.SaveChanges();
                }
                return RedirectToAction("KreirajVest");
            }
            return View(model);
        }


        // GET: Home/Delete/5
        public ActionResult Izbrisi(int id)
        {
            var vest = db.Vesti.Find(id);
            if (vest == null)
            {
                return HttpNotFound();
            }
            return View(vest);
        }

        [HttpPost, ActionName("Izbrisi")]
        [ValidateAntiForgeryToken]
        public ActionResult PotvrdiBrisanje(int id)
        {
            var vest = db.Vesti.Find(id);
            if (vest != null)
            {
                // Obrisi slike sa diska
                if (!string.IsNullOrEmpty(vest.Slika1))
                {
                    var slika1Path = Server.MapPath(vest.Slika1);
                    if (System.IO.File.Exists(slika1Path))
                        System.IO.File.Delete(slika1Path);
                }
                if (!string.IsNullOrEmpty(vest.Slika2))
                {
                    var slika2Path = Server.MapPath(vest.Slika2);
                    if (System.IO.File.Exists(slika2Path))
                        System.IO.File.Delete(slika2Path);
                }
                if (!string.IsNullOrEmpty(vest.Slika3))
                {
                    var slika3Path = Server.MapPath(vest.Slika3);
                    if (System.IO.File.Exists(slika3Path))
                        System.IO.File.Delete(slika3Path);
                }

                db.Vesti.Remove(vest);
                db.SaveChanges();
            }
            return RedirectToAction("KreirajVest");
        }

        public ActionResult Detalji(int id)
        {
            var vest = db.Vesti.FirstOrDefault(v => v.Id == id);

            return View(vest); // Ne moraš postavljati KreiranoUFormatted
        }


        public ActionResult ItUcenici()
        {
            // Пример учитавања са базе - прилагоди по својој структури
            var projektiIzBaze = db.Projekti.ToList();

            // Претвори их у view modele, примера ради
            var projekti = projektiIzBaze.Select(p => new ProjekatViewModel
            {
                Id = p.Id,
                Naslov = p.Tekst1,  // Или користи поље које је за наслов
                Opis = p.Tekst2,    // Или неко друго поље за опис
                SlikaGlavna = p.Slika1Path,
                OstaleSlike = new List<string> { p.Slika2Path, p.Slika3Path, p.Slika4Path }
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList()
            }).ToList();

            ViewBag.ITSmerOpis = "IT smer је савршен избор за све који желе да се баве технологијама будућности. Кроз наш образовни програм, ученици савладавају најновије технике програмирања, дизајнирања, роботике и мрежних технологија. Ученици такође стичу важно практично искуство кроз рад са стварним пројектима и сарадњу са IT фирмама.";

            return View(projekti);
        }

        public ActionResult JezickiSmer()
        {
            var poslednjiSmer = db.JezickiSmer
                .OrderByDescending(x => x.Id) // ili .FirstOrDefault()
                .FirstOrDefault();

            return View(poslednjiSmer);
        }


        public ActionResult MatematickiSmer()
        {
            var smer = db.MatematickiSmer
                         .OrderByDescending(x => x.Id)
                         .FirstOrDefault();

            return View(smer);
        }



        public ActionResult DodajJezickiSmer()
        {
            var podaci = db.JezickiSmer.ToList();
            return View(podaci);
        }

        [HttpPost]
        public ActionResult DodajJezickiSmer(HttpPostedFileBase Slika1, HttpPostedFileBase Slika2, HttpPostedFileBase Slika3, HttpPostedFileBase Slika4, string Opis)
        {
            string folder = Server.MapPath("~/Content/slike/");
            Directory.CreateDirectory(folder);

            string path1 = Slika1 != null ? "/Content/slike/" + Path.GetFileName(Slika1.FileName) : null;
            string path2 = Slika2 != null ? "/Content/slike/" + Path.GetFileName(Slika2.FileName) : null;
            string path3 = Slika3 != null ? "/Content/slike/" + Path.GetFileName(Slika3.FileName) : null;
            string path4 = Slika4 != null ? "/Content/slike/" + Path.GetFileName(Slika4.FileName) : null;

            Slika1?.SaveAs(Path.Combine(folder, Path.GetFileName(Slika1.FileName)));
            Slika2?.SaveAs(Path.Combine(folder, Path.GetFileName(Slika2.FileName)));
            Slika3?.SaveAs(Path.Combine(folder, Path.GetFileName(Slika3.FileName)));
            Slika4?.SaveAs(Path.Combine(folder, Path.GetFileName(Slika4.FileName)));

            var model = new JezickiSmer
            {
                Slika1 = path1,
                Slika2 = path2,
                Slika3 = path3,
                Slika4 = path4,
                Opis = Opis
            };

            db.JezickiSmer.Add(model);
            db.SaveChanges();

            return RedirectToAction("DodajJezickiSmer");
        }







        // GET: DodajJezickiSmer/AzuriranjeJsmera/5
        public ActionResult AzuriranjeJsmera(int id)
        {
            var smer = db.JezickiSmer.Find(id);
            if (smer == null)
            {
                return HttpNotFound();
            }
            return View(smer);
        }

        // POST: DodajJezickiSmer/AzuriranjeJsmera/5
        [HttpPost]
        public ActionResult AzuriranjeJsmera(int id, HttpPostedFileBase Slika1, HttpPostedFileBase Slika2, HttpPostedFileBase Slika3, HttpPostedFileBase Slika4, string Opis)
        {
            var smer = db.JezickiSmer.Find(id);
            if (smer == null)
            {
                return HttpNotFound();
            }

            string folder = Server.MapPath("~/Content/slike/");
            Directory.CreateDirectory(folder);

            if (Slika1 != null)
            {
                string path = "/Content/slike/" + Path.GetFileName(Slika1.FileName);
                Slika1.SaveAs(Path.Combine(folder, Path.GetFileName(Slika1.FileName)));
                smer.Slika1 = path;
            }
            if (Slika2 != null)
            {
                string path = "/Content/slike/" + Path.GetFileName(Slika2.FileName);
                Slika2.SaveAs(Path.Combine(folder, Path.GetFileName(Slika2.FileName)));
                smer.Slika2 = path;
            }
            if (Slika3 != null)
            {
                string path = "/Content/slike/" + Path.GetFileName(Slika3.FileName);
                Slika3.SaveAs(Path.Combine(folder, Path.GetFileName(Slika3.FileName)));
                smer.Slika3 = path;
            }
            if (Slika4 != null)
            {
                string path = "/Content/slike/" + Path.GetFileName(Slika4.FileName);
                Slika4.SaveAs(Path.Combine(folder, Path.GetFileName(Slika4.FileName)));
                smer.Slika4 = path;
            }

            smer.Opis = Opis;

            db.SaveChanges();
            return RedirectToAction("DodajJezickiSmer");
        }


        public ActionResult BrisanjeJsmera(int id)
        {
            var smer = db.JezickiSmer.Find(id);
            if (smer == null)
            {
                return HttpNotFound();
            }

            db.JezickiSmer.Remove(smer);
            db.SaveChanges();

            return RedirectToAction("DodajJezickiSmer");
        }





        public ActionResult DodajSlider()
        {
            var model = db.Slider.ToList();
            return View(model);
        }

        // POST: DodajSlider
        [HttpPost]
       
        public ActionResult DodajSlider(HttpPostedFileBase slika1, HttpPostedFileBase slika2, HttpPostedFileBase slika3)
        {
            string folder = Server.MapPath("~/Content/slike/");
            Directory.CreateDirectory(folder);

            string putanja1 = SnimiSliku(slika1, folder);
            string putanja2 = SnimiSliku(slika2, folder);
            string putanja3 = SnimiSliku(slika3, folder);

            Slider novi = new Slider
            {
                Slika1 = putanja1,
                Slika2 = putanja2,
                Slika3 = putanja3
            };

            db.Slider.Add(novi);
            db.SaveChanges();

            var model = db.Slider.ToList();
            return View(model); // Vrati novi model s dodatkom
        }

        private string SnimiSliku(HttpPostedFileBase fajl, string folder)
        {
            if (fajl != null && fajl.ContentLength > 0)
            {
                string ime = Path.GetFileName(fajl.FileName);
                string punaPutanja = Path.Combine(folder, ime);
                fajl.SaveAs(punaPutanja);
                return "/Content/slike/" + ime;
            }
            return null;
        }







        // GET: DodajSlider/AzuriranjeSlider/5
        public ActionResult AzuriranjeSlider(int id)
        {
            var slider = db.Slider.Find(id);
            if (slider == null)
            {
                return HttpNotFound();
            }
            return View(slider);
        }

        // POST: DodajSlider/AzuriranjeSlider/5
        [HttpPost]
       
        public ActionResult AzuriranjeSlider(int id, HttpPostedFileBase slika1, HttpPostedFileBase slika2, HttpPostedFileBase slika3)
        {
            var slider = db.Slider.Find(id);
            if (slider == null)
            {
                return HttpNotFound();
            }

            string folder = Server.MapPath("~/Content/slike/");

            if (slika1 != null) slider.Slika1 = SnimiSliku(slika1, folder);
            if (slika2 != null) slider.Slika2 = SnimiSliku(slika2, folder);
            if (slika3 != null) slider.Slika3 = SnimiSliku(slika3, folder);

            db.SaveChanges();
            return RedirectToAction("DodajSlider");
        }

        // GET: DodajSlider/BrisiSlider/5
        public ActionResult BrisiSlider(int id)
        {
            var slider = db.Slider.Find(id);
            if (slider == null)
            {
                return HttpNotFound();
            }

            db.Slider.Remove(slider);
            db.SaveChanges();

            return RedirectToAction("DodajSlider");
        }












        public ActionResult DodajMatematickiSmer()
            {
                var lista = db.MatematickiSmer.ToList();
                return View(lista);
            }

            [HttpPost]
            public ActionResult DodajMatematickiSmer(HttpPostedFileBase slika1, HttpPostedFileBase slika2, HttpPostedFileBase slika3, HttpPostedFileBase slika4, string opis)
            {
                string folderPath = Server.MapPath("~/Content/slike/");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string Putanja(HttpPostedFileBase file)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(file.FileName);
                        string fullPath = Path.Combine(folderPath, fileName);
                        file.SaveAs(fullPath);
                        return "/Content/slike/" + fileName;
                    }
                    return null;
                }

                var model = new MatematickiSmer
                {
                    Slika1 = Putanja(slika1),
                    Slika2 = Putanja(slika2),
                    Slika3 = Putanja(slika3),
                    Slika4 = Putanja(slika4),
                    Opis = opis
                };

                db.MatematickiSmer.Add(model);
                db.SaveChanges();

                return RedirectToAction("DodajMatematickiSmer");
            }





        public ActionResult AzurirajMatematickiSmer(int id)
        {
            var model = db.MatematickiSmer.Find(id);
            if (model == null)
                return HttpNotFound();

            return View(model);
        }

        [HttpPost]
        public ActionResult AzurirajMatematickiSmer(int id, MatematickiSmer updated, HttpPostedFileBase slika1, HttpPostedFileBase slika2, HttpPostedFileBase slika3, HttpPostedFileBase slika4)
        {
            var model = db.MatematickiSmer.Find(id);
            if (model == null)
                return HttpNotFound();

            string folderPath = Server.MapPath("~/Content/slike/");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string SaveFile(HttpPostedFileBase file)
            {
                if (file != null && file.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(file.FileName);
                    string fullPath = Path.Combine(folderPath, fileName);
                    file.SaveAs(fullPath);
                    return "/Content/slike/" + fileName;
                }
                return null;
            }

            model.Slika1 = SaveFile(slika1) ?? model.Slika1;
            model.Slika2 = SaveFile(slika2) ?? model.Slika2;
            model.Slika3 = SaveFile(slika3) ?? model.Slika3;
            model.Slika4 = SaveFile(slika4) ?? model.Slika4;
            model.Opis = updated.Opis;

            db.SaveChanges();
            return RedirectToAction("DodajMatematickiSmer");
        }






        public ActionResult IzbrisiMatematickiSmer(int id)
        {
            var model = db.MatematickiSmer.Find(id);
            if (model == null)
                return HttpNotFound();

            db.MatematickiSmer.Remove(model);
            db.SaveChanges();

            return RedirectToAction("DodajMatematickiSmer");
        }








        [HttpGet]
        public ActionResult UbaciRaspored()
        {
            var sviRasporedi = db.Rasporedi.OrderByDescending(r => r.ID).ToList();
            ViewBag.SviRasporedi = sviRasporedi;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UbaciRaspored(RasporedViewModel model)
        {
            if (ModelState.IsValid)
            {
                string folder = Server.MapPath("~/Content/slike/");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string file1 = Path.GetFileName(model.Slika1.FileName);
                string file2 = Path.GetFileName(model.Slika2.FileName);

                string putanja1 = Path.Combine(folder, file1);
                string putanja2 = Path.Combine(folder, file2);

                model.Slika1.SaveAs(putanja1);
                model.Slika2.SaveAs(putanja2);

                var raspored = new Rasporedi
                {
                    Naslov = model.Naslov,
                    Slika1 = "/Content/slike/" + file1,
                    Slika2 = "/Content/slike/" + file2
                };

                db.Rasporedi.Add(raspored);
                db.SaveChanges();

                TempData["Message"] = "Распоред је успешно убачен!";
                return RedirectToAction("UbaciRaspored");
            }

            TempData["Message"] = "Грешка при уносу.";
            return View(model);
        }








        // GET: prikaz forme za izmenu
        public ActionResult IzmeniRaspored(int id)
        {
            using (var db = new GimnazijaDBEntities())
            {
                var raspored = db.Rasporedi.Find(id);
                if (raspored == null)
                    return HttpNotFound();

                var model = new RasporedViewModel
                {
                    ID = raspored.ID,
                    Naslov = raspored.Naslov,
                    // Slike ne stavljamo u model jer su fajlovi, ali možeš prikazati stare u View
                };
                ViewBag.StaraSlika1 = raspored.Slika1;
                ViewBag.StaraSlika2 = raspored.Slika2;

                return View(model);
            }
        }

        // POST: update rasporeda
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IzmeniRaspored(RasporedViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new GimnazijaDBEntities())
                {
                    var raspored = db.Rasporedi.Find(model.ID);
                    if (raspored == null)
                        return HttpNotFound();

                    raspored.Naslov = model.Naslov;

                    string folder = Server.MapPath("~/Content/slike");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    // Provera i cuvanje nove slike 1
                    if (model.Slika1 != null)
                    {
                        string file1 = Path.GetFileName(model.Slika1.FileName);
                        string putanja1 = Path.Combine(folder, file1);
                        model.Slika1.SaveAs(putanja1);
                        raspored.Slika1 = "/Content/slike/" + file1;
                    }

                    // Provera i cuvanje nove slike 2
                    if (model.Slika2 != null)
                    {
                        string file2 = Path.GetFileName(model.Slika2.FileName);
                        string putanja2 = Path.Combine(folder, file2);
                        model.Slika2.SaveAs(putanja2);
                        raspored.Slika2 = "/Content/slike/" + file2;
                    }

                    db.SaveChanges();
                }
                TempData["Message"] = "Распоред је успешно ажуриран!";
                return RedirectToAction("UbaciRaspored");
            }

            ViewBag.Message = "Грешка при ажурирању.";
            return View(model);
        }

        // POST: brisanje rasporeda
        public ActionResult IzbrisiRaspored()
        {
            using (var db = new GimnazijaDBEntities())
            {
                var sviRasporedi = db.Rasporedi.OrderByDescending(r => r.ID).ToList();
                return View(sviRasporedi);
            }
        }


        [HttpPost]
        //  [ValidateAntiForgeryToken]
        public ActionResult IzbrisiRaspored(int id)
        {
            using (var db = new GimnazijaDBEntities())
            {
                var raspored = db.Rasporedi.Find(id);
                if (raspored == null)
                    return HttpNotFound();

                // Briši slike sa servera ako postoje
                var fullPath1 = Server.MapPath("~" + raspored.Slika1);
                if (System.IO.File.Exists(fullPath1))
                    System.IO.File.Delete(fullPath1);

                var fullPath2 = Server.MapPath("~" + raspored.Slika2);
                if (System.IO.File.Exists(fullPath2))
                    System.IO.File.Delete(fullPath2);

                db.Rasporedi.Remove(raspored);
                db.SaveChanges();
            }

            TempData["Message"] = "Распоред је успешно избрисан!";
            return RedirectToAction("IzbrisiRaspored");
        }




        public ActionResult DodajMaturante()
        {
            var svi = db.Maturanti.ToList();
            return View("DodajMaturante", svi);
        }
        [HttpPost]
        public ActionResult DodajMaturante(Maturanti maturant, HttpPostedFileBase slika)
        {
            if (ModelState.IsValid)
            {
                if (slika != null && slika.ContentLength > 0)
                {
                    string imeFajla = Path.GetFileName(slika.FileName);
                    string putanja = Path.Combine(Server.MapPath("~/Content/slike/"), imeFajla);
                    slika.SaveAs(putanja);

                    maturant.SlikaPath = "/Content/slike/" + imeFajla;
                }

                db.Maturanti.Add(maturant);
                db.SaveChanges();

                return RedirectToAction("DodajMaturante");
            }

            var svi = db.Maturanti.ToList();
            return View(svi);
        }








        public ActionResult PogledajRaspored()
        {
            using (var db = new GimnazijaDBEntities())
            {
                var rasporedi = db.Rasporedi.OrderByDescending(r => r.ID).ToList();
                return View(rasporedi);
            }
        }


        // GET: Home/Edit/5
        public ActionResult Edit(int id)
        {
            var maturant = db.Maturanti.Find(id);
            if (maturant == null)
                return HttpNotFound();

            return View(maturant);
        }

        // Prikaz forme za izmenu maturanta
        public ActionResult AzurirajMaturante(int id)
        {
            var maturant = db.Maturanti.Find(id);
            if (maturant == null)
                return HttpNotFound();

            return View(maturant);
        }

        // POST - snimanje izmene
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AzurirajMaturante(Maturanti maturant, HttpPostedFileBase slika)
        {
            if (ModelState.IsValid)
            {
                if (slika != null && slika.ContentLength > 0)
                {
                    string imeFajla = Path.GetFileName(slika.FileName);
                    string putanja = Path.Combine(Server.MapPath("~/Content/slike/"), imeFajla);
                    slika.SaveAs(putanja);
                    maturant.SlikaPath = "/Content/slike/" + imeFajla;
                }
                else
                {
                    // Sačuvaj staru sliku ako nema nove
                    var stari = db.Maturanti.AsNoTracking().FirstOrDefault(m => m.MaturantID == maturant.MaturantID);
                    if (stari != null)
                        maturant.SlikaPath = stari.SlikaPath;
                }

                db.Entry(maturant).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("DodajMaturante");
            }
            return View(maturant);
        }

        // GET - potvrda za brisanje
        public ActionResult IzbrisiMaturante(int id)
        {
            var maturant = db.Maturanti.Find(id);
            if (maturant == null)
                return HttpNotFound();

            return View(maturant);
        }

        // POST - brisanje
        [HttpPost, ActionName("IzbrisiMaturante")]
        [ValidateAntiForgeryToken]
        public ActionResult IzbrisiMaturanteConfirmed(int id)
        {
            var maturant = db.Maturanti.Find(id);
            if (maturant != null)
            {
                db.Maturanti.Remove(maturant);
                db.SaveChanges();
            }
            return RedirectToAction("DodajMaturante");
        }



        public ActionResult AzurirajEks(int id)
        {
            var eks = db.Ekskurzije.Find(id);
            if (eks == null)
                return HttpNotFound();

            return View(eks);
        }

        // POST: AzurirajEkskurziju/Edit/5
        [HttpPost]
        public ActionResult AzurirajEks(int id, string Naslov, string Opis, IEnumerable<HttpPostedFileBase> slike)
        {
            var eks = db.Ekskurzije.Find(id);
            if (eks == null)
                return HttpNotFound();

            eks.Naslov = Naslov;
            eks.Opis = Opis;

            int i = 1;
            foreach (var file in slike)
            {
                if (file != null && file.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(file.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/slike/"), fileName);
                    file.SaveAs(path);

                    typeof(Ekskurzije).GetProperty("Slika" + i).SetValue(eks, "/Content/slike/" + fileName);
                }
                i++;
                if (i > 8) break;
            }

            db.SaveChanges();
            return RedirectToAction("DodajEkskurziju");
        }
    
        public ActionResult IzbrisiEks(int id)
        {
            var eks = db.Ekskurzije.Find(id);
            if (eks == null)
                return HttpNotFound();

            db.Ekskurzije.Remove(eks);
            db.SaveChanges();

            return RedirectToAction("DodajEkskurziju");
        }
    


        public ActionResult DodajEkskurziju()
        {
            var lista = db.Ekskurzije.ToList();
            return View(lista);
        }

        // POST: DodajEkskurziju
        [HttpPost]
        public ActionResult DodajEkskurziju(string Naslov, string Opis, IEnumerable<HttpPostedFileBase> slike)
        {
            var eks = new Ekskurzije
            {
                Naslov = Naslov,
                Opis = Opis
            };

            int i = 1;
            foreach (var file in slike)
            {
                if (file != null && file.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(file.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/slike/"), fileName);
                    file.SaveAs(path);

                    typeof(Ekskurzije).GetProperty("Slika" + i).SetValue(eks, "/Content/slike/" + fileName);
                    i++;
                    if (i > 8) break;
                }
            }

            db.Ekskurzije.Add(eks);
            db.SaveChanges();

            return RedirectToAction("DodajEkskurziju");
        }
    }
}










