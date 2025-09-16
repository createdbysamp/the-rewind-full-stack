using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
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
        // UNPROTECTED HOME PAGE
        // PROTECTION ... added :(
        if (HttpContext.Session.GetInt32(SessionUserId) is null)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }

        // METHOD
        var vm = new MoviesIndexViewModel
        {
            Movies = _context
                .Movies.Select(m => new MovieRowViewModel { Id = m.Id, Title = m.Title })
                .ToList(),
        };
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
        };

        _context.Movies.Add(movie);
        _context.SaveChanges();
        return RedirectToAction("MoviesIndex");
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
        var movie = _context.Movies.FirstOrDefault(m => m.Id == id);

        // if not found, return 404 error
        if (movie is null)
            return NotFound();

        // map the entity to a viewModel

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

    // ---- EDIT FORM GET ---- //
    [HttpGet("/{id}/edit")]
    public IActionResult EditMovieForm(int id)
    {
        // PROTECTION
        if (HttpContext.Session.GetInt32(SessionUserId) is null)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }
        // METHOD
        var movie = _context.Movies.FirstOrDefault(m => m.Id == id);
        if (movie is null)
            return NotFound();

        // MAKE SURE USER IS WHO CREATED IT
        // if (movie.UserId != SessionUserId)
        // {
        //     return Forbid();
        // }

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
        // PROTECTION
        if (HttpContext.Session.GetInt32(SessionUserId) is null)
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
        // PROTECTION
        if (HttpContext.Session.GetInt32(SessionUserId) is null)
        {
            return RedirectToAction("LoginForm", "Account", new { error = "not-authenticated" });
        }

        // METHOD
        var movie = _context.Movies.FirstOrDefault(m => m.Id == id);
        if (movie is null)
            return NotFound();

        // MUST BE USER-WHO-CREATED
        // if (movie.UserId != SessionUserId)
        // {
        //     return Forbid();
        // }

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
        // PROTECTION
        if (HttpContext.Session.GetInt32(SessionUserId) is null)
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
}

// IF YOU'RE READING THIS, YOU'RE DOING GREAT! :)
