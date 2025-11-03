using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MoviesDashboard.Models;
using MoviesDashboard.Repositories.IRepositories;
using MoviesDashboard.ViewModels;

namespace MoviesDashboard.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MovieController : Controller
    {
        private readonly IRepository<Movie> _MovieRepo;
        private readonly IRepository<Category> _CategoryRepo;
        private readonly IRepository<Cinema> _CinemaRepo;
        private readonly IRepository<MovieImage> _MovieImageRepo;
        public MovieController(
            IRepository<Movie> movieRepo,
            IRepository<Category> categoryRepo,
            IRepository<Cinema> cinemaRepo,
            IRepository<MovieImage> movieImageRepo)
        {
            _MovieRepo = movieRepo;
            _CategoryRepo = categoryRepo;
            _CinemaRepo = cinemaRepo;
            _MovieImageRepo = movieImageRepo;
        }

        public async Task<IActionResult> Index(int currentNumber = 1)
        {
            var movies = await _MovieRepo.GetAllAsync();
            ViewData["currentNumber"] = currentNumber;
            ViewData["pageNumbers"] = Math.Ceiling(movies.Count() / 8.0);
            movies = movies.Skip((currentNumber - 1) * 8).Take(8);

            return View(movies.AsEnumerable());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            CreateMovieVM createMovieVM = new()
            {
                Categories = new SelectList(await _CategoryRepo.GetAllAsync(), "Id", "Name"),
                Cinemas = new SelectList(await _CinemaRepo.GetAllAsync(), "Id", "Name"),
            };
            return View(createMovieVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMovieVM vm)
        {
            ModelState.Remove(nameof(vm.Categories));
            ModelState.Remove(nameof(vm.Cinemas));

            if (!ModelState.IsValid)
            {
                // لازم نرجّع القوائم المنسدلة تاني لو فيه error علشان يعرض المدخلات الخطأ
                vm.Categories = new SelectList(await _CategoryRepo.GetAllAsync(), "Id", "Name");
                vm.Cinemas = new SelectList(await _CinemaRepo.GetAllAsync(), "Id", "Name");
                return View(vm);
            }

            // الصورة الاساسية
            string? mainFileName = null;
            if (vm.MainImage is not null && vm.MainImage.Length > 0)
            {
                // توليد guid + extension
                mainFileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.MainImage.FileName);
                // وضعها في ال path الخاص بيها
                var mainPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//Images//Movies", mainFileName);
                // حفظ الصورة في المسار
                using var stream = System.IO.File.Create(mainPath);
                vm.MainImage.CopyTo(stream);
                //vm.MainImage = mainFileName;
            }

            // إنشاء كائن Movie جديد
            var movie = new Movie
            {
                Name = vm.Name,
                Descrption = vm.Descrption,
                Price = vm.Price,
                Status = vm.Status,
                Date = vm.Date,
                Time = vm.Time,
                CategoryId = vm.CategoryId,
                CinemaId = vm.CinemaId,
                MainImg = mainFileName,
                CreatedOn = DateTime.Now,
                CreatedBy = RouteData.Values["area"]?.ToString() ?? "System",
                LastModifiedBy = RouteData.Values["area"]?.ToString() ?? "System",
                LastModifiedOn = DateTime.Now
            };
            await _MovieRepo.CreateAsync(movie);
            await _MovieRepo.CommitAsync();

            // Response.Cookies.Append("Cookies-succuss", "Done You Add new Movie 👌");  دي مشكلتها انها مش بتختفي غير لما تعملها اعدادات معينه في البارامتر التاليت
            TempData["Cookies-succuss"] = "Done You Added new Movie";

            // رفع الصور الفرعية (لو فيه)
            if (vm.SubImages != null && vm.SubImages.Count > 0)
            {
                foreach (var img in vm.SubImages)
                {
                    string subFile = Guid.NewGuid() + Path.GetExtension(img.FileName);
                    string subPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//Images//Movies", subFile);

                    using var stream = System.IO.File.Create(subPath);
                    img.CopyTo(stream);

                    await _MovieImageRepo.CreateAsync(new MovieImage
                    {
                        MovieId = movie.Id,
                        ImageUrl = subFile,
                        Order = 1
                    });
                }

                await _MovieImageRepo.CommitAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Movie/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            //var movie = _MovieRepo.GetOneAsync(m=>m.Id == id, includes: [m => m.SubImages]);
            var movie = await _MovieRepo.GetOneAsync(m => m.Id == id, includes: [s=> s.SubImages]);


            if (movie == null)
                return NotFound();

            var vm = new EditMovieVM
            {
                Id = movie.Id,
                Name = movie.Name,
                Descrption = movie.Descrption,
                Price = movie.Price,
                Status = movie.Status,
                Date = movie.Date,
                Time = movie.Time,
                MainImg = movie.MainImg,                   // اسم الملف الحالي
                CategoryId = movie.CategoryId,
                CinemaId = movie.CinemaId,
                SubImages = movie.SubImages.Select(si => new MovieImageVM { ImageUrl = si.ImageUrl, Order = si.Order }).ToList(),
                Categories = new SelectList(await _CategoryRepo.GetAllAsync(), "Id", "Name"),
                Cinemas = new SelectList(await _CinemaRepo.GetAllAsync(), "Id", "Name")
            };

            return View(vm);
        }

        // POST: Admin/Movie/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditMovieVM vm)
        {
            // Remove display-only collections so they don't break ModelState (optional if they aren't validated)
            ModelState.Remove(nameof(vm.Categories));
            ModelState.Remove(nameof(vm.Cinemas));
            ModelState.Remove(nameof(vm.SubImages));

            if (!ModelState.IsValid)
            {
                // Re-populate dropdowns before returning the view
                vm.Categories = new SelectList(await _CategoryRepo.GetAllAsync(), "Id", "Name");
                vm.Cinemas = new SelectList(await _CinemaRepo.GetAllAsync(), "Id", "Name");

                return View(vm);
            }

            var movie = await _MovieRepo.GetOneAsync(m => m.Id == vm.Id);

            // update scalar properties
            movie.Name = vm.Name;
            movie.Descrption = vm.Descrption;
            movie.Price = vm.Price;
            movie.Status = vm.Status;
            movie.Date = vm.Date;
            movie.Time = vm.Time;
            movie.CategoryId = vm.CategoryId;
            movie.CinemaId = vm.CinemaId;
            movie.LastModifiedOn = DateTime.Now;
            movie.LastModifiedBy = User?.Identity?.Name ?? "Admin";

            // 1) Handle main image - replace if new file provided
            if (vm.MainImageFile != null && vm.MainImageFile.Length > 0)
            {
                // delete old file if exists
                if (!string.IsNullOrEmpty(movie.MainImg))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//Images//movies", movie.MainImg);
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                var newMainName = Guid.NewGuid().ToString() + Path.GetExtension(vm.MainImageFile.FileName);
                var newMainPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//images//movies", newMainName);

                using var stream = System.IO.File.Create(newMainPath);
                vm.MainImageFile.CopyTo(stream);

                movie.MainImg = newMainName;
            }

            // 2) Handle new sub images (append)
            if (vm.NewSubImages != null && vm.NewSubImages.Any())
            {
                foreach (var file in vm.NewSubImages)
                {
                    if (file == null || file.Length == 0) continue;

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//images//movies", fileName);

                    using var fs = new FileStream(filePath, FileMode.Create);
                    file.CopyTo(fs);

                    // Add DB record
                    var movieImage = new MovieImage
                    {
                        MovieId = movie.Id,
                        ImageUrl = fileName,
                        Order = 1
                    };
                    await _MovieImageRepo.CreateAsync(movieImage);
                }
            }

            await _MovieImageRepo.CommitAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(int id)
        {
            var movie =await _MovieRepo.GetOneAsync(m=> m.Id == id);
            if (movie == null)
                return NotFound();
            
            _MovieRepo.Delete(movie);
            await _MovieRepo.CommitAsync();

            return RedirectToAction(nameof(Index));

        }

    }
}
