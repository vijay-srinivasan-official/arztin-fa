using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static arztin.Models.ArztinDataModels;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace arztin.DataDomain
{
	public class ArztinDbContext: DbContext
	{
        public DbSet<Users> Users { get; set; }
        public DbSet<Doctors> Doctors { get; set; }
        public DbSet<Appointments> Appointments { get; set; }
        public DbSet<Reviews> Reviews { get; set; }

        public ArztinDbContext(DbContextOptions<ArztinDbContext> options) : base(options)
        {

        }
    }
}

