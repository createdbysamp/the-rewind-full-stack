using Microsoft.AspNetCore.Mvc;

public class ErrorController : Controller
{
    // [Route("Error/404")]
    // public IActionResult NotFound() => View();
    // public IActionResult NotFound() => View();

    [HttpGet("error/{code}")]
    public IActionResult Handle(int code)
    {
        if (code == 404)
        {
            // Serve a custom view for 404 errors
            return View("PageNotFound");
        }

        // Optional: handle other codes
        return View("ServerError");
    }

    [HttpGet("error/boom")]
    public IActionResult Boom()
    {
        // This is a test method that will intentionally throw a 500 error.
        // It's a useful way to test our custom error page without introducing a bug.
        return new StatusCodeResult(500);
    }
}
