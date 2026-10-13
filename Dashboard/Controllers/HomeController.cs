using Dashboard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

public class HomeController : Controller
{
    DbEntity db = new DbEntity();


    // ============================================================
    // INDEX
    // ============================================================

    public ActionResult Dashboard()
    {
        return View();
    }

    public ActionResult Index()
    {
      
        // TOTAL COUNTS
      
        var mumbaiCount = db.Report_2021.Count();
        var vizagCount = db.Report_2022.Count();
        var vizagcounttti = db.Report_2022.Where(x=>x.final_tti!=null).Count();
        var totalCount = mumbaiCount + vizagCount;
 
        // MALE / FEMALE COUNTS

        var mumbaiMaleCount = db.Report_2021.Count(x => x.SEX == "M");
        var mumbaiFemaleCount = db.Report_2021.Count(x => x.SEX == "F");
        var vizagMaleCount =  db.Report_2022.Count(x => x.SEX == "M");
        var vizagFemaleCount =  db.Report_2022.Count(x => x.SEX == "F");
   
        // COMBINED MALE / FEMALE
      
        var totalMaleCount = mumbaiMaleCount + vizagMaleCount;
        var totalFemaleCount =  mumbaiFemaleCount + vizagFemaleCount;
  
        // VIEWBAG COUNTS
  
        ViewBag.TotalCount = totalCount;

        ViewBag.MumbaiCount = mumbaiCount;
        ViewBag.VizagCount = vizagCount;

        ViewBag.MumbaiMaleCount = mumbaiMaleCount;
        ViewBag.MumbaiFemaleCount = mumbaiFemaleCount;

        ViewBag.VizagMaleCount = vizagMaleCount;
        ViewBag.VizagFemaleCount = vizagFemaleCount;

        ViewBag.TotalMaleCount = totalMaleCount;
        ViewBag.TotalFemaleCount = totalFemaleCount;
   
        // STATUS ORDER
     
        var statusOrder = new[]
        {
            "Localised",
            "Regional",
            "Dist. Mets",
            "Too Advanced",
            "Not applicable",
            "Not Known",
            "Unk. Primary",
            "Treated outside"
        };
     
        // FIXED COLORS
      
        var colorMap = new Dictionary<string, string>
        {
            { "Treated outside", "#37AFE1" },
            { "Unk. Primary", "#00A8FF" },
            { "Not Known", "#8D0B41" },
            { "Not applicable", "#9B59B6" },
            { "Too Advanced", "#FF33A1" },
            { "Dist. Mets", "#FF2929" },
            { "Regional", "#FF8000" },
            { "Localised", "#33FF57" }
        };
      
        // DEFAULT COLORS
  
        var defaultColors = new[]
        {
            "#FF00FF",
            "#00FFFF",
            "#FFFF00",
            "#FF6347",
            "#B0E0E6"
        };
      
        // CREATE CLINICAL CHART DATA
        
        Func<List<string>, int, object> createChartData = (data, total) =>
            {
                if (total == 0)
                {
                    return new List<object>();
                }

                var random = new Random();

                var result =  data .GroupBy(x => x) .Select(g => new
                    {
                        Key = g.Key,
                        Count = g.Count(),
                        Percentage = Math.Round( g.Count() * 100.0 / total,1 )
                    })
                    .OrderBy(x =>  Array.IndexOf(  statusOrder, x.Key  ) == -1 ? 999  : Array.IndexOf( statusOrder, x.Key ))
                    .Select(item =>
                    {
                        string color;

                        if (!colorMap.TryGetValue(  item.Key,  out color))
                        {
                            color =   defaultColors[  random.Next(defaultColors.Length) ];
                        }

                        return new
                        {
                            item.Key,  item.Count, item.Percentage, color
                        };
                    }) .ToList();

                return result;
            };

        // MUMBAI CLINICAL DATA
     
        var mumbaiStatusData =  db.Report_2021 .Select(x => x.N_CLINEXT) .ToList();
        ViewBag.MumbaiData =createChartData(  mumbaiStatusData,  mumbaiStatusData.Count  );

        // MUMBAI AGE DATA
      
        ViewBag.ageMumbaidata = CreateAgeData(  db.Report_2021.Select(x => x.BROAD_AGE).ToList(), mumbaiCount  );
   
        // VIZAG CLINICAL DATA
      
        var vizagStatusData = db.Report_2022  .Select(x => x.N_CLINEXT) .ToList();
        ViewBag.VizagData = createChartData( vizagStatusData, vizagStatusData.Count );
      
        // VIZAG AGE DATA
   
        ViewBag.ageVizagdata = CreateAgeData(  db.Report_2022 .Select(x => x.BROAD_AGE).ToList(),  vizagCount );
      
        // COMBINED CLINICAL DATA
       
        var allStatusData =  db.Report_2021  .Select(x => x.N_CLINEXT) .Concat( db.Report_2022.Select(x => x.N_CLINEXT) ) .ToList();

        ViewBag.Alldata = createChartData( allStatusData,  allStatusData.Count  );
  
        // COMBINED AGE DATA
    
        var allAgeData = db.Report_2021 .Select(x => x.BROAD_AGE) .Concat( db.Report_2022 .Select(x => x.BROAD_AGE) ) .ToList();
        ViewBag.ageAlldata =  CreateAgeData( allAgeData, allAgeData.Count  );
     
        // TREATMENT - ALL
     

        ViewBag.obj =  GetTreatmentData( db.Report_2021, db.Report_2022 );
    
        // TREATMENT - MUMBAI
    
        ViewBag.MumbaiObj =  GetTreatmentData( db.Report_2021 );
    
        // TREATMENT - VIZAG
      
        ViewBag.VizagObj =   GetTreatmentData( db.Report_2022 );
      
        // TTI - ALL
    
        var allTtiData = db.Report_2021.Where(x => x.final_tti != null).Select(x => x.final_tti)
                         .Concat(db.Report_2022.Where(x => x.final_tti != null).Select(x => x.final_tti) ).ToList();
        ViewBag.alltti =   CreateTTIData( allTtiData,totalCount );


        // TTI - MUMBAI
    
        var mumbaiTtiData =  db.Report_2021 .Where(x => x.final_tti != null) .Select(x => x.final_tti).ToList();
        ViewBag.mumbaitti = CreateTTIData( mumbaiTtiData,mumbaiCount );

        // TTI - VIZAG
      
        var vizagTtiData = db.Report_2022.Where(x => x.final_tti != null).Select(x => x.final_tti).ToList();

        ViewBag.vizagtti = CreateTTIData( vizagTtiData, vizagCount);

        // INITIAL SYSTEM4 SITES
      
       ViewBag.Sites =  db.Report_2022.Where(x =>  x.System4 != null && x.System4 != "").Select(x => x.System4).Distinct().OrderBy(x => x).ToList();

        return View();
    }

