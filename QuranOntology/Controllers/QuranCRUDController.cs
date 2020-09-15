using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using QuranOntology.Models.QuranEF;
using System.ComponentModel.DataAnnotations;

namespace QuranOntology.Controllers
{
    public class QuranCRUDController : Controller
    {
        private QuranDBEntities db = new QuranDBEntities();

        // GET: /QuranCRUD/
        public ActionResult Index()
        {
            return View(db.Qurans.ToList());
        }

        // GET: /QuranCRUD/Details/5
        public ActionResult Details(byte? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quran quran = db.Qurans.Find(id);
            if (quran == null)
            {
                return HttpNotFound();
            }
            return View(quran);
        }

        // GET: /QuranCRUD/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /QuranCRUD/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="ID,DatabaseID,SuraID,VerseID,AyahText")] Quran quran)
        {
            if (ModelState.IsValid)
            {
                db.Qurans.Add(quran);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(quran);
        }

        // GET: /QuranCRUD/Edit/5
        public ActionResult Edit(byte? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quran quran = db.Qurans.Find(id);
            if (quran == null)
            {
                return HttpNotFound();
            }
            return View(quran);
        }

        // POST: /QuranCRUD/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="ID,DatabaseID,SuraID,VerseID,AyahText")] Quran quran)
        {
            if (ModelState.IsValid)
            {
                db.Entry(quran).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(quran);
        }

        // GET: /QuranCRUD/Delete/5
        public ActionResult Delete(byte? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quran quran = db.Qurans.Find(id);
            if (quran == null)
            {
                return HttpNotFound();
            }
            return View(quran);
        }

        // POST: /QuranCRUD/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(byte id)
        {
            Quran quran = db.Qurans.Find(id);
            db.Qurans.Remove(quran);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult SearchEnglish()
        {
            return View();
        }

        public JsonResult VerseSearch(string term) {

          
            if (term != "")
            {
              var searchResults = (from verses in db.QuranTranslationByAhmedAlis
                                     where verses.AyahText.Contains(term)
                                     select new {

                                         suranumber = verses.SuraID,
                                         ayatnumber = verses.VerseID,
                                         ayatText = verses.AyahText


                                     }).ToList();
                return Json(searchResults);
            }

            return Json("");

        }

        public JsonResult ayatRefsOnly(string surah, string Ayat) {

            int surat = Convert.ToInt32(surah);
            int ayat = Convert.ToInt32(Ayat);


            var ontologyID = (from ontol in db.Ontologies
                              where ontol.SuraID == surat && ontol.VerseID == ayat
                              select ontol.ID).FirstOrDefault();

            var ref_list = (from refe in db.Refrences
                            where refe.OntologiesID == ontologyID
                            select new
                            {
                                surahID = refe.SuraID,
                                ayahID = refe.VerseID,
                                AyatText = (from arb in db.QuranTranslationByAhmedAlis
                                            where arb.SuraID == refe.SuraID && arb.VerseID == refe.VerseID
                                            select arb.AyahText).FirstOrDefault()

                            }).ToList();


            return Json(ref_list);
        }

        public class SurahModel
        {
            [Display(Name = "Select Surah Number")]
            public List<SelectListItem> surah_numbers { get; set; }
        }
 

        public ActionResult AddRefrence()
        {
            List<SelectListItem> sura = db.Qurans.GroupBy(x => x.SuraID).AsEnumerable().Select(t =>
            new SelectListItem
            {
                Text = t.First().SuraID.ToString(),
                Value = t.First().SuraID.ToString()
            }).ToList();

            SurahModel view_data = new SurahModel() {
                surah_numbers = sura
            };

            return View(view_data);

        }

        public JsonResult GetVerses(string id)
        {

            List<SelectListItem> Verses = db.Qurans.AsEnumerable().Where(rec => rec.SuraID == Convert.ToInt32(id)).Select(t =>
            new SelectListItem
            {
                Text = t.VerseID.ToString(),
                Value = t.VerseID.ToString()
            }).ToList();

            return Json(new SelectList(Verses, "Value", "Text"));

        }


        public JsonResult GetAyatText(string surahID,string AyahID) {

            int surat = Convert.ToInt32(surahID);
            int ayat = Convert.ToInt32(AyahID);

            var arbiText = (from arb in db.Qurans
                            from eng in db.QuranTranslationByAhmedAlis
                            from urd in db.QuranUrduByJalandhris
                            where arb.SuraID == surat && arb.VerseID == ayat &&
                            eng.SuraID == surat && eng.VerseID == ayat &&
                            urd.SuraID == surat && urd.VerseID == ayat
                            select new {
                                arbi = arb.AyahText,
                                english = eng.AyahText,
                                urdu = urd.AyahText
                            }).FirstOrDefault();

            var ontologyID = (from ontol in db.Ontologies
                              where ontol.SuraID == surat && ontol.VerseID == ayat
                              select ontol.ID).FirstOrDefault();

            var ref_list = (from refe in db.Refrences
                            where refe.OntologiesID == ontologyID
                            select new  {
                                surahID = refe.SuraID,
                                ayahID = refe.VerseID,
                                refrenceID = refe.ID,
                                AyatText = (from arb in db.Qurans
                                            where arb.SuraID == refe.SuraID && arb.VerseID == refe.VerseID
                                            select arb.AyahText).FirstOrDefault()

                            }).ToList();

            var wholedata = new {
                ayatText = arbiText,
                ayatRefrences = ref_list
            };


            return Json(wholedata);
        }

      
                    

        public JsonResult saveRefrence( string mainSurahID, string mainAyatID , string surahID , string AyahID)
        {

            int Msurat = Convert.ToInt32(mainSurahID);
            int Mayat = Convert.ToInt32(mainAyatID);
            int surat = Convert.ToInt32(surahID);
            int ayat = Convert.ToInt32(AyahID);

            var ontologyID = (from ontol in db.Ontologies
                              where ontol.SuraID == Msurat && ontol.VerseID == Mayat
                              select ontol.ID).FirstOrDefault();

            Refrence new_Refrence = new Refrence() {
                OntologiesID = ontologyID,
                SuraID = (Byte)surat,
                VerseID = (short)ayat
            };

            db.Refrences.Add(new_Refrence);
            db.SaveChanges();

            return Json("Refrence Added");
        }


        public JsonResult DeleteRefrence(string id)
        {
            int id_dlt = Convert.ToInt32(id);
            Refrence ref_dlt = db.Refrences.Find(id_dlt);
            if (ref_dlt == null)
            {
                return Json("Not Found");
            }
            else
            {
                db.Refrences.Remove(ref_dlt);
                db.SaveChanges();
                return Json("Ok");
            }
            
        }
    protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
