using System.ComponentModel.DataAnnotations;

namespace TheRewind.ViewModels;

public class LoginFormViewModel
{
    [Required(ErrorMessage = "Please enter email.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Required(ErrorMessage = "Please enter password.")]
    public string Password { get; set; } = string.Empty;

    public string? Error { get; set; }
}
