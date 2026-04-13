using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Woldon.Controllers;

/// <summary>
/// Controller responsible for serving the main application UI.
/// Secured with [Authorize] to prevent unauthorized access to the Dashboard.
/// </summary>
[Authorize]
public class HomeController : Controller
{
    // ==========================================
    // ROUTING & VIEW DELIVERY
    // ==========================================

    /// <summary>
    /// Serves the Dashboard view. 
    /// Maps both the root URL and the legacy index.html path.
    /// </summary>
    [Route("/")]
    [Route("/index.html")]
    public IActionResult Index()
    {
        // Explicitly returning the Dashboard view from the protected Views folder.
        // This ensures the file is only accessible via this authorized controller.
        return View("~/Views/Dashboard.cshtml");
    }
}