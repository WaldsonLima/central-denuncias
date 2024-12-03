using CentralDenuncias.Models;
using Microsoft.EntityFrameworkCore;

namespace CentralDenuncias.Context
{
    public class Database : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("User ID=postgres;Password=06SxkU2BDTsh9GqL;Host=bashfully-pioneering-ox.data-1.use1.tembo.io;Port=5432;Database=app;Pooling=true;");
        }
        public DbSet<denuncia> denuncia { get; set; }
    }
}
