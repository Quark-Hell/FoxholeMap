using Microsoft.EntityFrameworkCore;
using FoxholeMap.Models;

namespace FoxholeMap.DataBase;

public class UsersDbContext : DbContext
{
    public DbSet<UserModel> Users { get; set; }

    public UsersDbContext(DbContextOptions<UsersDbContext> options)
        : base(options)
    {
    }
}