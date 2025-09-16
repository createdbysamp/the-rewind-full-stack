using TheRewind.Models;

namespace TheRewind.ViewModels;

public class MoviesIndexViewModel
{
    public List<Movie> AllMovies { get; set; } = [];
    public int TotalCount => AllMovies.Count;
}
