using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoxholeMap.Models;

public class MessageModel
{
    [Key]
    public int MessageID { get; set; }
    public int ChatID { get; set; } = -1;
    
    [ForeignKey("ChatID")]
    public ChatModel Chat { get; set; }
    
    public string UserName { get; set; } = "Undefined";
    public string Message { get; set; } = "Undefined";
    public DateTime MessageDate { get; set; } = DateTime.Now;
}