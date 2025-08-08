using System.ComponentModel.DataAnnotations;

namespace FoxholeMap.Models;

public class ChatModel
{
    [Key]
    public int ChatID { get; set; } = 0;
    public string ChatName { get; set; } = "Undefined";

    public List<MessageModel> Messages { get; set; } = new List<MessageModel>();
}