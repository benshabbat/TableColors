using Microsoft.EntityFrameworkCore;

namespace ColorsTableNew.Models
{
    public class ColorDbContext:DbContext
    {
        public ColorDbContext(DbContextOptions<ColorDbContext> options):base(options)
        {
            
        }

       public DbSet<ColorModel> Colors { get; set; }
    }
}