    // GET SYSTEM4 SITES
    
    public JsonResult GetSystem4Sites(string center)
    {
        List<string> sites = new List<string>();


        if (center == "mumbai")
        {
            sites = db.Report_2021.Where(x =>  x.System4 != null && x.System4 != "").Select(x => x.System4).Distinct().OrderBy(x => x) .ToList();
        }

        else if (center == "vizag")
        {
            sites =  db.Report_2022.Where(x =>  x.System4 != null && x.System4 != "").Select(x => x.System4).Distinct().OrderBy(x => x).ToList();
        }

        else if (center == "registry")
        {
            var mumbaiSites =db.Report_2021.Where(x => x.System4 != null && x.System4 != "").Select(x => x.System4).Distinct().ToList();

            var vizagSites =db.Report_2022.Where(x =>x.System4 != null && x.System4 != "").Select(x => x.System4).Distinct().ToList();

            sites =mumbaiSites.Concat(vizagSites).Distinct().OrderBy(x => x).ToList();
        }

        return Json(  sites, JsonRequestBehavior.AllowGet );
    }



   
    // GET System2 SUBSITE

    public JsonResult GetSubSites(string center,string system4)
    {
        List<string> subSites =  new List<string>();

        if (string.IsNullOrEmpty(system4))
        {
            return Json( subSites, JsonRequestBehavior.AllowGet );
        }

        // MUMBAI
       
        if (center == "mumbai")
        {
            subSites = db.Report_2021.Where(x => x.System4 == system4 && x.System2 != null && x.System2 != "").Select(x => x.System2).Distinct().OrderBy(x => x).ToList();
        }

        // VIZAG
    
        else if (center == "vizag")
        {
            subSites = db.Report_2022 .Where(x =>x.System4 == system4 && x.System2 != null && x.System2 != "").Select(x => x.System2).Distinct().OrderBy(x => x) .ToList();
        }

        // REGISTRY
   
        else if (center == "registry")
        {
            var mumbaiSubSites = db.Report_2021 .Where(x => x.System4 == system4 && x.System2 != null && x.System2 != "") .Select(x => x.System2).Distinct().ToList();

            var vizagSubSites =db.Report_2022.Where(x => x.System4 == system4 && x.System2 != null && x.System2 != "").Select(x => x.System2) .Distinct().ToList();

            subSites = mumbaiSubSites.Concat(vizagSubSites).Distinct().OrderBy(x => x).ToList();
        }

        return Json(subSites,JsonRequestBehavior.AllowGet );
    }


