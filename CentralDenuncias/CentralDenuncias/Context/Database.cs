using CentralDenuncias.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CentralDenuncias.Context
{
    public class Database : DbContext
    {
        private readonly IConfiguration _configuration;
        public Database(DbContextOptions<Database> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("Default"));
        }
        public DbSet<denuncia> denuncia { get; set; }
        public DbSet<ip> ip { get; set;  }
    }
}
