using Microsoft.AspNetCore.Mvc;
using MoviesDashboard.Models;
using MoviesDashboard.Repositories.IRepositories;

namespace MoviesDashboard.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CinemaController : Controller
    {
        private readonly IRepository<Cinema> _Repository;

        public CinemaController(IRepository<Cinema> repository)
        {
            _Repository = repository;
        }

        public async Task<ActionResult> Index(int currentNumber = 1)
        {
            var cinemas = await _Repository.GetAllAsync(tracker: false);
            ViewData["CurrentNumber"] = currentNumber;
            ViewData["pageNumbers"] = Math.Ceiling(cinemas.Count() / 8.0);
            cinemas = cinemas.Skip((currentNumber - 1) * 8).Take(8);

            return View(cinemas);
        }

        public ActionResult Create()
        {
            Cinema cinema = new();
            return View(cinema);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Cinema cinema, IFormFile image)
        {
            try
            {
                string fileName;
                if (image is not null)
                {
                    fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//Images//Cinemas", fileName);
                    using var stream = System.IO.File.Create(path);
                    image.CopyTo(stream);
                    cinema.Img = fileName;
                }
                var areaName = RouteData.Values["area"]?.ToString() ?? "System";

                cinema.CreatedOn = DateTime.Now;
                cinema.CreatedBy = areaName;
                cinema.LastModifiedBy = areaName;
                cinema.LastModifiedOn = DateTime.Now;

                await _Repository.CreateAsync(cinema);
                await _Repository.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Edit(int id)
        {
            var cinema = _Repository.GetOneAsync(a=>a.Id == id);
            return View(cinema);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Cinema cinema, IFormFile image)
        {
            try
            {
                string fileName;
                var oldCinema = await _Repository.GetOneAsync(c => c.Id == cinema.Id, tracker: false);
                if (image is not null)
                {
                    fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//Images//Cinemas", fileName);
                    using var stream = System.IO.File.Create(path);
                    image.CopyTo(stream);
                    cinema.Img = fileName;
                }
                else
                    cinema.Img = oldCinema?.Img;

                if (cinema is not null && oldCinema is not null)
                {
                    var areaName = RouteData.Values["area"]?.ToString() ?? "System";
                    cinema.CreatedBy = oldCinema.CreatedBy;
                    cinema.CreatedOn = oldCinema.CreatedOn;
                    cinema.LastModifiedBy = areaName;
                    cinema.LastModifiedOn = DateTime.Now;

                    _Repository.Update(cinema);
                    await _Repository.CommitAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public async Task<ActionResult> Delete(int id)
        {
            var cinema = await _Repository.GetOneAsync(a=> a.Id == id);
            if (cinema is not null)
            {
                _Repository.Delete(cinema);
                await _Repository.CommitAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
