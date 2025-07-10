namespace FoxholeMap.DataBase;

public class User
{
    public int ID { get; set; }
    public string Username { get; set; }
    public string PasswordHash  { get; set; }
    public int Rank { get; set; }
}