using System.Text.Json;

using FoxholeMap.DataBase;
using FoxholeMap.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Security.Cryptography;
using System.Text;

namespace FoxholeMap.Controllers;

public class ChatController  : Controller
{
    private readonly ChatsDbContext _db;
    private readonly ILogger<ChatController> _logger;
    
    private readonly string key = "1234567890ABCDEF1234567890ABCDEF"; // 32 байта для AES-256
    private readonly string iv = "ABCDEF1234567890"; // 16 байт для AES

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
            .Include(c => c.Messages) // грузим сразу сообщения
            .FirstOrDefault(c => c.ChatID == chatID);

        return chat;
    }
    
    private string EncryptStringAES(string plainText, string key, string iv)
    {
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = Encoding.UTF8.GetBytes(iv);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(plainBytes, 0, plainBytes.Length);
                cs.FlushFinalBlock();

                byte[] encrypted = ms.ToArray();
                return Convert.ToBase64String(encrypted);
            }
        }
    }

    private string DecryptStringAES(string cipherText, string key, string iv)
    {
        byte[] buffer = Convert.FromBase64String(cipherText);

        Aes aes = Aes.Create();

        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = Encoding.UTF8.GetBytes(iv);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
        using (MemoryStream ms = new MemoryStream(buffer))
        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
        using (StreamReader sr = new StreamReader(cs))
        {
            return sr.ReadToEnd();
        }

    }

    private ChatModel DecryptChat(ref ChatModel chat)
    {
        foreach (var msg in chat.Messages)
        {
            msg.Message = DecryptStringAES(msg.Message, key, iv);
        }

        return chat;
    }
    
    private ChatModel EncryptChat(ref ChatModel chat)
    {
        foreach (var msg in chat.Messages)
        {
            msg.Message = EncryptStringAES(msg.Message, key, iv);
        }

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
            "<div class=\"message-avatar\">\n" +
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
        
        var htmlMessages = new StringBuilder();
        
        foreach (var msg in _currentChat.Messages)
        {
            bool isOwn = (msg.UserName == request.Username); 

            htmlMessages.Append(GenerateHtmlMessage(msg.Message, isOwn));
        }
        
        return Json(new { chatHtml = htmlMessages.ToString() });
    }

    [HttpPost]
    public JsonResult SendMessage(MessageModel message)
    {
        return Json(new ChatModel());
    }
}