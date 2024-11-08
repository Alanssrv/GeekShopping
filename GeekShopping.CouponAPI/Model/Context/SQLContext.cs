using Microsoft.EntityFrameworkCore;

namespace GeekShopping.CouponAPI.Model.Context
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

        public DbSet<Coupon> Coupons { get; set; }
    }
}
