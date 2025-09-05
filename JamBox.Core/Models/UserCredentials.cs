namespace JamBox.Core.Models;

public class UserCredentials
{
    public string ServerUrl { get; set; }
    public string Username { get; set; }
    public string Password { get; set; } // For production, encrypt this!
}