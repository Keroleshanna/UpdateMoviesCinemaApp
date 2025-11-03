using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesDashboard.Data;
using MoviesDashboard.Models;
using MoviesDashboard.Repositories.IRepositories;
using MoviesDashboard.ViewModels;
using System.Threading.Tasks;

namespace MoviesDashboard.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ActorController : Controller
    {
        private readonly IRepository<Actor> _actorRepo;

        public ActorController(IRepository<Actor> ActorRepo)
        {
            _actorRepo = ActorRepo;
        }
        // GET: ActorController
        public async Task<ActionResult> Index(int currentNumber = 1)
        {
            var actor = await _actorRepo.GetAllAsync();
            ViewData["CurrentNumber"] = currentNumber;
            ViewData["PageNumbers"] = Math.Ceiling(actor.Count() / 8.0);
            actor = actor.Skip((currentNumber - 1) * 8).Take(8);
            return View(actor.AsEnumerable());
        }



        // GET: ActorController/Create
        public ActionResult Create()
        {
            Actor actor = new();
            return View(actor);
        }

        // POST: ActorController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Actor actor, IFormFile img)
        {
            try
            {
                string fileName = null!;
                if (img is not null && img.Length > 0)
                {
                    fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//Images//Actors", fileName);
                    // حفظ الصورة في المسار
                    using var stream = System.IO.File.Create(path);
                    img.CopyTo(stream);
                }
                if (actor is not null)
                {
                    var areaName = RouteData.Values["area"]?.ToString() ?? "System";

                    actor.CreatedBy = areaName;
                    actor.LastModifiedBy = areaName;
                    actor.CreatedOn = DateTime.Now;
                    actor.LastModifiedOn = DateTime.Now;
                    actor.Img = fileName;

                    await _actorRepo.CreateAsync(actor);
                    await _actorRepo.CommitAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }




        // GET: ActorController/Edit/5
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var actor = await _actorRepo.GetOneAsync(a=> a.Id == id);
            return View(actor);
        }

        // POST: ActorController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Actor actor, IFormFile img)
        {
            if (actor is not null)
            {
                string fileName = "";
                if (img is not null && img.Length > 0)
                {
                    fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//Images//Actors", fileName);
                    using var stream = System.IO.File.Create(path);
                    img.CopyTo(stream);
                    actor.Img = fileName;
                }
                else
                {
                    var oldActor = await _actorRepo.GetOneAsync(a=> a.Id == actor.Id, tracked: false);
                    if (oldActor?.Img is not null)
                        fileName = oldActor.Img;
                }
                var areaName = RouteData.Values["area"]?.ToString() ?? "System";
                actor.LastModifiedOn = DateTime.Now;
                actor.LastModifiedBy = areaName;
                actor.Img = fileName;
                _actorRepo.Update(actor);
                await _actorRepo.CommitAsync();
            }
            return RedirectToAction(nameof(Index));

        }




        public async Task<IActionResult> Delete(int id)
        {
            var actor = await _actorRepo.GetOneAsync(a => a.Id == id);
            if (actor is not null)
            {
                _actorRepo.Delete(actor);
                await _actorRepo.CommitAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
