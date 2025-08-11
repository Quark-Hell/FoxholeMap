using System.ComponentModel.DataAnnotations;

namespace FoxholeMap.Models;

public class ChatModel
{
    [Key]
    public int ChatID { get; set; } = 0;
    public string ChatName { get; set; } = "Undefined";
    public string KeyBase64 { get; set; }
    public DateTime GeneratedAt { get; set; }

    public List<MessageModel> Messages { get; set; } = new List<MessageModel>();
}