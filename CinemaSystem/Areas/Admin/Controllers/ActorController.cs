using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace CinemaSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ActorController : Controller
    {
        ApplicationDbContext _context = new();

        public IActionResult Index()
        {
            var actors = _context.Actors.AsNoTracking().AsQueryable();

            // Add Filter

            return View(actors.AsEnumerable());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Actor());
        }

        [HttpPost]
        public IActionResult Create(Actor actor)
        {
            if(!ModelState.IsValid)
            {
                return View(actor);
            }

            _context.Actors.Add(actor);
            _context.SaveChanges();

            //return View(nameof(Index));
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var actor = _context.Actors.FirstOrDefault(e => e.Id == id);

            if (actor is null)
                return RedirectToAction("NotFoundPage", "Home");

            return View(actor);
        }

        [HttpPost]
        public IActionResult Edit(Actor actor)
        {
            if (!ModelState.IsValid)
            {
                //ModelState.AddModelError(string.Empty, "Any More Errors");

                return View(actor);
            }

            _context.Actors.Update(actor);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var actor = _context.Actors.FirstOrDefault(e => e.Id == id);

            if (actor is null)
                return RedirectToAction("NotFoundPage", "Home");

            _context.Actors.Remove(actor);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
