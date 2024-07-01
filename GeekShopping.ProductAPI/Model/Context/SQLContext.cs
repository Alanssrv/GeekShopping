using GeekShopping.ProductAPI.Model.Base;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.ProductAPI.Model.Context
{
    public class SQLContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public SQLContext(IConfiguration configuration) {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Configuration.GetConnectionString("DatabaseConnection"));
        }

        public DbSet<Product> Products { get; set; }
    }
}
