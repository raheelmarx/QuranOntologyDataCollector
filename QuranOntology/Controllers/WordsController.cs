using QuranOntology.Models.QuranEF;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuranOntology.Controllers
{
    public class WordsController : Controller
    {
        private QuranDBEntities db = new QuranDBEntities();
        public class SurahModel
        {
            [Display(Name = "Select Surah Number")]
            public List<SelectListItem> surah_numbers { get; set; }
        }

        public ActionResult WordRefrences()
        {
            List<SelectListItem> sura = db.Qurans.GroupBy(x => x.SuraID).AsEnumerable().Select(t =>
              new SelectListItem
              {
                  Text = t.First().SuraID.ToString(),
                  Value = t.First().SuraID.ToString()
              }).ToList();

            SurahModel view_data = new SurahModel()
            {
                surah_numbers = sura
            };
            return View(view_data);
        }

        public ActionResult getWordData( string wordId) {

            long word_id = Convert.ToInt64(wordId);

            var wordRefs = (from word_refs in db.WordRefrences
                            where word_refs.NounVerbID == word_id && word_refs.NounVerbRefrenceID != null
                            select new {
                                wordText = db.NounVerbs.Where(word => word.ID == word_refs.NounVerbRefrenceID).Select(w => w.NounVerbText).FirstOrDefault(),
                                refrenceID = word_refs.ID
                            }).ToList();

            var verseRefs = (from word_refs in db.WordRefrences
                             where word_refs.NounVerbID == word_id && word_refs.RefrenceSuraID != null && word_refs.RefrenceVerseID != null
                             select new
                             {
                                 suraID = word_refs.RefrenceSuraID,
                                 ayatID = word_refs.RefrenceVerseID,
                                 ayatText = db.Qurans.Where(word => word.SuraID == word_refs.RefrenceSuraID && word.VerseID == word_refs.RefrenceVerseID)
                                 .Select(w => w.AyahText).FirstOrDefault(),
                                 refrenceID = word_refs.ID
                             }).ToList();

            var wholedata = new {
                word_refs = wordRefs,
                verse_refs = verseRefs
            };

            return Json(wholedata);
        }

        // GET: Words
        public ActionResult VerseWords()
        {
            List<SelectListItem> sura = db.Qurans.GroupBy(x => x.SuraID).AsEnumerable().Select(t =>
             new SelectListItem
             {
                 Text = t.First().SuraID.ToString(),
                 Value = t.First().SuraID.ToString()
             }).ToList();

            SurahModel view_data = new SurahModel()
            {
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


        public JsonResult GetAyatText(string surahID, string AyahID)
        {

            int surat = Convert.ToInt32(surahID);
            int ayat = Convert.ToInt32(AyahID);

            var arbiText = (from arb in db.Qurans
                            from eng in db.QuranTranslationByAhmedAlis
                            from urd in db.QuranUrduByJalandhris
                            where arb.SuraID == surat && arb.VerseID == ayat &&
                            eng.SuraID == surat && eng.VerseID == ayat &&
                            urd.SuraID == surat && urd.VerseID == ayat
                            select new
                            {
                                arbi = arb.AyahText,
                                english = eng.AyahText,
                                urdu = urd.AyahText
                            }).FirstOrDefault();

            var ontologyID = (from ontol in db.Ontologies
                              where ontol.SuraID == surat && ontol.VerseID == ayat
                              select ontol.ID).FirstOrDefault();

            var ref_list = (from refe in db.Refrences
                            where refe.OntologiesID == ontologyID
                            select new
                            {
                                surahID = refe.SuraID,
                                ayahID = refe.VerseID,
                                refrenceID = refe.ID,
                                AyatText = (from arb in db.Qurans
                                            where arb.SuraID == refe.SuraID && arb.VerseID == refe.VerseID
                                            select arb.AyahText).FirstOrDefault()

                            }).ToList();

            var wholedata = new
            {
                ayatText = arbiText,
                ayatRefrences = ref_list
            };


            return Json(wholedata);
        }
        


        public JsonResult addVerseRefrence(string word_id, string surahID, string AyahID)
        {
            int surat = Convert.ToInt32(surahID);
            int ayat = Convert.ToInt32(AyahID);
            long word = Convert.ToInt64(word_id);

            WordRefrence word_ref = new WordRefrence()
            {
                NounVerbID = word,
                RefrenceSuraID = (Byte)surat,
                RefrenceVerseID = (short)ayat
            };
            db.WordRefrences.Add(word_ref);
            db.SaveChanges();

            return Json("Refrence Added");
        }
        

        public JsonResult addWordRefrence(string main_word_id, string word_id)
        {
            long word = Convert.ToInt64(word_id);
            long main_word = Convert.ToInt64(main_word_id);

            WordRefrence word_ref = new WordRefrence()
            {
                NounVerbID = main_word,
                NounVerbRefrenceID = word
                
            };
            db.WordRefrences.Add(word_ref);
            db.SaveChanges();

            return Json("Refrence Added");
        }




        public JsonResult getAyatData(string surahID, string AyahID)
        {

            int surat = Convert.ToInt32(surahID);
            int ayat = Convert.ToInt32(AyahID);

            var arbiText = (from arb in db.Qurans
                            from eng in db.QuranTranslationByAhmedAlis
                            from urd in db.QuranUrduByJalandhris
                            where arb.SuraID == surat && arb.VerseID == ayat &&
                            eng.SuraID == surat && eng.VerseID == ayat &&
                            urd.SuraID == surat && urd.VerseID == ayat
                            select new
                            {
                                arbi = arb.AyahText,
                                english = eng.AyahText,
                                urdu = urd.AyahText
                            }).FirstOrDefault();

         

            var word_list = (from word in db.NounVerbs
                             from word_ref in db.WordRefrences
                            where word.ID == word_ref.NounVerbID &&  word_ref.RefrenceSuraID == surat && word_ref.RefrenceVerseID == ayat                     
                            select new
                            {                                
                               wordText = word.NounVerbText,
                               refrenceID = word_ref.ID
                            }).ToList();

            var wholedata = new
            {
                ayatText = arbiText,
                ayatwords = word_list
            };


            return Json(wholedata);
        }

        public JsonResult saveWord(string wordText, string SurahID, string AyatID)
        {

            int surat = Convert.ToInt32(SurahID);
            int ayat = Convert.ToInt32(AyatID);

            NounVerb new_word = new NounVerb()
            {
                NounVerbText = wordText,                
            };

            NounVerb saved_word = db.NounVerbs.Add(new_word);
            db.SaveChanges();
            WordRefrence word_ref = new WordRefrence()
            {
                NounVerbID = saved_word.ID,
                RefrenceSuraID = (Byte)surat,
                RefrenceVerseID = (short)ayat
            };
            db.WordRefrences.Add(word_ref);
            db.SaveChanges();

            return Json("Word Added");
        }

        public JsonResult DeleteWord(string id)
        {
            int id_dlt = Convert.ToInt32(id);
            WordRefrence ref_dlt = db.WordRefrences.Find(id_dlt);
            if (ref_dlt == null)
            {
                return Json("Not Found");
            }
            else
            {
                db.WordRefrences.Remove(ref_dlt);
                db.SaveChanges();
                return Json("Ok");
            }

        }

        

        public JsonResult saveIndivisual(string wordText)
        {           

            NounVerb new_word = new NounVerb()
            {
                NounVerbText = wordText
            };

            db.NounVerbs.Add(new_word);
            db.SaveChanges();

            return Json("Word Added");
        }


        public JsonResult GetWordsList()
        {

            var wordslist = db.NounVerbs.Select(word => new { Text = word.NounVerbText, Value = word.ID }).ToList();
            return Json(wordslist);

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