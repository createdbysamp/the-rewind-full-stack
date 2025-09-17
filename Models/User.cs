using System.ComponentModel.DataAnnotations;

namespace TheRewind.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ---- one to many relationship properties ---- //
    public List<Movie> Movies { get; set; } = [];

    // --- many to many relationship properties --- //
    // many-to-many relationship with album through the rating table
    public List<Rating> Ratings { get; set; } = [];
}
