namespace JamBox.Core.Models;

public class UserCredentials
{
    public required string ServerUrl { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}