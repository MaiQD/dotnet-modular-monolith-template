namespace App.Modules.Identity.Application.DTOs;

public class TokenPairDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}


