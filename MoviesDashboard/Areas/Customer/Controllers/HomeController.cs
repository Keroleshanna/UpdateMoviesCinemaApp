using Microsoft.AspNetCore.Mvc;
using MoviesDashboard.Models;
using MoviesDashboard.Repositories.IRepositories;
using System.Diagnostics;

namespace ECommerce518.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IRepository<Movie> _movieRepository;

        public HomeController(IRepository<Movie> repository)
        {
            _movieRepository = repository;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var movies = await _movieRepository.GetAllAsync(tracked: false);
            movies = movies.Skip((page - 1) * 8).Take(8);
            return View(movies);
        }

        public IActionResult Privacy()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
