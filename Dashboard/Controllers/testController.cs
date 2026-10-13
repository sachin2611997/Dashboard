using Dashboard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Dashboard.Controllers
{
    public class testController : Controller
    {
        DbEntity db = new DbEntity();
        // GET: test
        public ActionResult Index()
        {
            var allAgeData = db.Report_2021.Select(x => x.BROAD_AGE).Concat(db.Report_2022.Select(x => x.BROAD_AGE)).ToList();
            ViewBag.agedata = allAgeData.GroupBy(x => x).Select(y => new { y.Key, Count = Math.Round(y.Count() * 100.0 / allAgeData.Count, 1) })
                            .OrderBy(i => i.Key == ">=60").ThenBy(i => i.Key == "46-59").ThenBy(i => i.Key == "20-45").ThenBy(i => i.Key == "0-19").ToList();
            return View();
        }
    }
}