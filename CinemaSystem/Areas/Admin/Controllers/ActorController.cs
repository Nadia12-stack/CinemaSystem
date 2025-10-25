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

        public IActionResult Index(int page = 1)
        {
            int pageSize = 11;

            var totalActors = _context.Actors.Count();

            var actors = _context.Actors
                .Include(a => a.MovieActors).ThenInclude(ma => ma.Movie)
                .Include(a => a.SocialLinks)
                .AsNoTracking()
                .OrderBy(a => a.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalActors / pageSize);
            ViewBag.TotalActors = totalActors;

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
            TempData["SuccessMessage"] = "Actor has been added successfully!";
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
            TempData["SuccessMessage"] = "Actor has been updated successfully!";
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

        public IActionResult Details(int id)
        {
            var actor = _context.Actors
                .Include(a => a.MovieActors)
                    .ThenInclude(ma => ma.Movie)
                .Include(a => a.SocialLinks)
                .FirstOrDefault(a => a.Id == id);

            if (actor == null)
                return NotFound();

            return View(actor);
        }


    }
}