    //Get AGE wise data
    public ActionResult GetAgewise(string center)
    {
        List<string> sites = new List<string>();


        if (center == "mumbai")
        {
            sites = db.Report_2021.Where(x => x.BROAD_AGE != null && x.BROAD_AGE != "").Select(x => x.BROAD_AGE).Distinct().OrderBy(x => x).ToList();
        }

        else if (center == "vizag")
        {
            sites = db.Report_2022.Where(x => x.BROAD_AGE != null && x.BROAD_AGE != "").Select(x => x.BROAD_AGE).Distinct().OrderBy(x => x).ToList();
        }

        else if (center == "registry")
        {
            var mumbaiSites = db.Report_2021.Where(x => x.BROAD_AGE != null && x.BROAD_AGE != "").Select(x => x.BROAD_AGE).Distinct().ToList();

            var vizagSites = db.Report_2022.Where(x => x.BROAD_AGE != null && x.BROAD_AGE != "").Select(x => x.BROAD_AGE).Distinct().ToList();

            sites = mumbaiSites.Concat(vizagSites).Distinct().OrderBy(x => x).ToList();
        }

        return Json(sites, JsonRequestBehavior.AllowGet);
    }



    // ============================================================
    // FILTERED DASHBOARD DATA
    //
    // CENTER
    //      registry
    //      mumbai
    //      vizag
    //
    // SYSTEM4
    //      Lung
    //      Breast
    //      etc.
    //
    // System2
    //      Bronchus
    //      etc.
    // ============================================================

