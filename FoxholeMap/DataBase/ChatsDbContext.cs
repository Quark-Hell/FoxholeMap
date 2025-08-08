using Microsoft.EntityFrameworkCore;
using FoxholeMap.Models;

namespace FoxholeMap.DataBase;

public class ChatsDbContext : DbContext
{
    public DbSet<ChatModel> Chats { get; set; }
    public DbSet<MessageModel> Messages { get; set; }

    public ChatsDbContext(DbContextOptions<ChatsDbContext> options)
        : base(options)
    {
    }
}