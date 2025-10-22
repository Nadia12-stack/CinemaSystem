using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MovieController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(FilterMovieVM filterMovieVM, int page = 1)
        {
            var movies = _context.Movies
                .Include(e => e.Category)
                .Include(e => e.MovieCinemas)
                .Include(e => e.MovieActors).ThenInclude(ma => ma.Actor)
                .AsNoTracking()
                .AsQueryable();

            // Filters 
            if (!string.IsNullOrWhiteSpace(filterMovieVM.name))
            {
                movies = movies.Where(e => e.Name.Contains(filterMovieVM.name.Trim()));
                ViewBag.name = filterMovieVM.name;
            }

            if (filterMovieVM.categoryId is not null)
            {
                movies = movies.Where(e => e.CategoryId == filterMovieVM.categoryId);
                ViewBag.categoryId = filterMovieVM.categoryId;
            }

            if (filterMovieVM.cinemaId is not null)
            {
                movies = movies.Where(e => e.MovieCinemas.Any(c => c.CinemaId == filterMovieVM.cinemaId));
                ViewBag.cinemaId = filterMovieVM.cinemaId;
            }

          
            // Categories & Cinemas
            ViewBag.categories = _context.Categories.AsEnumerable();
            ViewData["cinemas"] = _context.Cinemas.AsEnumerable();

            // Pagination
            ViewBag.TotalPages = Math.Ceiling(movies.Count() / 8.0);
            ViewBag.CurrentPage = page;
            movies = movies.Skip((page - 1) * 8).Take(8);

            return View(movies.AsEnumerable());
        }

        [HttpGet]
        public IActionResult Create()
        {
            var categories = _context.Categories;
            var cinemas = _context.Cinemas;
            var actors = _context.Actors;

            return View(new MovieVM
            {
                Categories = categories.AsEnumerable(),
                Cinemas = cinemas.AsEnumerable(),
                actors = actors.AsEnumerable()
            });
        }

        [HttpPost]
        public IActionResult Create(Movie movie, IFormFile img, List<IFormFile>? subImgs, string[] actors)
        {
            var transaction = _context.Database.BeginTransaction();

            try
            {
                // Main Image
                if (img is not null && img.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                    var filePath = Path.Combine("wwwroot/images", fileName);
                    using var stream = System.IO.File.Create(filePath);
                    img.CopyTo(stream);
                    movie.MainImg = fileName;
                }

                // Save Movie
                var movieCreated = _context.Movies.Add(movie);
                _context.SaveChanges();

                // Sub Images
                if (subImgs is not null)
                {
                    foreach (var item in subImgs)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(item.FileName);
                        var filePath = Path.Combine("wwwroot/images/movie_images", fileName);
                        using var stream = System.IO.File.Create(filePath);
                        item.CopyTo(stream);

                        _context.MovieSubImages.Add(new()
                        {
                            Img = fileName,
                            MovieId = movieCreated.Entity.Id,
                        });
                    }

                    _context.SaveChanges();
                }

                TempData["success-notification"] = "Add Movie Successfully";
                transaction.Commit();
            }
            catch
            {
                TempData["error-notification"] = "Error While Saving the movie";
                transaction.Rollback();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var movie = _context.Movies
                .Include(e => e.MovieActors)
                .Include(e => e.movieSubImages)
                .FirstOrDefault(e => e.Id == id);

            if (movie is null)
                return RedirectToAction("NotFoundPage", "Home");

            var categories = _context.Categories;
            var cinemas = _context.Cinemas;
            var actors = _context.Actors;

            return View(new MovieVM
            {
                Categories = categories.AsEnumerable(),
                Cinemas = cinemas.AsEnumerable(),
                actors = actors.AsEnumerable(),
                Movie = movie
            });
        }

        [HttpPost]
        public IActionResult Edit(Movie movie, IFormFile? img, List<IFormFile>? subImgs, string[] actors)
        {
            var movieInDb = _context.Movies
                .Include(e => e.MovieActors)
                .AsNoTracking()
                .FirstOrDefault(e => e.Id == movie.Id);

            if (movieInDb is null)
                return RedirectToAction("NotFoundPage", "Home");

            // Main Image
            if (img is not null && img.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(img.FileName);
                var filePath = Path.Combine("wwwroot/images", fileName);
                using var stream = System.IO.File.Create(filePath);
                img.CopyTo(stream);

                var oldPath = Path.Combine("wwwroot/images", movieInDb.MainImg);
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);

                movie.MainImg = fileName;
            }
            else
            {
                movie.MainImg = movieInDb.MainImg;
            }

            _context.Movies.Update(movie);
            _context.SaveChanges();

            // Sub Images
            if (subImgs is not null && subImgs.Count > 0)
            {
                movie.movieSubImages = new List<MovieSubImage>();

                foreach (var item in subImgs)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(item.FileName);
                    var filePath = Path.Combine("wwwroot/images/movie_images", fileName);
                    using var stream = System.IO.File.Create(filePath);
                    item.CopyTo(stream);

                    movie.movieSubImages.Add(new()
                    {
                        Img = fileName,
                        MovieId = movie.Id,
                    });
                }

                _context.SaveChanges();
            }

            TempData["success-notification"] = "Update Movie Successfully";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var movie = _context.Movies
                .Include(e => e.movieSubImages)
                .FirstOrDefault(e => e.Id == id);

            if (movie is null)
                return RedirectToAction("NotFoundPage", "Home");

            var oldPath = Path.Combine("wwwroot/images", movie.MainImg);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);

            foreach (var item in movie.movieSubImages)
            {
                var subImgOldPath = Path.Combine("wwwroot/images/movie_images", item.Img);
                if (System.IO.File.Exists(subImgOldPath))
                    System.IO.File.Delete(subImgOldPath);
            }

            _context.Movies.Remove(movie);
            _context.SaveChanges();

            TempData["success-notification"] = "Delete Movie Successfully";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DeleteSubImg(int movieId, string Img)
        {
            var movieSubImgInDb = _context.MovieSubImages
                .FirstOrDefault(e => e.MovieId == movieId && e.Img == Img);

            if (movieSubImgInDb is null)
                return RedirectToAction("NotFoundPage", "Home");

            var oldPath = Path.Combine("wwwroot/images/movie_images", movieSubImgInDb.Img);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);

            _context.Remove(movieSubImgInDb);
            _context.SaveChanges();

            return RedirectToAction(nameof(Edit), new { id = movieId });
        }
    }
}
