using TheRewind.Models;

namespace TheRewind.ViewModels;

public class MoviesIndexViewModel
{
    public List<MovieViewModel> AllMovies { get; set; } = [];
    public int TotalCount => AllMovies.Count;
}
