using System.ComponentModel.DataAnnotations;

namespace Planner.Api.DTOs;

public sealed class BookSlotRequest
{
    [Required, MinLength(2)]
    public string UserName { get; init; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;
}
