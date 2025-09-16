namespace TheRewind.ViewModels;

public class MoviesIndexViewModel
{
    public List<MovieRowViewModel> Movies { get; set; } = [];
    public int TotalCount => Movies.Count;
}
