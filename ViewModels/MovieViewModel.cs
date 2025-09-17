using System.ComponentModel.DataAnnotations;

namespace TheRewind.ViewModels;

public class MovieViewModel
{
    // null on create; populated on edit

    public int? Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [MinLength(2, ErrorMessage = "Title must be at least two characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Genre is required")]
    [MinLength(2, ErrorMessage = "Genre must be at least two characters.")]
    public string Genre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Release Date is required")]
    public DateOnly ReleaseDate { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [MinLength(10, ErrorMessage = "Description must be at least ten characters.")]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // tie in for user - public authorization
    public string AuthorUsername { get; set; } = string.Empty;

    // tie in for ratings
    public int RatingCount { get; set; }
    public bool RatedByMe { get; set; }
    public double AvgRating { get; set; }

    // tie in for userId
    public int UserId { get; set; }
}
