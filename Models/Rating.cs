using System.ComponentModel.DataAnnotations;

namespace TheRewind.Models;

public class Rating
{
    [Key]
    public int Id { get; set; }

    // foreign key for user who liked post
    public int UserId { get; set; }

    // foreign key for the movie that was liked
    public int MovieId { get; set; }

    // navigation property for user
    public User? User { get; set; }

    // navigation property for album
    public Movie? Movie { get; set; }

    [Range(1, 5, ErrorMessage = "Rating must be between 1 & 5.")]
    public int RatingValue { get; set; }
}