    public JsonResult GetSiteDashboardData( string center, string system4,string System2, List<string> ages)
    {
        try
        {
            if (string.IsNullOrEmpty(center))
            {
                center = "registry";
            }

            if (system4 == null)
            {
                system4 = "";
            }

            if (System2 == null)
            {
                System2 = "";
            }
          
            // MUMBAI     

            if (center == "mumbai")
            {
                var query = db.Report_2021.AsQueryable();
                if (ages != null && ages.Count > 0)
                {
                    query = query.Where(x => ages.Contains(x.BROAD_AGE));
                }


                if (!string.IsNullOrEmpty(system4))
                {
                    query = query.Where( x => x.System4 == system4 );
                }

                if (!string.IsNullOrEmpty(System2))
                {
                    query = query.Where( x => x.System2 == System2 );
                }

                return Json( BuildMumbaiDashboard(  query ),JsonRequestBehavior.AllowGet);
            }

            // VIZAG
         
            if (center == "vizag")
            {
                var query = db.Report_2022.AsQueryable();
                if (ages != null && ages.Count > 0)
                {
                    query = query.Where(x => ages.Contains(x.BROAD_AGE));
                }


                if (!string.IsNullOrEmpty(system4))
                {
                    query = query.Where( x => x.System4 == system4 );
                }

                if (!string.IsNullOrEmpty(System2))
                {
                    query = query.Where(  x => x.System2 == System2 );
                }

                return Json( BuildVizagDashboard( query ), JsonRequestBehavior.AllowGet);
            }
         
            // REGISTRY 

            if (center == "registry")
            {
                var mumbaiQuery = db.Report_2021.AsQueryable();

                var vizagQuery =  db.Report_2022.AsQueryable();
                if (ages != null && ages.Count > 0)
                {
                    mumbaiQuery = mumbaiQuery.Where(x => ages.Contains(x.BROAD_AGE));
                    vizagQuery = vizagQuery.Where(x => ages.Contains(x.BROAD_AGE));
                }


                if (!string.IsNullOrEmpty(system4))
                {
                    mumbaiQuery = mumbaiQuery.Where( x => x.System4 == system4 );

                    vizagQuery = vizagQuery.Where( x => x.System4 == system4 );
                }


                if (!string.IsNullOrEmpty(System2))
                {
                    mumbaiQuery = mumbaiQuery.Where( x => x.System2 == System2 );

                    vizagQuery = vizagQuery.Where(  x => x.System2 == System2 );
                }


                return Json( BuildRegistryDashboard(mumbaiQuery, vizagQuery ),JsonRequestBehavior.AllowGet  );
            }


            return Json(  EmptyDashboard(), JsonRequestBehavior.AllowGet );
        }
        catch (Exception ex)
        {
            return Json(
                new
                {
                    total = 0,
                    male = 0,
                    female = 0,

                    clinicalData = new List<object>(),
                    ageData = new List<object>(),
                    treatmentData = new List<object>(),
                    ttiData = new List<object>(),
                    error = ex.Message
                },
                JsonRequestBehavior.AllowGet
            );
        }
    }


    // MUMBAI DASHBOARD

    private object BuildMumbaiDashboard( IQueryable<Report2021> query)
    {
        var total = query.Count();
        var male = query.Count( x => x.SEX == "M" );
        var female =query.Count( x => x.SEX == "F" );

        // CLINICAL

        var clinical = query.Where(x => x.N_CLINEXT != null).Select(x => x.N_CLINEXT).ToList();
        var clinicalData =CreateClinicalData( clinical);

        // AGE
    
        var age =  query .Where(x => x.BROAD_AGE != null).Select(x => x.BROAD_AGE).ToList();
        var ageData = CreateAgeData(age, total);
            
        // TREATMENT
    
        var treatmentData = GetTreatmentData(query );
       
        // TTI
    
        var tti = query.Where(x => x.final_tti != null) .Select(x => x.final_tti) .ToList();

        var ttiData =  CreateTTIData(  tti ,total);
        return new
        {
            total = total,
            male = male,
            female = female,

            clinicalData = clinicalData,
            ageData = ageData,
            treatmentData = treatmentData,
            ttiData = ttiData
        };
    }

  
    // VIZAG DASHBOARD
    

    private object BuildVizagDashboard( IQueryable<Report2022> query)
    {
        var total = query.Where(x => x.final_tti != null).Count();
        var male = query.Count( x => x.SEX == "M" );
        var female = query.Count( x => x.SEX == "F" );
     
        // CLINICAL
      
        var clinical =  query .Where(x => x.N_CLINEXT != null) .Select(x => x.N_CLINEXT).ToList();
        var clinicalData = CreateClinicalData(clinical );
   
        // AGE
      
