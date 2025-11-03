using Microsoft.AspNetCore.Http;
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
    public class CategoryController : Controller
    {
        private readonly IRepository<Category> _Repository;

        public CategoryController(IRepository<Category> repository)
        {
            _Repository = repository;
        }



        // GET: CategoryController
        public async Task<ActionResult> Index(int currentNumber = 1)
        {
            var categories = await _Repository.GetAllAsync(tracked: false);
            ViewData["PageNumbers"] = Math.Ceiling(categories.Count() / 8.0);
            ViewData["CurrentNumber"] = currentNumber;
            categories = categories.Skip((currentNumber - 1) * 8).Take(8);
            return View(categories);
        }

        // GET: CategoryController/Create
        public ActionResult Create()
        {
            CategoryVM category = new();
            return View(category);
        }

        // POST: CategoryController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CategoryVM category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            if (category is not null)
            {
                Category newCategory = new()
                {
                    Name = category.Name,
                    Status = category.Status,
                    CreatedOn = DateTime.Now,
                    CreatedBy = RouteData.Values["area"]?.ToString() ?? "System",
                    LastModifiedBy = RouteData.Values["area"]?.ToString() ?? "System",
                    LastModifiedOn = DateTime.Now
                };

                await _Repository.CreateAsync(newCategory);
                await _Repository.CommitAsync();
            }
            return RedirectToAction(nameof(Index));

        }

        // GET: CategoryController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var category = await _Repository.GetOneAsync(c => c.Id == id);
            if (category is null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: CategoryController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Category category)
        {
            if (category is null)
            {
                return NotFound();
            }

            var oldCategory = await _Repository.GetOneAsync(c => c.Id == category.Id);
            if (oldCategory is not null)
            {
                oldCategory.Name = category.Name;
                oldCategory.Status = category.Status;
                oldCategory.LastModifiedBy = RouteData.Values["area"]?.ToString() ?? "System";
                oldCategory.LastModifiedOn = DateTime.Now;

                _Repository.Update(oldCategory);
                await _Repository.CommitAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _Repository.GetOneAsync(a => a.Id == id, includes: [m => m.Movies]);

            if (category == null)
                return NotFound();

            if (category.Movies.Count != 0)
            {
                // 🔴 هنا بتضيف رسالة خطأ على ModelState
                ModelState.AddModelError(string.Empty, "لا يمكن حذف هذا التصنيف لأنه يحتوي على أفلام مرتبطة به.");

                // 👇 رجّع المستخدم لنفس صفحة الـ _RepositoryIndex مع عرض كل التصنيفات
                var categories = await _Repository.GetAllAsync();
                return View("Index", categories);
            }

            _Repository.Delete(category);
            await _Repository.CommitAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
