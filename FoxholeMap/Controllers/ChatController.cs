using Quartz;

using FoxholeMap.DataBase;
using FoxholeMap.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using System.Security.Cryptography;
using System.Text;

namespace FoxholeMap.Controllers;

public class ChatReEncryptionJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ChatReEncryptionJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await ReEncryptAllChats();
    }

    private async Task ReEncryptAllChats()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatsDbContext>();

        var chats = db.Chats
            .Include(c => c.Messages)
            .ToList();

        foreach (var chat in chats)
        {
            if (chat.GeneratedAt.Date == DateTime.UtcNow.Date)
                continue;
            
            var oldKeyBase64 = chat.KeyBase64;
            var oldKey = Convert.FromBase64String(oldKeyBase64);
            
            using var rng = RandomNumberGenerator.Create();
            byte[] newKeyBytes = new byte[32];
            rng.GetBytes(newKeyBytes);
            string newKeyBase64 = Convert.ToBase64String(newKeyBytes);

            chat.KeyBase64 = newKeyBase64;
            chat.GeneratedAt = DateTime.UtcNow;
            
            foreach (var msg in chat.Messages)
            {
                var decrypted = DecryptMessage(msg.Message, oldKey);
                msg.Message = EncryptMessage(decrypted, newKeyBytes);
            }
        }

        await db.SaveChangesAsync();
    }
    
    private string EncryptMessage(string text, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(text);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String(aes.IV) + ":" + Convert.ToBase64String(cipherBytes);
    }

    private string DecryptMessage(string cipherText, byte[] key)
    {
        var parts = cipherText.Split(':');
        if (parts.Length != 2)
            throw new FormatException("Invalid encrypted data format");

        var iv = Convert.FromBase64String(parts[0]);
        var cipherBytes = Convert.FromBase64String(parts[1]);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

}


public class ChatController  : Controller
{
    private readonly ChatsDbContext _db;
    private readonly ILogger<ChatController> _logger;

    private int _currentChatID = -1;
    private ChatModel _currentChat;

    public ChatController(ILogger<ChatController> logger, ChatsDbContext db)
    {
        _db = db;
        _logger = logger;   
    }

    public IActionResult Index()
    {
        return View();
    }
    
    private ChatModel GetChatByID(int chatID)
    {
        var db = new ChatsDbContext(
            new DbContextOptionsBuilder<ChatsDbContext>()
            .UseSqlite("Data Source=DataBase/Chats.db")
            .Options);
        
        var chat = db.Chats
            .Include(c => c.Messages)
            .FirstOrDefault(c => c.ChatID == chatID);

        return chat;
    }

    private string GenerateHtmlMessage(string content, bool isOwn)
    {
        string message = "";
        
        if (isOwn)
        {
            message = "<div class=\"message own\">\n";
        }
        else
        {
            message = "<div class=\"message\">\n";
        }
        
        message += 
            "<div class=\"message-avatar non-selectable\">\n" +
                "<img src=\"/MapAssets/UI_Icons/bin.png\" alt=\"Вы\">\n" +
            "</div>\n" +
            "<div class=\"message-content\">\n" +
                content +
            "</div>\n" +
            "</div>";
        
        return message; 
    }

    [HttpPost]
    public JsonResult ReadChat([FromBody] ChatRequest request)
    {
        _currentChatID = request.ChatID;
        _currentChat = GetChatByID(request.ChatID);

        var messagesList = new List<object>();

        foreach (var msg in _currentChat.Messages)
        {
            bool isOwn = (msg.UserName == request.Username);
            messagesList.Add(new
            {
                _message = msg.Message,
                _isOwn = isOwn
            });
        }

        return Json(new { chat = messagesList });
    }

    [HttpPost]
    public JsonResult SendMessage([FromBody] MessageModel message)
    {
        // Находим чат
        var chat = _db.Chats
            .Include(c => c.Messages)
            .FirstOrDefault(c => c.ChatID == message.ChatID);

        if (chat == null)
        {
            return Json(new { success = false, error = "Чат не найден" });
        }
        
        message.MessageDate = DateTime.Now;
        message.Chat = null;
        
        bool isOwn = true; 
        var html = GenerateHtmlMessage(message.Message, isOwn);
        
        _db.Messages.Add(message);
        _db.SaveChanges();

        return Json(new { success = true, html });
    }
    
    [HttpPost]
    public JsonResult GetDailyKey([FromBody] ChatRequest chatRequest)
    {
        //TODO: Add Authorize check

        var chatKey = _db.Chats.FirstOrDefault(k => k.ChatID == chatRequest.ChatID);
        if (chatKey == null)
        {
            return Json(new { success = false, error = "Чат не найден" });
        }
        
        return Json(new {chatKey = chatKey.KeyBase64 });
    }
}