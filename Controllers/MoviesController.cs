using System.Threading.Tasks;
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
    public async Task<IActionResult> MoviesIndex()
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
        var movies = await _context
            .Movies.AsNoTracking()
            .Include(m => m.User)
            .Include(m => m.Ratings) // added ratings
            // .ThenInclude(r => r.User) // added user for each rating
            .ToListAsync();

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
    public async Task<IActionResult> CreateNewMovie(MovieViewModel viewModel)
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
        await _context.SaveChangesAsync();
        return RedirectToAction("MovieDetails", new { id = movie.Id });
    }

    // --- INDIVIDUAL MOVIE VIEW --- //
    [HttpGet("{id}")]
    public async Task<IActionResult> MovieDetails(int id)
    {
        // PROTECTION
        if (HttpContext.Session.GetInt32(SessionUserId) is null)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }

        // METHOD
        var movie = await _context
            .Movies.AsNoTracking()
            .Include(m => m.User) // include user
            .Include(m => m.Ratings) // include ratings
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(m => m.Id == id);

        // if not found, return 404 error
        if (movie is null)
            return NotFound();

        // skipp mapping the entity to a viewModel and instead return view of movie
        return View(movie);
    }

    // ---- EDIT FORM GET ---- //
    [HttpGet("/{id}/edit")]
    public async Task<IActionResult> EditMovieForm(int id)
    {
        var currentUserId = HttpContext.Session.GetInt32(SessionUserId);
        // PROTECTION
        if (currentUserId is null)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }
        // METHOD
        var movie = await _context.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
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
    public async Task<IActionResult> UpdateMovie(int id, MovieViewModel viewModel)
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

        var movie = await _context.Movies.FindAsync(id);
        if (movie == null)
        {
            return NotFound();
        }
        // MAKE SURE USER IS WHO CREATED IT
        if (movie.UserId != currentUserId)
        {
            return Forbid();
        }

        // map the movie to the VIEWMODEL data

        movie.Title = viewModel.Title;
        movie.ReleaseDate = viewModel.ReleaseDate;
        movie.Genre = viewModel.Genre;
        movie.Description = viewModel.Description;
        movie.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return RedirectToAction("MovieDetails", new { id = movie.Id });
    }

    // ---- GET DELETE CONFIRMATION ACTION ---- //
    [HttpGet("/{id}/delete")]
    public async Task<IActionResult> ConfirmDelete(int id)
    {
        var currentUserId = HttpContext.Session.GetInt32(SessionUserId);
        // PROTECTION
        if (currentUserId is null)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }

        // METHOD
        var movie = await _context.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
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
    public async Task<IActionResult> DeleteMovie(int id, MovieViewModel vm)
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
        var movie = await _context.Movies.FindAsync(id);
        if (movie is null)
        {
            return NotFound();
        }

        // UPDATE DB
        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();

        // REDIRECT TO HOME (REQUIRED BY WIREFRAME)
        return RedirectToAction("MoviesIndex");
    }

    // --- HTTP POST ACTION FOR RATING --- //
    [HttpPost("{id}/rate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate(int id, int ratingValue)
    {
        var userId = HttpContext.Session.GetInt32(SessionUserId);
        if (userId is not int uid)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }

        // Check if the user has already liked this post
        var alreadyRated = await _context.Ratings.FirstOrDefaultAsync(r =>
            r.UserId == uid && r.MovieId == id
        );
        // if it's rated ...
        if (alreadyRated != null)
        {
            // set new ratingValue
            alreadyRated.RatingValue = ratingValue;
        }
        else
        {
            // set it to new class of Rating
            var newRate = new Rating
            {
                // map variables to variables
                UserId = uid,
                MovieId = id,
                RatingValue = ratingValue,
            };
            // add to ratings table
            _context.Ratings.Add(newRate);
        }

        // save changes
        await _context.SaveChangesAsync();

        // return to movieDetails page
        return RedirectToAction("MovieDetails", new { id });
    }
}

// IF YOU'RE READING THIS, YOU'RE DOING GREAT! :)
