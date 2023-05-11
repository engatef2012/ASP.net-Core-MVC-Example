using CRUD.Models;
using CRUD.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace CRUD.Controllers
{
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly int _maxAllowedImageSize = 1_048_576;
        private readonly List<string> _allowedExtensions = new() { ".jpg", ".png" };
        private readonly IToastNotification _toastNotification;
        public MovieController(ApplicationDbContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies.AsNoTracking().OrderByDescending(m => m.Rate).ToListAsync();
            return View(movies);
        }
        public async Task<IActionResult> Create()
        {
            var viewModel = new MovieFormViewModel
            {
                Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync()
            };
            return View("MovieForm", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieFormViewModel model)
        {
            model.Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
            if (!ModelState.IsValid)
            {
                return View("MovieForm", model);
            }
            var files = Request.Form.Files;
            // checking whether the model had the poster or not
            if (!files.Any())
            {
                ModelState.AddModelError("poster", "Add poster");
                return View("MovieForm", model);
            }
            var poster = files.FirstOrDefault();
            // checking if the poster extension is allowed 
            if (!_allowedExtensions.Contains(Path.GetExtension(poster.FileName.ToLower())))
            {
                ModelState.AddModelError("poster", "the poster must be .jpg or .png");
                return View("MovieForm", model);
            }
            // checking if the poster size is less than 1MB (1_048_576 bytes)
            if (poster.Length > _maxAllowedImageSize)
            {
                ModelState.AddModelError("poster", "the poster must be less than 1MB");
                return View("MovieForm", model);
            }
            using var datastream = new MemoryStream();
            await poster.CopyToAsync(datastream);
            var movie = new Movie()
            {
                GenreId = model.GenreId,
                Poster = datastream.ToArray(),
                Rate = model.Rate,
                Title = model.Title,
                StoryLine = model.StoryLine,
                Year = model.Year
            };
            _context.Movies.Add(movie);
            _context.SaveChanges();
            _toastNotification.AddSuccessToastMessage("Movie Added Successfully");
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound();
            var model = new MovieFormViewModel()
            {
                Id = movie.MovieId,
                Rate = movie.Rate,
                Title = movie.Title,
                Year = movie.Year,
                StoryLine = movie.StoryLine,
                GenreId = movie.GenreId,
                Poster = movie.Poster,
                Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync()
            };
            return View("MovieForm", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                return View("MovieForm", model);
            }
            var movie = await _context.Movies.FindAsync(model.Id);
            if (movie == null)
                return NotFound();
            var files = Request.Form.Files;
            if (files.Any())
            {
                var poster = files.FirstOrDefault();
                using var memoryStream = new MemoryStream();
                await poster.CopyToAsync(memoryStream);
                model.Poster = memoryStream.ToArray();
                if (poster.Length > _maxAllowedImageSize)
                {
                    model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("poster", "the poster must be less than 1MB");
                    return View("MovieForm", model);
                }
                if (!_allowedExtensions.Contains(Path.GetExtension(poster.FileName.ToLower())))
                {
                    model.Genres = await _context.Genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("poster", "the poster must be .jpg or .png");
                    return View("MovieForm", model);
                }
                movie.Poster = model.Poster;
            }
            movie.Title = model.Title;
            movie.Year = model.Year;
            movie.StoryLine = model.StoryLine;
            movie.Rate = model.Rate;
            movie.GenreId = model.GenreId;
            await _context.SaveChangesAsync();
            _toastNotification.AddSuccessToastMessage("Movie Edited Successfully");
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();
            var movie = await _context.Movies.Include(m => m.Genre).FirstOrDefaultAsync(m => m.MovieId == id);
            if (movie == null)
                return NotFound();
            return View(movie);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound();
            _context.Movies.Remove(movie);
            _context.SaveChanges();
            return Ok();
        }
    }
}