        var age = query .Where(x => x.BROAD_AGE != null)  .Select(x => x.BROAD_AGE) .ToList();
        var ageData =CreateAgeData(age,total );
   
        // TREATMENT 
        var treatmentData =GetTreatmentData(query );
           
       // TTI
        
        var tti = query .Where(x => x.final_tti != null) .Select(x => x.final_tti) .ToList();
        var ttiData =CreateTTIData( tti,total );

        return new
        {
            total = total,
            male = male,
            female = female,
            clinicalData = clinicalData,
            ageData = ageData,
            treatmentData = treatmentData,
            ttiData = ttiData
        };
    }



    // ============================================================
    // REGISTRY DASHBOARD
    // MUMBAI + VIZAG
    // ============================================================

    private object BuildRegistryDashboard( IQueryable<Report2021> mumbaiQuery, IQueryable<Report2022> vizagQuery)
    {
        var mumbaiTotal =mumbaiQuery.Count();
        var vizagTotal = vizagQuery.Count();
        var total = mumbaiTotal + vizagTotal;
        var male =  mumbaiQuery.Count( x => x.SEX == "M" ) +vizagQuery.Count( x => x.SEX == "M" );
        var female = mumbaiQuery.Count( x => x.SEX == "F"  ) + vizagQuery.Count( x => x.SEX == "F" );
       
        // CLINICAL
     
        var mumbaiClinical =  mumbaiQuery .Where(x => x.N_CLINEXT != null) .Select(x => x.N_CLINEXT) .ToList();
        var vizagClinical =  vizagQuery.Where(x => x.N_CLINEXT != null)  .Select(x => x.N_CLINEXT) .ToList();
        var clinical =  mumbaiClinical .Concat(vizagClinical).ToList();
        var clinicalData = CreateClinicalData( clinical );
  
        // AGE
     
        var mumbaiAge = mumbaiQuery .Where(x => x.BROAD_AGE != null) .Select(x => x.BROAD_AGE).ToList();
        var vizagAge = vizagQuery .Where(x => x.BROAD_AGE != null) .Select(x => x.BROAD_AGE) .ToList();
        var age =  mumbaiAge .Concat(vizagAge) .ToList();
        var ageData = CreateAgeData( age, total );

        // TREATMENT
   
        var treatmentData =  GetTreatmentData( mumbaiQuery, vizagQuery );
  
        // TTI
        var mumbaiTti = mumbaiQuery .Where(x => x.final_tti != null).Select(x => x.final_tti) .ToList();
        var vizagTti = vizagQuery .Where(x => x.final_tti != null) .Select(x => x.final_tti).ToList();
        var tti =  mumbaiTti .Concat(vizagTti).ToList();
        var ttiData = CreateTTIData( tti,total );

        return new
        {
            total = total,
            male = male,
            female = female,

            clinicalData = clinicalData,
            ageData = ageData,
            treatmentData = treatmentData,
            ttiData = ttiData
        };
    }



    // ============================================================
    // CLINICAL DATA
    // ============================================================

    private List<object> CreateClinicalData( List<string> data)
    {
        var statusOrder =
            new[]
            {
                "Localised",
                "Regional",
                "Dist. Mets",
                "Too Advanced",
                "Not applicable",
                "Not Known",
                "Unk. Primary",
                "Treated outside"
            };

        var colorMap =
            new Dictionary<string, string>
            {
                { "Treated outside", "#37AFE1" },
                { "Unk. Primary", "#00A8FF" },
                { "Not Known", "#8D0B41" },
                { "Not applicable", "#9B59B6" },
                { "Too Advanced", "#FF33A1" },
                { "Dist. Mets", "#FF2929" },
                { "Regional", "#FF8000" },
                { "Localised", "#33FF57" }
            };

        if (data == null || data.Count == 0)
        {
            return new List<object>();
        }

        var grouped = data .GroupBy(x => x) .Select(g => new { Key = g.Key,Count = g.Count() }).ToList();

        var total =  grouped.Sum(x => x.Count);

        if (total == 0)
        {
            return new List<object>();
        }

        return grouped
            .Select(x => new
            {
                x.Key,
                x.Count,

                Percentage =  Math.Round(x.Count * 100.0 / total, 1 ),
                color =  colorMap.ContainsKey(x.Key) ? colorMap[x.Key]  : "#999999",
                Sort =    Array.IndexOf(statusOrder, x.Key) == -1 ? 999 : Array.IndexOf( statusOrder, x.Key )
            })
            .OrderBy(x => x.Sort)
            .Select(x => (object)new
            {
                x.Key,
                x.Count,
                x.Percentage,
                x.color
            })
           .ToList();
    }



