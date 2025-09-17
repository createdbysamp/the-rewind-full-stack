using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;
using TheRewind.Models;
using TheRewind.ViewModels;

namespace TheRewind.Controllers;

public class MoviesController : Controller
{
    private readonly MovieContext _context;
    private const string SessionUserId = "userId";

    public MoviesController(MovieContext context)
    {
        _context = context;
    }

    // general routing
    [Route("/movies")]
    // ----- MOVIES index action method ----- //
    [HttpGet("/all")]
    public IActionResult MoviesIndex()
    {
        var userId = HttpContext.Session.GetInt32(SessionUserId);
        // UNPROTECTED HOME PAGE
        // PROTECTION ... added :(
        if (userId is not int uid)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }
        // get the current user ID for RatedByMe variable

        // Get movies from database
        var movies = _context
            .Movies.Include(m => m.User)
            .Include(m => m.Ratings) // added ratings
            // .ThenInclude(r => r.User) // added user for each rating
            .ToList();

        // map movie to NEW MovieViewModel
        var movieViewModels = movies
            .Select(movie => new MovieViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                Genre = movie.Genre,
                ReleaseDate = movie.ReleaseDate,
                Description = movie.Description,
                CreatedAt = movie.CreatedAt,
                UpdatedAt = movie.UpdatedAt,
                AuthorUsername = movie.User?.UserName ?? "Unknown",
                RatingCount = movie.Ratings.Count, // count ratings for this movie
                RatedByMe = movie.Ratings.Any(r => r.UserId == uid), // true if userId has rated
                AvgRating = movie.Ratings.Any()
                    ? Math.Round(movie.Ratings.Average(r => r.RatingValue), 1)
                    : 0,
            })
            .ToList();

        var vm = new MoviesIndexViewModel { AllMovies = movieViewModels };
        return View(vm);
    }

    // ----- NEW MOVIES ACTION ----- //
    [HttpGet("/new")]
    public IActionResult NewMovieForm()
    {
        // PROTECTION
        if (HttpContext.Session.GetInt32(SessionUserId) is null)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }
        // VIEW
        return View(new MovieViewModel());
    }

    // --- NEW MOVIE POST --- //
    [HttpPost("/create")]
    [ValidateAntiForgeryToken]
    public IActionResult CreateNewMovie(MovieViewModel viewModel)
    {
        var userId = HttpContext.Session.GetInt32(SessionUserId);
        // PROTECTION
        if (HttpContext.Session.GetInt32(SessionUserId) is null)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }
        // METHOD
        if (!ModelState.IsValid)
        {
            return View("NewMovieForm", viewModel);
        }

        var movie = new Movie
        {
            Title = viewModel.Title,
            ReleaseDate = viewModel.ReleaseDate,
            Genre = viewModel.Genre,
            Description = viewModel.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = (int)userId,
        };

        _context.Movies.Add(movie);
        _context.SaveChanges();
        return RedirectToAction("MovieDetails", new { id = movie.Id });
    }

    // --- INDIVIDUAL MOVIE VIEW --- //
    [HttpGet("{id}")]
    public IActionResult MovieDetails(int id)
    {
        // PROTECTION
        if (HttpContext.Session.GetInt32(SessionUserId) is null)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }

        // METHOD
        var movie = _context
            .Movies.Include(m => m.User) // include user
            .Include(m => m.Ratings) // include ratings
            .ThenInclude(r => r.User)
            .FirstOrDefault(m => m.Id == id);

        // if not found, return 404 error
        if (movie is null)
            return NotFound();

        // skipp mapping the entity to a viewModel and instead return view of movie
        return View(movie);
    }

    // ---- EDIT FORM GET ---- //
    [HttpGet("/{id}/edit")]
    public IActionResult EditMovieForm(int id)
    {
        var currentUserId = HttpContext.Session.GetInt32(SessionUserId);
        // PROTECTION
        if (currentUserId is null)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }
        // METHOD
        var movie = _context.Movies.FirstOrDefault(m => m.Id == id);
        if (movie is null)
            return NotFound();

        // MAKE SURE USER IS WHO CREATED IT
        if (movie.UserId != currentUserId)
        {
            return Forbid();
        }

        var vm = new MovieViewModel
        {
            Id = movie.Id,
            Title = movie.Title,
            Genre = movie.Genre,
            ReleaseDate = movie.ReleaseDate,
            Description = movie.Description,
        };
        return View(vm);
    }

    // --- EDIT FORM POST --- //
    [HttpPost()]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateMovie(int id, MovieViewModel viewModel)
    {
        var currentUserId = HttpContext.Session.GetInt32(SessionUserId);
        // PROTECTION
        if (currentUserId is null)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }
        // METHOD
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View("EditMovieForm", viewModel);
        }

        var movie = _context.Movies.Find(id);
        if (movie == null)
        {
            return NotFound();
        }
        // MAKE SURE USER IS WHO CREATED IT
        if (movie.UserId != currentUserId)
        {
            return Forbid();
        }

        movie.Title = viewModel.Title;
        movie.ReleaseDate = viewModel.ReleaseDate;
        movie.Genre = viewModel.Genre;
        movie.Description = viewModel.Description;
        movie.UpdatedAt = DateTime.Now;

        _context.SaveChanges();

        return RedirectToAction("MovieDetails", new { id = movie.Id });
    }

    // ---- GET DELETE CONFIRMATION ACTION ---- //
    [HttpGet("/{id}/delete")]
    public IActionResult ConfirmDelete(int id)
    {
        var currentUserId = HttpContext.Session.GetInt32(SessionUserId);
        // PROTECTION
        if (currentUserId is null)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }

        // METHOD
        var movie = _context.Movies.FirstOrDefault(m => m.Id == id);
        if (movie is null)
            return NotFound();

        // MUST BE USER-WHO-CREATED
        if (movie.UserId != currentUserId)
        {
            return Forbid();
        }

        // MAP TO NEW VIEWMODEL OF MOVIE
        var vm = new MovieViewModel
        {
            Id = movie.Id,
            Title = movie.Title,
            Genre = movie.Genre,
            ReleaseDate = movie.ReleaseDate,
            Description = movie.Description,
        };
        // RETURN VIEW
        return View("ConfirmDelete", vm);
    }

    // --- POST DELETE ACTION --- //
    [HttpPost("/{id}/delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteMovie(int id, MovieViewModel vm)
    {
        var currentUserId = HttpContext.Session.GetInt32(SessionUserId);
        // PROTECTION
        if (currentUserId is null)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }

        // IF MOVIE DOES NOT EXIST
        if (vm.Id is null || vm.Id.Value != id)
        {
            return BadRequest();
        }
        var movie = _context.Movies.Find(id);
        if (movie is null)
        {
            return NotFound();
        }

        // UPDATE DB
        _context.Movies.Remove(movie);
        _context.SaveChanges();

        // REDIRECT TO HOME (REQUIRED BY WIREFRAME)
        return RedirectToAction("MoviesIndex");
    }

    // --- HTTP POST ACTION FOR RATING --- //
    [HttpPost("{id}/rate")]
    [ValidateAntiForgeryToken]
    public IActionResult Rate(int id, int ratingValue)
    {
        var userId = HttpContext.Session.GetInt32(SessionUserId);
        if (userId is not int uid)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }

        // Check if the user has already liked this post
        var alreadyRated = _context.Ratings.FirstOrDefault(r => r.UserId == uid && r.MovieId == id);
        if (alreadyRated != null)
        {
            alreadyRated.RatingValue = ratingValue;
        }
        else
        {
            var newRate = new Rating
            {
                UserId = uid,
                MovieId = id,
                RatingValue = ratingValue,
            };
            _context.Ratings.Add(newRate);
        }
        _context.SaveChanges();

        return RedirectToAction("MovieDetails", new { id });
    }
}

// IF YOU'RE READING THIS, YOU'RE DOING GREAT! :)
