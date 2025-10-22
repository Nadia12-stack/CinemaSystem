using CinemaSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace CinemaSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CinemaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CinemaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cinemas = _context.Cinemas.AsNoTracking()
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.Description,
                    e.Status,
                }).ToList();

            return View(cinemas);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateCinemaVM());
        }

        [HttpPost]
        public IActionResult Create(CreateCinemaVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var cinema = vm.Adapt<Cinema>();

            if (vm.Img is not null && vm.Img.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(vm.Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using var stream = System.IO.File.Create(filePath);
                vm.Img.CopyTo(stream);

                cinema.Img = fileName;
            }

            _context.Cinemas.Add(cinema);
            _context.SaveChanges();

            TempData["success-notification"] = "Cinema added successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var cinema = _context.Cinemas.Find(id);
            if (cinema is null)
                return RedirectToAction("NotFoundPage", "Home");

            return View(cinema.Adapt<UpdateCinemaVM>());
        }

        [HttpPost]
        public IActionResult Edit(UpdateCinemaVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var cinemaInDb = _context.Cinemas.AsNoTracking().FirstOrDefault(e => e.Id == vm.Id);
            if (cinemaInDb is null)
                return RedirectToAction("NotFoundPage", "Home");

            var cinema = vm.Adapt<Cinema>();

            if (vm.NewImg is not null && vm.NewImg.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(vm.NewImg.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using var stream = System.IO.File.Create(filePath);
                vm.NewImg.CopyTo(stream);

                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", cinemaInDb.Img);
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);

                cinema.Img = fileName;
            }
            else
            {
                cinema.Img = cinemaInDb.Img;
            }

            _context.Cinemas.Update(cinema);
            _context.SaveChanges();

            TempData["success-notification"] = "Cinema updated successfully";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var cinema = _context.Cinemas.Find(id);
            if (cinema is null)
                return RedirectToAction("NotFoundPage", "Home");

            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", cinema.Img);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);

            _context.Cinemas.Remove(cinema);
            _context.SaveChanges();

            TempData["success-notification"] = "Cinema deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