    // ============================================================
    // AGE DATA
    // ============================================================

    private List<object> CreateAgeData( List<string> data, int total)
    {
        if (data == null || data.Count == 0)
        {
            return new List<object>();
        }

        var ageOrder =  new[] { "0 - 19", "20 - 45", "46 - 59", ">=60" };

        var filteredData = data .Where(x => x != null) .ToList();

        if (filteredData.Count == 0)
        {
            return new List<object>();
        }

        var denominator = filteredData.Count;

        return filteredData .GroupBy(x => x) .Select(g => new  { Key = g.Key, Count = g.Count(), Percentage =  Math.Round( g.Count() * 100.0 / denominator, 1 ),
                Sort = Array.IndexOf( ageOrder, g.Key ) == -1  ? 999: Array.IndexOf( ageOrder, g.Key )
            })
            .OrderBy(x => x.Sort) .Select(x => (object)new
            {
                x.Key,
                x.Count,
                x.Percentage
            })
            .ToList();
    }



    // ============================================================
    // TREATMENT DATA - MUMBAI
    // ============================================================

    private List<object> GetTreatmentData(IQueryable<Report2021> query)
    {
        var data =  query .Where(x => x.SIXTMT != null) .ToList();


        return data.Select(x =>
            {
                string treatment = x.TMH_RX;

                if (treatment == "Only Sx")
                {
                    return "Only Sx";
                }

                if (treatment == "Only RT")
                {
                    return "Only RT";
                }

                if (
                    treatment == "Only HT" ||
                    treatment == "Only Targeted" ||
                    treatment == "Only Immunotherapy" ||
                    treatment == "Only CT"
                )
                {
                    return "Only Systemic";
                }

                return "Combination";
            })
            .GroupBy(x => x)
            .Select(g => (object)new
            {
                Key = g.Key,
                Count = g.Count()
            })
            .ToList();
    }



    // ============================================================
    // TREATMENT DATA - VIZAG
    // ============================================================

    private List<object> GetTreatmentData( IQueryable<Report2022> query)
    {
        var data =  query.Where(x => x.SIXTMT != null) .ToList();


        return data
            .Select(x =>
            {
                string treatment =  x.TMH_RX;

                if (treatment == "Only Sx")
                {
                    return "Only Sx";
                }

                if (treatment == "Only RT")
                {
                    return "Only RT";
                }

                if (
                    treatment == "Only HT" ||
                    treatment == "Only Targeted" ||
                    treatment == "Only Immunotherapy" ||
                    treatment == "Only CT"
                )
                {
                    return "Only Systemic";
                }

                return "Combination";
            })
            .GroupBy(x => x)
            .Select(g => (object)new
            {
                Key = g.Key,
                Count = g.Count()
            })
            .ToList();
    }



    // ============================================================
    // TREATMENT DATA - REGISTRY
    // ============================================================

