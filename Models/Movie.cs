using System.ComponentModel.DataAnnotations;

namespace TheRewind.Models;

public class Movie
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MinLength(2, ErrorMessage = "Title must be more than 2 characters...")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MinLength(2, ErrorMessage = "Genre must be at least 2 char")]
    public string Genre { get; set; } = string.Empty;

    [Required]
    public DateOnly ReleaseDate { get; set; }

    [Required]
    [MinLength(10, ErrorMessage = "description must be at least 10 char")]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt = DateTime.UtcNow;
    public DateTime UpdatedAt = DateTime.UtcNow;

    // --- ONE TO MANY RELATIONSHIP PROPERTIES --- //

    // FOREIGN KEY FOR THE USER WHO CREATED POST
    // navigation property for the user
    public int UserId { get; set; }
    public User? User { get; set; }

    // many-to-many relationship with user through the rating join table
    public List<Rating> Ratings { get; set; } = [];
}
