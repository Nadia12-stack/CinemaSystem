using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaSystem.Models;

namespace CinemaSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ActorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActorController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var actors = _context.Actors.AsNoTracking().ToList();
            return View(actors);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Actor());
        }

        [HttpPost]
        public IActionResult Create(Actor actor, IFormFile? img)
        {
            if (!ModelState.IsValid)
                return View(actor);

            if (img is not null && img.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/actors", fileName);
                using var stream = System.IO.File.Create(path);
                img.CopyTo(stream);
                actor.Img = fileName;
            }

            _context.Actors.Add(actor);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var actor = _context.Actors.FirstOrDefault(a => a.Id == id);
            if (actor is null)
                return RedirectToAction("NotFoundPage", "Home");

            return View(actor);
        }

        [HttpPost]
        public IActionResult Edit(Actor actor, IFormFile? img)
        {
            var actorInDb = _context.Actors.AsNoTracking().FirstOrDefault(a => a.Id == actor.Id);
            if (actorInDb is null)
                return RedirectToAction("NotFoundPage", "Home");

            if (!ModelState.IsValid)
                return View(actor);

            if (img is not null && img.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/actors", fileName);
                using var stream = System.IO.File.Create(path);
                img.CopyTo(stream);

                var oldPath = Path.Combine("wwwroot/images/actors", actorInDb.Img);
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);

                actor.Img = fileName;
            }
            else
            {
                actor.Img = actorInDb.Img;
            }

            _context.Actors.Update(actor);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var actor = _context.Actors.FirstOrDefault(a => a.Id == id);
            if (actor is null)
                return RedirectToAction("NotFoundPage", "Home");

            var path = Path.Combine("wwwroot/images/actors", actor.Img);
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            _context.Actors.Remove(actor);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
