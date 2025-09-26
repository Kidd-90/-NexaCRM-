using System.ComponentModel.DataAnnotations;
using NexaCRM.Services.Admin.Validation;

namespace NexaCRM.Services.Admin.Models;

/// <summary>
/// Model used for registering a new user.
/// </summary>
public class NewUser
{
    [Required]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "User ID")]
    [StringLength(20, MinimumLength = 6, ErrorMessage = "User ID must be between 6 and 20 characters.")]
    [RegularExpression(
        "^(?=.*[A-Za-z])(?=.*[@._-])[A-Za-z0-9@._-]+$",
        ErrorMessage = "User ID must contain letters and at least one of the following special characters: . _ @ -.")]
    [DisallowSequentialCharacters(3, ErrorMessage = "User ID cannot contain repeated or sequential character runs of length 3 or more.")]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [StringLength(64, MinimumLength = 10, ErrorMessage = "Password must be between 10 and 64 characters.")]
    [RegularExpression(
        "^(?=.*[A-Z])(?=.*[a-z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{10,64}$",
        ErrorMessage = "Password must contain uppercase, lowercase, number, and special characters.")]
    [DisallowSequentialCharacters(3, ErrorMessage = "Password cannot contain repeated or sequential character runs of length 3 or more.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms.")]
    public bool TermsAccepted { get; set; }
}

