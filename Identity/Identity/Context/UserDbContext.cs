using Identity.Models;
using Microsoft.EntityFrameworkCore;
namespace Identity.Context;
public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions options) : base(options) { }
    public DbSet<User> Users { get; set; }
}


