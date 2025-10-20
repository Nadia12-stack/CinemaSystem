
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CinemaSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MovieController : Controller
    {
        ApplicationDbContext _context = new();

        public IActionResult Index(FilterMovieVM filterMovieVM, int page = 1)
        {
           
            var movies = _context.Movies.AsNoTracking().AsQueryable();

            // Add Filter
            movies = movies.Include(e => e.Category).Include(e => e.Brand);

            #region Filter Movie
            // Add Filter 
            if (filterMovieVM.name is not null)
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
                movies = movies.Where(e => e.BrandId == filterMovieVM.cinemaId);
                ViewBag.brandId = filterMovieVM.cinemaId;
            }

         

            // Categories
            var categories = _context.Categories;
            //ViewData["categories"] = categories.AsEnumerable();
            ViewBag.categories = categories.AsEnumerable();

            // Cinemas
            var cinemas = _context.Cinemas;
            ViewData["cinemas"] = cinemas.AsEnumerable();
            #endregion

            #region Pagination
            // Pagination
            ViewBag.TotalPages = Math.Ceiling(movies.Count() / 8.0);
            ViewBag.CurrentPage = page;
            movies = movies.Skip((page - 1) * 8).Take(8); // 0 .. 8 
            #endregion

            return View(movies.AsEnumerable());
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Categories
            var categories = _context.Categories;
            // Cinemas
            var cinemas = _context.Cinemas;

            return View(new MovieVM
            {
                Categories = categories.AsEnumerable(),
                Cinemas = cinemas.AsEnumerable(),
            });
        }

        [HttpPost]
        public IActionResult Create(Movie movie, IFormFile img, List<IFormFile>? subImgs, string[] actors)
        {
            var transaction = _context.Database.BeginTransaction();

            try
            {
                if (img is not null && img.Length > 0)
                {
                    // Save Img in wwwroot
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName); // 30291jsfd4-210klsdf32-4vsfksgs.png
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        img.CopyTo(stream);
                    }

                    // Save Img in db
                    movie.MainImg = fileName;
                }

                // Save movie in db
                var movieCreated = _context.Movies.Add(movie);
                _context.SaveChanges();

                if (img is not null && img.Length > 0)
                {
                    foreach (var item in subImgs)
                    {
                        // Save Img in wwwroot
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName); // 30291jsfd4-210klsdf32-4vsfksgs.png
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\movie_images", fileName);

                        using (var stream = System.IO.File.Create(filePath))
                        {
                            img.CopyTo(stream);
                        }

                        _context.MovieSubImages.Add(new()
                        {
                            Img = fileName,
                            MovieId = movieCreated.Entity.Id,
                        });
                    }

                    _context.SaveChanges();
                }

                if (actors.Any())
                {
                    foreach (var item in actors)
                    {
                        _context.Actors.Add(new()
                        {
                            Name = item,
                            MovieId = movieCreated.Entity.Id,
                        });
                    }

                    _context.SaveChanges();
                }

                //Response.Cookies.Append("success-notification", "Add Movie Successfully");
                TempData["success-notification"] = "Add Movie Successfully";

                transaction.Commit();
            }
            catch(Exception ex)
            {
                // Logging
                TempData["error-notification"] = "Error While Saving the movie";

                transaction.Rollback();

            }

            //return View(nameof(Index));
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var movie = _context.Movies.Include(e => e.actors).Include(e => e.movieSubImages).FirstOrDefault(e => e.Id == id);

            if (movie is null)
                return RedirectToAction("NotFoundPage", "Home");

            // Categories
            var categories = _context.Categories;
            // Cinemas
            var cinemas = _context.Cinemas;

            return View(new MovieVM
            {
                Categories = categories.AsEnumerable(),
                Cinemas = cinemas.AsEnumerable(),
                Movie = movie,
            });
        }

        [HttpPost]
        public IActionResult Edit(Movie movie, IFormFile? img, List<IFormFile>? subImgs, string[] actors)
        {
            var movieInDb = _context.Movies.Include(e => e.actors).AsNoTracking().FirstOrDefault(e => e.Id == movie.Id);
            if(movieInDb is null)
                return RedirectToAction("NotFoundPage", "Home");

            if (img is not null)
            {
                if(img.Length > 0)
                {
                    // Save Img in wwwroot
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName); // 30291jsfd4-210klsdf32-4vsfksgs.png
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        img.CopyTo(stream);
                    }

                    // Remove old Img in wwwroot
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", movieInDb.MainImg);
                    if(System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }

                    // Save Img in db
                    movie.MainImg = fileName;
                }
            }
            else
            {
                movie.MainImg = movieInDb.MainImg;
            }

            _context.Movies.Update(movie);
            _context.SaveChanges();

            if (subImgs is not null && subImgs.Count > 0)
            {
                movie.movieSubImages = new List<MovieSubImage>();

                foreach (var item in subImgs)
                {
                    // Save Img in wwwroot
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(item.FileName); // 30291jsfd4-210klsdf32-4vsfksgs.png
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\movie_images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        item.CopyTo(stream);
                    }

                    movie.movieSubImages.Add(new()
                    {
                        Img = fileName,
                        MovieId = movie.Id,
                    });
                }

                _context.SaveChanges();
            }


            if (actors.Any())
            {
                _context.Actors.RemoveRange(movieInDb.actors);

                movie.actors = new List<Actor>();

                foreach (var item in actors)
                {
                    movie.actors.Add(new()
                    {
                        Name = item,
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
            var movie = _context.Movies.Include(e=>e.actors).Include(e=>e.movieSubImages).FirstOrDefault(e => e.Id == id);

            if (movie is null)
                return RedirectToAction("NotFoundPage", "Home");

            // Remove old Img in wwwroot
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", movie.MainImg);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            foreach (var item in movie.movieSubImages)
            {
                var subImgOldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\movie_images", item.Img);
                if (System.IO.File.Exists(subImgOldPath))
                {
                    System.IO.File.Delete(subImgOldPath);
                }
            }


            _context.Movies.Remove(movie);
            _context.SaveChanges();

            TempData["success-notification"] = "Delete Movie Successfully";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult DeleteSubImg(int movieId, string Img)
        {
            var movieSubImgInDb = _context.MovieSubImages.FirstOrDefault(e=>e.MovieId == movieId && e.Img == Img);

            if(movieSubImgInDb is null)
                return RedirectToAction("NotFoundPage", "Home");

            // Remove old Img in wwwroot
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images\\movie_images", movieSubImgInDb.Img);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            _context.Remove(movieSubImgInDb);
            _context.SaveChanges();

            return RedirectToAction(nameof(Edit), new { id = movieId });
        }
    }
}
