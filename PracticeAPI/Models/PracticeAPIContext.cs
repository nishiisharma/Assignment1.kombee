using Microsoft.EntityFrameworkCore;
using static PracticeAPI.Models.User;

namespace PracticeAPI.Models
{
    public class PracticeAPIContext : DbContext
    {
      

        public PracticeAPIContext(DbContextOptions<PracticeAPIContext> options):base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Hobby> Hobbies { get; set; }
        public DbSet<FileUpload> FileUploads { get; set; }
    }
}
