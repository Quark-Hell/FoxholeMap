using System.ComponentModel.DataAnnotations;

namespace FoxholeMap.Models;

public class ChatRequest
{
    [Key]
    public int ChatID { get; set; }
    public string Username { get; set; }
}