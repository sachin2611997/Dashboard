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


    private static readonly Dictionary<string, string[]> YearCenters = new Dictionary<string, string[]>
{
    { "2021", new[] { "mumbai" } },
    { "2022", new[] { "vizag"  } },
    { "2023", new string[0]      }      // no report yet
};

    // is there a report for this center in this year?  ("" = All Years = yes)
    private static bool YearHas(string year, string center)
    {
        if (string.IsNullOrEmpty(year)) return true;
        string[] list;
        return YearCenters.TryGetValue(year, out list) && list.Contains(center);
    }

    // should this center's table be read for the selected center + year?
    private static bool UseMumbai(string center, string year)
    {
        return (center == "mumbai" || center == "registry") && YearHas(year, "mumbai");
    }

    private static bool UseVizag(string center, string year)
    {
        return (center == "vizag" || center == "registry") && YearHas(year, "vizag");
    }

    public ActionResult Index()
    {
        ViewBag.YearCenters = YearCenters;
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
    
    public JsonResult GetSystem4Sites(string center, string year)
    {
        var sites = new List<string>();

        if (UseMumbai(center, year))
            sites.AddRange(db.Report_2021.Where(x => x.System4 != null && x.System4 != "")
                             .Select(x => x.System4).Distinct().ToList());

        if (UseVizag(center, year))
            sites.AddRange(db.Report_2022.Where(x => x.System4 != null && x.System4 != "")
                             .Select(x => x.System4).Distinct().ToList());

        return Json(sites.Distinct().OrderBy(x => x).ToList(), JsonRequestBehavior.AllowGet);
    }



   
    // GET System2 SUBSITE

    public JsonResult GetSubSites(string center,string system4, string year)
    {
        var subSites = new List<string>();

        if (string.IsNullOrEmpty(system4))
            return Json(subSites, JsonRequestBehavior.AllowGet);

        if (UseMumbai(center, year))
            subSites.AddRange(db.Report_2021.Where(x => x.System4 == system4 && x.System2 != null && x.System2 != "")
                                .Select(x => x.System2).Distinct().ToList());

        if (UseVizag(center, year))
            subSites.AddRange(db.Report_2022.Where(x => x.System4 == system4 && x.System2 != null && x.System2 != "")
                                .Select(x => x.System2).Distinct().ToList());

        return Json(subSites.Distinct().OrderBy(x => x).ToList(), JsonRequestBehavior.AllowGet);
    }


    //Get AGE wise data
    public ActionResult GetAgewise(string center, string year)
    {
        var ages = new List<string>();

        if (UseMumbai(center, year))
            ages.AddRange(db.Report_2021.Where(x => x.BROAD_AGE != null && x.BROAD_AGE != "")
                            .Select(x => x.BROAD_AGE).Distinct().ToList());

        if (UseVizag(center, year))
            ages.AddRange(db.Report_2022.Where(x => x.BROAD_AGE != null && x.BROAD_AGE != "")
                            .Select(x => x.BROAD_AGE).Distinct().ToList());

        return Json(ages.Distinct().OrderBy(x => x).ToList(), JsonRequestBehavior.AllowGet);
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

    public JsonResult GetSiteDashboardData( string center, string system4,string System2, List<string> ages, string year)
    {
        try
        {
            if (string.IsNullOrEmpty(center)) center = "registry";
            if (system4 == null) system4 = "";
            if (System2 == null) System2 = "";
            if (year == null) year = "";

            bool useMumbai = UseMumbai(center, year);
            bool useVizag = UseVizag(center, year);

            // this center has no report in the selected year (or center has no table at all)
            if (!useMumbai && !useVizag)
                return Json(EmptyDashboard(), JsonRequestBehavior.AllowGet);

            var mumbaiQuery = db.Report_2021.AsQueryable();
            var vizagQuery = db.Report_2022.AsQueryable();

            if (ages != null && ages.Count > 0)
            {
                mumbaiQuery = mumbaiQuery.Where(x => ages.Contains(x.BROAD_AGE));
                vizagQuery = vizagQuery.Where(x => ages.Contains(x.BROAD_AGE));
            }

            if (!string.IsNullOrEmpty(system4))
            {
                mumbaiQuery = mumbaiQuery.Where(x => x.System4 == system4);
                vizagQuery = vizagQuery.Where(x => x.System4 == system4);
            }

            if (!string.IsNullOrEmpty(System2))
            {
                mumbaiQuery = mumbaiQuery.Where(x => x.System2 == System2);
                vizagQuery = vizagQuery.Where(x => x.System2 == System2);
            }

            if (center == "mumbai") return Json(BuildMumbaiDashboard(mumbaiQuery), JsonRequestBehavior.AllowGet);
            if (center == "vizag") return Json(BuildVizagDashboard(vizagQuery), JsonRequestBehavior.AllowGet);

            // REGISTRY: only the centers that have a report in this year are counted
            if (!useMumbai) mumbaiQuery = mumbaiQuery.Where(x => false);
            if (!useVizag) vizagQuery = vizagQuery.Where(x => false);

            return Json(BuildRegistryDashboard(mumbaiQuery, vizagQuery), JsonRequestBehavior.AllowGet);
        }
        catch (Exception ex)
        {
            return Json(new
            {
                total = 0,
                male = 0,
                female = 0,
                clinicalData = new List<object>(),
                ageData = new List<object>(),
                treatmentData = new List<object>(),
                ttiData = new List<object>(),
                error = ex.Message
            }, JsonRequestBehavior.AllowGet);
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
        var total = query.Count();
        //var ttiTotal = query.Count(x => x.final_tti != null);
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

    private static string NormalizeState(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        var sb = new System.Text.StringBuilder();
        foreach (char ch in s.ToUpper())
            if (ch >= 'A' && ch <= 'Z') sb.Append(ch);
        return sb.ToString();
    }

    private static readonly Dictionary<string, string> StateNameToId = BuildStateMap();

    private static Dictionary<string, string> BuildStateMap()
    {
        // alias (any spelling you may have in the data) -> map id
        var raw = new Dictionary<string, string>
    {
        {"ANDAMAN AND NICOBAR", "INAN"}, {"ANDAMAN & NICOBAR ISLANDS", "INAN"}, {"ANDAMAN NICOBAR", "INAN"},
        {"ANDHRA PRADESH", "INAP"}, {"ARUNACHAL PRADESH", "INAR"}, {"ARUNACHAL", "INAR"}, {"ASSAM", "INAS"},
        {"BIHAR", "INBR"}, {"CHANDIGARH", "INCH"},
        {"CHHATTISGARH", "INCT"}, {"CHATTISGARH", "INCT"}, {"CHHATISGARH", "INCT"}, {"CHATISGARH", "INCT"}, {"CHHATTISGADH", "INCT"},
        {"DADRA AND NAGAR HAVELI", "INDH"}, {"DADRA & NAGAR HAVELI", "INDH"}, {"DAMAN AND DIU", "INDH"}, {"DAMAN & DIU", "INDH"},
        {"DADRA AND NAGAR HAVELI AND DAMAN AND DIU", "INDH"},
        {"DELHI", "INDL"}, {"NEW DELHI", "INDL"}, {"NCT OF DELHI", "INDL"},
        {"GOA", "INGA"}, {"GUJARAT", "INGJ"}, {"GUJRAT", "INGJ"}, {"HARYANA", "INHR"}, {"HIMACHAL PRADESH", "INHP"}, {"JHARKHAND", "INJH"},
        {"KARNATAKA", "INKA"}, {"KERALA", "INKL"},
        {"MADHYA PRADESH", "INMP"}, {"MADHYAPRADESH", "INMP"}, {"M P", "INMP"}, {"MP", "INMP"},
        {"MAHARASHTRA", "INMH"}, {"MUMBAI", "INMH_Mumbai"}, {"MANIPUR", "INMN"},
        {"MEGALAYA", "INML"}, {"MEGHALAYA", "INML"}, {"MIZORAM", "INMZ"}, {"NAGALAND", "INNL"},
        {"ODISHA", "INOR"}, {"ORISSA", "INOR"}, {"PONDICHERRY", "INPY"}, {"PUDUCHERRY", "INPY"},
        {"PUNJAB", "INPB"}, {"RAJASTHAN", "INRJ"}, {"SIKKIM", "INSK"}, {"TAMIL NADU", "INTN"}, {"TAMILNADU", "INTN"},
        {"TELANGANA", "INTG"}, {"TELENGANA", "INTG"}, {"TRIPURA", "INTR"}, {"UTTAR PRADESH", "INUP"},
        {"UTTARAKHAND", "INUT"}, {"UTTARANCHAL", "INUT"}, {"WEST BENGAL", "INWB"},
        {"LAKSHADWEEP", "INLD"}, {"JAMMU & KASHMIR", "INJK"}, {"JAMMU AND KASHMIR", "INJK"}, {"JAMMU KASHMIR", "INJK"}, {"J&K", "INJK"},
        {"LADAKH", "INLA"}
    };

        var map = new Dictionary<string, string>();
        foreach (var kv in raw) map[NormalizeState(kv.Key)] = kv.Value;
        return map;
    }

    // Builds { mapId -> patientCount } for the selected filters
    private Dictionary<string, int> BuildStateCountsFromDb(string center, string system4, string System2,
                                                     List<string> ages, string year, out int foreign, out Dictionary<string, int> unmapped)
    {
        center = string.IsNullOrEmpty(center) ? "registry" : center.ToLower();
        system4 = system4 ?? "";
        System2 = System2 ?? "";

        var rows = new List<KeyValuePair<string, int>>();

        // MUMBAI (Report_2021)  -> used for "mumbai" and "registry"
        if (UseMumbai(center, year))
        {
            var q = db.Report_2021.AsQueryable();

            if (ages != null && ages.Count > 0) q = q.Where(x => ages.Contains(x.BROAD_AGE));
            if (system4 != "") q = q.Where(x => x.System4 == system4);
            if (System2 != "") q = q.Where(x => x.System2 == System2);

            rows.AddRange(
                q.Where(x => x.STATE != null)
                 .GroupBy(x => x.STATE)
                 .Select(g => new { State = g.Key, Count = g.Count() })
                 .ToList()
                 .Select(a => new KeyValuePair<string, int>(a.State, a.Count)));
        }

        // VIZAG (Report_2022)   -> used for "vizag" and "registry"
        if (UseVizag(center, year))
        {
            var q = db.Report_2022.AsQueryable();

            if (ages != null && ages.Count > 0) q = q.Where(x => ages.Contains(x.BROAD_AGE));
            if (system4 != "") q = q.Where(x => x.System4 == system4);
            if (System2 != "") q = q.Where(x => x.System2 == System2);

            rows.AddRange(
                q.Where(x => x.STATE != null)
                 .GroupBy(x => x.STATE)
                 .Select(g => new { State = g.Key, Count = g.Count() })
                 .ToList()
                 .Select(a => new KeyValuePair<string, int>(a.State, a.Count)));
        }

        // every state starts at 0 so none is missing on the map
        var result = StateNameToId.Values.Distinct().ToDictionary(id => id, id => 0);
        foreign = 0;
        unmapped = new Dictionary<string, int>();

        foreach (var r in rows)
        {
            var name = NormalizeState(r.Key);
            string id;

            if (StateNameToId.TryGetValue(name, out id))
                result[id] += r.Value;       // += so Mumbai+Maharashtra / Daman+Dadra add together
            else if (name == "FOREIGN")
                foreign += r.Value;
            else
            {
                // state text in the database that matches no map state -> shown on the page so you can add it
                var shown = (r.Key ?? "").Trim();
                int old; unmapped.TryGetValue(shown, out old);
                unmapped[shown] = old + r.Value;
            }
        }

        return result;
    }


    // ---------- CACHE: same filters => no database hit for 10 minutes ----------
    private Dictionary<string, int> BuildStateCounts(string center, string system4, string System2,
                                                     List<string> ages, string year, out int foreign, out Dictionary<string, int> unmapped)
    {
        string key = "indiamap3|" + (year ?? "") + "|" + (center ?? "registry").ToLower() + "|" + (system4 ?? "") + "|" + (System2 ?? "") + "|" +
                     string.Join(",", (ages ?? new List<string>()).OrderBy(a => a));

        var hit = System.Web.HttpRuntime.Cache[key] as Tuple<Dictionary<string, int>, int, Dictionary<string, int>>;
        if (hit != null)
        {
            foreign = hit.Item2;
            unmapped = hit.Item3;
            return hit.Item1;
        }

        var result = BuildStateCountsFromDb(center, system4, System2, ages, year, out foreign, out unmapped);

        System.Web.HttpRuntime.Cache.Insert(key, Tuple.Create(result, foreign, unmapped), null,
            DateTime.UtcNow.AddMinutes(10), System.Web.Caching.Cache.NoSlidingExpiration);

        return result;
    }

    // Popup page:  /Home/Indiamap?center=mumbai&system4=Lung&System2=Bronchus&ages=0 - 19&ages=20 - 45
    public ActionResult Indiamap(string center, string system4, string System2, List<string> ages, string year)
    {
        int foreign;
        Dictionary<string, int> unmapped;
        var result = BuildStateCounts(center, system4, System2, ages, year, out foreign, out unmapped);

        string c = string.IsNullOrEmpty(center) ? "registry" : center.ToLower();

        ViewBag.Foreign = foreign;
        ViewBag.MumbaiCount = result.ContainsKey("INMH_Mumbai") ? result["INMH_Mumbai"] : 0;
        ViewBag.Total = (result.Values.Sum() + foreign + unmapped.Values.Sum()).ToString("N0");
        ViewBag.Unmapped = unmapped.Count == 0 ? "" :
            string.Join(", ", unmapped.OrderByDescending(u => u.Value).Select(u => (u.Key == "" ? "(blank)" : u.Key) + " = " + u.Value));

        ViewBag.YearLabel = string.IsNullOrEmpty(year) ? "All Years" : year;
        ViewBag.CenterLabel = c == "mumbai" ? "Mumbai" : c == "vizag" ? "Vizag" : "All Registry Centers";
        ViewBag.SiteLabel = string.IsNullOrEmpty(system4) ? "All Sites" : system4;
        ViewBag.SubSiteLabel = string.IsNullOrEmpty(System2) ? "All Subsites" : System2;
        ViewBag.AgeLabel = (ages == null || ages.Count == 0) ? "All Ages" : string.Join(", ", ages);

        return View(result);   // Dictionary<string,int>
    }

    // Same filters, JSON (kept in case you still call it from somewhere)
    public ActionResult GetStateCounts(string center, string system4, string System2, List<string> ages, string year)
    {
        int foreign;
        Dictionary<string, int> unmapped;
        var result = BuildStateCounts(center, system4, System2, ages, year, out foreign, out unmapped);
        return Json(result, JsonRequestBehavior.AllowGet);
    }

}