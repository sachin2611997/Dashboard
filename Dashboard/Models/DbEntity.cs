using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Dashboard.Models
{
    public class DbEntity:DbContext
    {
        public DbEntity() : base("DbEntity")
        {

        }
        public DbSet<Report2021> Report_2021 { get; set; }
        public DbSet<Report2022> Report_2022 { get; set; }
        
    }
}