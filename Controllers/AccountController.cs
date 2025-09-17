using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TheRewind.Models;
using TheRewind.Services;
using TheRewind.ViewModels;

namespace TheRewind.Controllers;

[Route("account/")]
public class AccountController : Controller
{
    private readonly MovieContext _context;
    private readonly IPasswordService _passwords;

    private const string SessionUserId = "userId";

    public AccountController(MovieContext context, IPasswordService passwords)
    {
        _context = context;
        _passwords = passwords;
    }

    // ---- REGISTER GET ACTION ---- //
    [HttpGet("register")]
    public IActionResult RegisterForm()
    {
        return View(new RegisterFormViewModel());
    }

    // --- REGISTER POST ACTION --- //
    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessRegisterForm(RegisterFormViewModel vm)
    {
        // 1. Normalize so validation runs on clean values
        vm.UserName = (vm.UserName ?? "").Trim().ToLowerInvariant();
        vm.Email = (vm.Email ?? "").Trim().ToLowerInvariant();
        vm.Password = (vm.Password ?? "").Trim();
        vm.ConfirmPassword = (vm.ConfirmPassword ?? "").Trim();

        // 2. Is the model valid according to annotations?
        if (!ModelState.IsValid)
        {
            // Validation failed: re-render to show field errors.
            return View("RegisterForm", vm);
        }

        // 3. Is the username unique?
        bool userAlreadyExists = await _context.Users.AnyAsync(
            (user) => user.UserName == vm.UserName
        );
        if (userAlreadyExists)
        {
            // Manually add a model error that will display in the view.
            ModelState.AddModelError("UserName", "That username is already in use.");
            // Re-render to show field error.
            return View("RegisterForm", vm);
        }
        // 3. Is the email unique?
        bool emailAlreadyExists = await _context.Users.AnyAsync((user) => user.Email == vm.Email);
        if (emailAlreadyExists)
        {
            // Manually add a model error that will display in the view.
            ModelState.AddModelError("Email", "That email is already in use.");
            // Re-render to show field error.
            return View("RegisterForm", vm);
        }

        // 4. Hash the password with BCrypt before storing.
        var hashedPassword = _passwords.Hash(vm.Password);

        // 5. Create and save the new User record -- USING USER MODEL --
        var newUser = new User
        {
            Email = vm.Email,
            PasswordHash = hashedPassword,
            UserName = vm.UserName,
        };

        // UPDATE DB
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        // 6. Store the user’s ID in session (acts as login).
        HttpContext.Session.SetInt32(SessionUserId, newUser.Id);

        // 7. Redirect to the home page (or dashboard).
        return RedirectToAction("MoviesIndex", "Movies");
    }

    // --- LOGIN GET ACTION --- //
    [HttpGet("login")]
    public IActionResult LoginForm(string? error)
    {
        // SUMMON LOGIN FORM VIEWMODEL
        var loginFormViewModel = new LoginFormViewModel { Error = error };
        return View(loginFormViewModel);
    }

    // --- LOGIN POST ACTION --- //
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessLoginForm(LoginFormViewModel vm)
    {
        // 1. Normalize the email and password for validation
        vm.Email = (vm.Email ?? "").Trim().ToLowerInvariant();
        vm.Password = (vm.Password ?? "").Trim();

        // 2. Check if the model is valid according to annotations.
        if (!ModelState.IsValid)
        {
            // Validation failed: re-render to show field errors.
            return View("LoginForm", vm);
        }

        // 3. Find the user in the database
        // Hint: Use LINQ's SingleOrDefault() to find a user by their email.
        var userExists = await _context.Users.SingleOrDefaultAsync(
            (user) => user.Email == vm.Email
        );
        // The return value will be null if no user is found.
        if (userExists is null)
            return RedirectToAction("LoginForm", new { error = "invalid-credentials" });

        // 4. Check if the user exists AND if the password is correct
        if (userExists is null || !_passwords.Verify(vm.Password, userExists.PasswordHash))
        // Hint: If the credentials are bad, redirect back to the login form
        {
            return RedirectToAction(
                "LoginForm",
                // with a specific error message via a query string. This is part of the PRG pattern.
                new { error = "invalid-credentials" }
            );
        }

        // 5. If successful, store the user's ID in session
        HttpContext.Session.SetInt32(SessionUserId, userExists.Id);

        // 6. Redirect to the home page
        return RedirectToAction("MoviesIndex", "Movies");
    }

    // --- HTTP GET for LOGOUT CONFIRMATION --- //

    [HttpGet("logout")]
    public IActionResult LogoutConfirm()
    {
        return View();
    }

    // --- HTTP POST for LOGOUT --- //
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        // LOGOUT ACTIONS
        HttpContext.Session.Clear(); // clears all session data for the current user
        return RedirectToAction("Index", "Home", new { message = "logout-successful" });
    }

    // --- PROTECTED PROFILE --- //
    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        // PROTECTION
        if (HttpContext.Session.GetInt32(SessionUserId) is null)
        {
            return RedirectToAction("LoginForm", new { error = "not-authenticated" });
        }

        int userId = (int)HttpContext.Session.GetInt32(SessionUserId);
        // use userId to get their email
        var user = await _context
            .Users.AsNoTracking()
            // .Include(u => u.Movies)
            // .Include(u => u.Ratings)
            .SingleOrDefaultAsync(u => u.Id == userId);

        // pass the user's email to the view
        ViewBag.UserEmail = user.Email;
        ViewBag.UserName = user.UserName;

        // count movies added by this user
        // count movies rated by this user
        ViewBag.MoviesAdded = await _context.Movies.CountAsync(m => m.UserId == userId);
        ViewBag.MoviesRated = await _context.Movies.CountAsync(r => r.UserId == userId);

        return View();
    }
}

// IF YOU'RE READING THIS, THEN ... YOU'RE COOL.
