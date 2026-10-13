using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dashboard.Models
{
    [Table("TMH_2022")]
    public class Report2022
    {
        [Key]
        public string REGNO { get; set; }

        public string System4 { get; set; }
        public string System3 { get; set; }
        public string System2 { get; set; }
        public double AGE_AT_REG { get; set; }
        public string PRVTMTYN { get; set; }
        public string SEX { get; set; }
        public string N_CLINEXT { get; set; }
        public string TMH_RX { get; set; }
        public int? SIXTMT { get; set; } //TMH-RX-TYPE
        public string STATE { get; set; }
        public string BROAD_AGE { get; set; }
        public string final_tti { get; set; }
        public string Lead_site { get; set; }
        public string Sys { get; set; }

        public string gynae_system { get; set; }
        //  public string SIXTMT { get; set; }

        public string System1 { get; set; }
        public string NMETHOD { get; set; }
        public string REL_DET { get; set; } //religion property
        public string MOTHER_TONGUE { get; set; } //mother tongue
        public string EDU_DET { get; set; } //education
        public string TREATMENT_MUM_All { get; set; }
        public string PRVTMT { get; set; }
        public string AGE_GRP { get; set; }

        public string TREATMENT_Before { get; set; }
        public string INST { get; set; }
    }
}