using LanguageCourseManagement.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LanguageCourseManagement.MVC.Controllers;

/// <summary>
/// Genel MVC sayfalarını yöneten endpoint'ler.
/// </summary>
public class HomeController : Controller
{
    /// <summary>
    /// Ana sayfayı görüntüler.
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Gizlilik sayfasını görüntüler.
    /// </summary>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Hata sayfasını görüntüler.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [AllowAnonymous]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
