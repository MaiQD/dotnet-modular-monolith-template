namespace App.Modules.Identity.Application.DTOs;

public class EnableAuthenticatorResponseDto
{
    public string SharedKey { get; set; } = string.Empty;
    public string AuthenticatorUri { get; set; } = string.Empty;
}


