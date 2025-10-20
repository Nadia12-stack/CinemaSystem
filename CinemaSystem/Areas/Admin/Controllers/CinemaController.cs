using CinemaSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Mapster;

namespace CinemaSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CinemaController : Controller
    {
        ApplicationDbContext _context = new();

        public IActionResult Index()
        {
            var cinemas = _context.Cinemas.AsNoTracking().AsQueryable();

            // Add Filter

            return View(cinemas.Select(e => new
            {
                e.Id,
                e.Name,
                e.Description,
                e.Status,
            }).AsEnumerable());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateCinemaVM());
        }

        [HttpPost]
        public IActionResult Create(CreateCinemaVM createCinemaVM)
        {
            if (!ModelState.IsValid)
            {
                return View(createCinemaVM);
            }

          

            Cinema cinema = createCinemaVM.Adapt<Cinema>();

            if (createCinemaVM.Img is not null && createCinemaVM.Img.Length > 0)
            {
                // Save Img in wwwroot
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(createCinemaVM.Img.FileName); 
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                using(var stream = System.IO.File.Create(filePath))
                {
                    createCinemaVM.Img.CopyTo(stream);
                }

                // Save Img in db
                cinema.Img = fileName;
            }

            // Save cinema in db
            _context.Cinemas.Add(cinema);
            _context.SaveChanges();

            TempData["success-notification"] = "Add Cinema Successfully";

            //return View(nameof(Index));
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var cinema = _context.Cinemas.FirstOrDefault(e => e.Id == id);

            if (cinema is null)
                return RedirectToAction("NotFoundPage", "Home");

         

            return View(cinema.Adapt<UpdateCinemaVM>());
        }

        [HttpPost]
        public IActionResult Edit(UpdateCinemaVM updateCinemaVM)
        {
            if (!ModelState.IsValid)
            {
                return View(updateCinemaVM);
            }

            var cinemaInDb = _context.Cinemas.AsNoTracking().FirstOrDefault(e => e.Id == updateCinemaVM.Id);
            if(cinemaInDb is null)
                return RedirectToAction("NotFoundPage", "Home");

            

            Cinema cinema = updateCinemaVM.Adapt<Cinema>();

            if (updateCinemaVM.NewImg is not null)
            {
                if(updateCinemaVM.NewImg.Length > 0)
                {
                    // Save Img in wwwroot
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(updateCinemaVM.NewImg.FileName); // 30291jsfd4-210klsdf32-4vsfksgs.png
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        updateCinemaVM.NewImg.CopyTo(stream);
                    }

                    // Remove old Img in wwwroot
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", cinemaInDb.Img);
                    if(System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }

                    // Save Img in db
                    cinema.Img = fileName;
                }
            }
            else
            {
                cinema.Img = cinemaInDb.Img;
            }

            _context.Cinemas.Update(cinema);
            _context.SaveChanges();

            TempData["success-notification"] = "Update Cinema Successfully";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var cinema = _context.Cinemas.FirstOrDefault(e => e.Id == id);

            if (cinema is null)
                return RedirectToAction("NotFoundPage", "Home");

            // Remove old Img in wwwroot
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", cinema.Img);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            _context.Cinemas.Remove(cinema);
            _context.SaveChanges();

            TempData["success-notification"] = "Delete Cinema Successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}