    private List<object> GetTreatmentData( IQueryable<Report2021> mumbaiQuery,IQueryable<Report2022> vizagQuery)
    {
        var mumbai = mumbaiQuery .Where(x => x.SIXTMT != null) .ToList()
            .Select(x =>
            {
                string treatment =   x.TMH_RX;

                if (treatment == "Only Sx")
                {
                    return "Only Sx";
                }

                if (treatment == "Only RT")
                {
                    return "Only RT";
                }

                if (
                    treatment == "Only HT" ||
                    treatment == "Only Targeted" ||
                    treatment == "Only Immunotherapy" ||
                    treatment == "Only CT"
                )
                {
                    return "Only Systemic";
                }

                return "Combination";
            });


        var vizag = vizagQuery .Where(x => x.SIXTMT != null) .ToList()
            .Select(x =>
            {
                string treatment =  x.TMH_RX;

                if (treatment == "Only Sx")
                {
                    return "Only Sx";
                }

                if (treatment == "Only RT")
                {
                    return "Only RT";
                }

                if (
                    treatment == "Only HT" ||
                    treatment == "Only Targeted" ||
                    treatment == "Only Immunotherapy" ||
                    treatment == "Only CT"
                )
                {
                    return "Only Systemic";
                }

                return "Combination";
            });


        return
            mumbai.Concat(vizag) .GroupBy(x => x) .Select(g => (object)new
            {
                Key = g.Key,
                Count = g.Count()
            })
            .ToList();
    }



    // ============================================================
    // TTI DATA
    // ============================================================

    private List<object> CreateTTIData( List<string> data,int total)
    {
        if (data == null)
        {
            return new List<object>();
        }
        var filtered =  data .Where(x => x != null).ToList();

        //var total =  filtered.Count;

        if (total == 0)
        {
            return new List<object>();
        }

        var ttiOrder = new[]
            {
                "<30 days",
                "31-90 days",
                "91-180 days",
                ">180 days"
            };


        return filtered.GroupBy(x => x) .Select(g => new
            {
                Key = g.Key,
                Count = g.Count(),
                Percentage = Math.Round(  g.Count() * 100.0 / total,  1 ),
                Sort =  Array.IndexOf(  ttiOrder,   g.Key ) == -1 ? 999 : Array.IndexOf(  ttiOrder, g.Key    )
            })
            .OrderBy(x => x.Sort)
            .Select(x => (object)new
            {
                x.Key,
                x.Count,
                x.Percentage,
                color = "#c51162"
            })
            .ToList();
    }



    // ============================================================
    // EMPTY DASHBOARD
    // ============================================================

    private object EmptyDashboard()
    {
        return new
        {
            total = 0,
            male = 0,
            female = 0,

            clinicalData =  new List<object>(),

            ageData =  new List<object>(),

            treatmentData = new List<object>(),

            ttiData =  new List<object>()
        };
    }

