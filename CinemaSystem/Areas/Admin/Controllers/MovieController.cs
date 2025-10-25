using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            const int pageSize = 11;

            var moviesQuery = _context.Movies
                .Include(m => m.Category)
                .Include(m => m.MovieCinemas).ThenInclude(mc => mc.Cinema)
                .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
                .AsNoTracking()
                .AsQueryable();

            // Filters
            if (!string.IsNullOrWhiteSpace(filterMovieVM.name))
                moviesQuery = moviesQuery.Where(m => m.Name.Contains(filterMovieVM.name.Trim()));

            if (filterMovieVM.categoryId.HasValue)
                moviesQuery = moviesQuery.Where(m => m.CategoryId == filterMovieVM.categoryId);

            if (filterMovieVM.cinemaId.HasValue)
                moviesQuery = moviesQuery.Where(m => m.MovieCinemas.Any(mc => mc.CinemaId == filterMovieVM.cinemaId));

            var totalMovies = moviesQuery.Count();
            var totalPages = (int)Math.Ceiling(totalMovies / (double)pageSize);

            var movies = moviesQuery
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Filter = filterMovieVM;
            ViewBag.TotalMovies = totalMovies;

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.Cinemas = new SelectList(_context.Cinemas, "Id", "Name");
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(movies);
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
        public IActionResult Create(MovieVM movieVM, IFormFile img, List<IFormFile>? subImgs, string[] actors, List<int> cinemaIds)
        {
            var transaction = _context.Database.BeginTransaction();

            try
            {
                var movie = movieVM.Movie!;

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
                _context.Movies.Add(movie);
                _context.SaveChanges();

                // Sub Images
                if (subImgs is not null)
                {
                    foreach (var item in subImgs)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(item.FileName);
                        var filePath = Path.Combine("wwwroot/images/movieimages", fileName);
                        using var stream = System.IO.File.Create(filePath);
                        item.CopyTo(stream);

                        _context.MovieSubImages.Add(new MovieSubImage
                        {
                            Img = fileName,
                            MovieId = movie.Id
                        });
                    }
                    _context.SaveChanges();
                }

                // Save Actors
                if (actors is not null)
                {
                    foreach (var actorId in actors)
                    {
                        _context.MovieActors.Add(new MovieActor
                        {
                            ActorId = int.Parse(actorId),
                            MovieId = movie.Id
                        });
                    }
                    _context.SaveChanges();
                }

                // Save Cinemas
                if (cinemaIds is not null)
                {
                    foreach (var cinemaId in cinemaIds)
                    {
                        _context.MovieCinema.Add(new MovieCinema
                        {
                            CinemaId = cinemaId,
                            MovieId = movie.Id
                        });
                    }
                    _context.SaveChanges();
                }

                TempData["SuccessMessage"] = "Add Movie Successfully";
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
                .Include(m => m.MovieActors)
                .Include(m => m.MovieCinemas)
                .Include(m => m.Category)
                .FirstOrDefault(m => m.Id == id);

            if (movie == null)
                return NotFound();

            var subImages = _context.MovieSubImages.Where(s => s.MovieId == id).ToList();

            var movieVM = new MovieVM
            {
                Movie = movie,
                MovieSubImages = subImages,
                Categories = _context.Categories.AsEnumerable(),
                Cinemas = _context.Cinemas.AsEnumerable(),
                actors = _context.Actors.AsEnumerable()
            };

            return View(movieVM);
        }


        [HttpPost]
        public IActionResult Edit(Movie movie, IFormFile? img, List<IFormFile>? subImgs, string[] actors, List<int> cinemaIds)
        {
            var transaction = _context.Database.BeginTransaction();

            try
            {
                var movieInDb = _context.Movies
                    .Include(e => e.MovieActors)
                    .Include(e => e.MovieCinemas)
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
                    foreach (var item in subImgs)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(item.FileName);
                        var filePath = Path.Combine("wwwroot/images/movieimages", fileName);
                        using var stream = System.IO.File.Create(filePath);
                        item.CopyTo(stream);

                        _context.MovieSubImages.Add(new MovieSubImage
                        {
                            Img = fileName,
                            MovieId = movie.Id
                        });
                    }
                    _context.SaveChanges();
                }

                // Update Actors
                var oldActors = _context.MovieActors.Where(a => a.MovieId == movie.Id);
                _context.MovieActors.RemoveRange(oldActors);
                _context.SaveChanges();

                if (actors is not null)
                {
                    foreach (var actorId in actors)
                    {
                        _context.MovieActors.Add(new MovieActor
                        {
                            ActorId = int.Parse(actorId),
                            MovieId = movie.Id
                        });
                    }
                    _context.SaveChanges();
                }

                // Update Cinemas
                var oldCinemas = _context.MovieCinema.Where(c => c.MovieId == movie.Id);
                _context.MovieCinema.RemoveRange(oldCinemas);
                _context.SaveChanges();

                if (cinemaIds is not null)
                {
                    foreach (var cinemaId in cinemaIds)
                    {
                        _context.MovieCinema.Add(new MovieCinema
                        {
                            CinemaId = cinemaId,
                            MovieId = movie.Id
                        });
                    }
                    _context.SaveChanges();
                }

                TempData["SuccessMessage"] = "Update Movie Successfully";
                transaction.Commit();
            }
            catch
            {
                TempData["error-notification"] = "Error While Updating the movie";
                transaction.Rollback();
            }

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
                var subImgOldPath = Path.Combine("wwwroot/images/movieimages", item.Img);
                if (System.IO.File.Exists(subImgOldPath))
                    System.IO.File.Delete(subImgOldPath);
            }

            _context.Movies.Remove(movie);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Delete Movie Successfully";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DeleteSubImg(int movieId, string Img)
        {
            var movieSubImgInDb = _context.MovieSubImages
                .FirstOrDefault(e => e.MovieId == movieId && e.Img == Img);

            if (movieSubImgInDb is null)
                return RedirectToAction("NotFoundPage", "Home");

            var oldPath = Path.Combine("wwwroot/images/movieimages", movieSubImgInDb.Img);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);

            _context.Remove(movieSubImgInDb);
            _context.SaveChanges();

            return RedirectToAction(nameof(Edit), new { id = movieId });
        }


        public IActionResult Details(int id)
        {
            var movie = _context.Movies
                .Include(m => m.Category)
                .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
                .Include(m => m.MovieCinemas).ThenInclude(mc => mc.Cinema)
                .Include(m => m.movieSubImages)
                //.Include(m => m.SocialLinks).ThenInclude(sl => sl.Actor)
                .FirstOrDefault(m => m.Id == id);

            if (movie == null)
                return NotFound();

            var viewModel = new MovieVM
            {
                Movie = movie,
                MovieSubImages = movie.movieSubImages

            };

            return View(viewModel);
        }



    }
}
