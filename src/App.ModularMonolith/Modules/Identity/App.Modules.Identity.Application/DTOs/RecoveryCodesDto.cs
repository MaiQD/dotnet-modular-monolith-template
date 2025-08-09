namespace App.Modules.Identity.Application.DTOs;

public class RecoveryCodesDto
{
    public IEnumerable<string> Codes { get; set; } = Array.Empty<string>();
}


