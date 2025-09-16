using Microsoft.EntityFrameworkCore;

namespace TheRewind.Models;

public class MovieContext : DbContext
{
    public DbSet<Movie> Movies { get; set; }

    public DbSet<User> Users { get; set; }

    // add DbSet for our like model
    // public DbSet<Rating> Ratings { get; set; }

    public MovieContext(DbContextOptions<MovieContext> options)
        : base(options) { }
}