    public ActionResult Indiamap()
    {
        var rows = db.Report_2022.GroupBy(x=>x.STATE).Select(y=>new { state=y.Key,count=y.Count()}).ToList();
        // 2) state name (UPPERCASE) -> map ID
        var nameToId = new Dictionary<string, string>
    {
        {"ANDAMAN AND NICOBAR", "INAN"}, {"ANDHRA PRADESH", "INAP"}, {"ARUNACHAL PRADESH", "INAR"}, {"ASSAM", "INAS"},
        {"BIHAR", "INBR"},  {"CHANDIGARH", "INCH"}, {"CHHATTISGARH", "INCT"}, {"DADRA AND NAGAR HAVELI", "INDH"}, {"DAMAN AND DIU", "INDH"},
        {"DELHI", "INDL"}, {"GOA", "INGA"}, {"GUJARAT", "INGJ"}, {"HARYANA", "INHR"}, {"HIMACHAL PRADESH", "INHP"}, {"JHARKHAND", "INJH"},
        {"KARNATAKA", "INKA"},{"KERALA", "INKL"},  {"MADHYA PRADESH", "INMP"},  {"MAHARASHTRA", "INMH"}, {"MUMBAI", "INMH_Mumbai"}, {"MANIPUR", "INMN"},
        { "MEGALAYA", "INML"}, {"MEGHALAYA", "INML"}, {"MIZORAM", "INMZ"}, {"NAGALAND", "INNL"}, {"ODISHA", "INOR"}, {"ORISSA", "INOR"},{"PONDICHERRY", "INPY"},
        {"PUNJAB", "INPB"},{"RAJASTHAN", "INRJ"}, {"SIKKIM", "INSK"}, {"TAMIL NADU", "INTN"}, {"TELANGANA", "INTG"}, {"TRIPURA", "INTR"}, {"UTTAR PRADESH", "INUP"},
        {"UTTARAKHAND", "INUT"}, {"WEST BENGAL", "INWB"},{"LAKSHADWEEP", "INLD"}, {"JAMMU & KASHMIR", "INJK"}, {"LADAKH", "INLA"}

    };

        // 3) start every state at 0 so none is missing
        var result = nameToId.Values.Distinct().ToDictionary(id => id, id => 0);
        int foreign = 0;

        foreach (var r in rows)
        {
            var name = (r.state ?? "").Trim().ToUpper();
            string id;
            if (nameToId.TryGetValue(name, out id))
                result[id] += r.count;        // += so Mumbai + Maharashtra and Daman + Dadra get added together
            else if (name == "FOREIGN")
                foreign += r.count;
            // anything else is ignored
        }

        ViewBag.Foreign = foreign;
        ViewBag.MumbaiCount = result.ContainsKey("INMH_Mumbai") ? result["INMH_Mumbai"] : 0;

        return View(result);                   // Dictionary<string,int>
    }
   
 
   public ActionResult GetStateCounts()
    {
        var states = db.Report_2022.Where(x => x.STATE != null).Select(x => x.STATE).ToList();

        var rows = states.GroupBy(s => s.Trim().ToUpper()).Select(g => new
            {
                State = g.Key,
                Count = g.Count()
            }).ToList();

        var nameToId = new Dictionary<string, string>
    {
        {"ANDAMAN AND NICOBAR", "INAN"}, {"ANDHRA PRADESH", "INAP"}, {"ARUNACHAL PRADESH", "INAR"}, {"ASSAM", "INAS"},
        {"BIHAR", "INBR"},  {"CHANDIGARH", "INCH"}, {"CHHATTISGARH", "INCT"}, {"DADRA AND NAGAR HAVELI", "INDH"}, {"DAMAN AND DIU", "INDH"},
        {"DELHI", "INDL"}, {"GOA", "INGA"}, {"GUJARAT", "INGJ"}, {"HARYANA", "INHR"}, {"HIMACHAL PRADESH", "INHP"}, {"JHARKHAND", "INJH"},
        {"KARNATAKA", "INKA"},{"KERALA", "INKL"},  {"MADHYA PRADESH", "INMP"},  {"MAHARASHTRA", "INMH"}, {"MUMBAI", "INMH_Mumbai"}, {"MANIPUR", "INMN"},
        { "MEGALAYA", "INML"}, {"MEGHALAYA", "INML"}, {"MIZORAM", "INMZ"}, {"NAGALAND", "INNL"}, {"ODISHA", "INOR"}, {"ORISSA", "INOR"},{"PONDICHERRY", "INPY"},
        {"PUNJAB", "INPB"},{"RAJASTHAN", "INRJ"}, {"SIKKIM", "INSK"}, {"TAMIL NADU", "INTN"}, {"TELANGANA", "INTG"}, {"TRIPURA", "INTR"}, {"UTTAR PRADESH", "INUP"},
        {"UTTARAKHAND", "INUT"}, {"WEST BENGAL", "INWB"},{"LAKSHADWEEP", "INLD"}, {"JAMMU & KASHMIR", "INJK"}, {"LADAKH", "INLA"}
    };

       
       var result = nameToId.Values .Distinct().ToDictionary(id => id, id => 0);

        foreach (var row in rows)
        {
            string id;

            if (nameToId.TryGetValue(row.State, out id))
            {
                result[id] += row.Count;
            }
        }

        return Json(result, JsonRequestBehavior.AllowGet);

    }


}