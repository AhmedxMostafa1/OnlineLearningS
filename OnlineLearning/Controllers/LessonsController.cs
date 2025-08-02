using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearning.Models;
using System.Text.RegularExpressions;

namespace OnlineLearning.Controllers
{
    public class LessonsController : Controller
    {
        private readonly OnlineLearningContext _context;

        public LessonsController(OnlineLearningContext context)
        {
            _context = context;
        }

        // GET: Lessons for a module
        public IActionResult Index(int moduleId)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Instructor")
            {
                return Unauthorized();
            }

            var module = _context.Modules
                .Include(m => m.Lessons)
                .FirstOrDefault(m => m.ModuleId == moduleId);

            if (module == null) return NotFound();

            ViewBag.ModuleTitle = module.ModuleTitle;
            ViewBag.ModuleId = module.ModuleId;
            return View(module.Lessons);
        }

        // GET: Create Lesson
        public IActionResult Create(int moduleId)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Instructor")
            {
                return Unauthorized();
            }

            var module = _context.Modules.Find(moduleId);
            if (module == null) return NotFound();

            ViewBag.ModuleId = module.ModuleId;
            ViewBag.ModuleTitle = module.ModuleTitle;

            return View(new Lesson { ModuleId = moduleId });
        }

        // POST: Create Lesson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Lesson lesson)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Instructor")
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                _context.Lessons.Add(lesson);
                _context.SaveChanges();
                return RedirectToAction("Index", new { moduleId = lesson.ModuleId });
            }

            ViewBag.ModuleId = lesson.ModuleId;
            return View(lesson);
        }
        [HttpGet]
        public async Task<IActionResult> ViewLesson(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");

            var lesson = await _context.Lessons
                .Include(l => l.Module)
                    .ThenInclude(m => m.Course)
                        .ThenInclude(c => c.Enrollments)
                .FirstOrDefaultAsync(l => l.LessonId == id);

            if (lesson == null)
                return NotFound();

            var course = lesson.Module?.Course;
            if (course == null)
                return BadRequest("Associated course not found.");

            bool canView = false;
            if (role == "Instructor" && course.InstructorId == userId)
                canView = true;
            else if (role == "Student" && userId.HasValue)
                canView = course.Enrollments.Any(e => e.StudentId == userId.Value);

            if (!canView)
                return Unauthorized("You are not authorized to view this lesson.");

            if (lesson.Type == "video")
            {
                var videoId = ExtractYouTubeId(lesson.LessonContentUrl);
                if (string.IsNullOrEmpty(videoId))
                    return BadRequest("Invalid or unsupported YouTube URL.");

                return Json(new { type = "video", title = lesson.LessonTitle, url = $"https://www.youtube.com/embed/{videoId}" });
            }
            else if (lesson.Type == "pdf")
            {
                return Json(new { type = "pdf", title = lesson.LessonTitle, url = lesson.LessonContentUrl });
            }

            return BadRequest("Unsupported lesson type.");
        }


        private string? ExtractYouTubeId(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            var regex = new Regex(@"(?:youtube\.com/(?:watch\?v=|embed/|v/|shorts/)|youtu\.be/)([A-Za-z0-9_-]{11})",
                                  RegexOptions.IgnoreCase);
            var match = regex.Match(url);
            return match.Success ? match.Groups[1].Value : null;
        }
        // GET: Lessons/Edit
        public IActionResult Edit(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Instructor")
                return Unauthorized();

            var lesson = _context.Lessons
                .Include(l => l.Module)
                .FirstOrDefault(l => l.LessonId == id);

            if (lesson == null) return NotFound();

            ViewBag.ModuleTitle = lesson.Module.ModuleTitle;
            return View(lesson);
        }


        // POST: Lessons/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Lesson lesson)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Instructor")
                return Unauthorized();

            if (ModelState.IsValid)
            {
                _context.Lessons.Update(lesson);
                _context.SaveChanges();
                return RedirectToAction("Index", new { moduleId = lesson.ModuleId });
            }

            return View(lesson);
        }
        // GET: Lessons/Delete
        public IActionResult Delete(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Instructor")
                return Unauthorized();

            var lesson = _context.Lessons
                .Include(l => l.Module)
                .FirstOrDefault(l => l.LessonId == id);

            if (lesson == null) return NotFound();

            return View(lesson);
        }

        // POST: Lessons/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Instructor")
                return Unauthorized();

            var lesson = _context.Lessons.FirstOrDefault(l => l.LessonId == id);
            if (lesson == null) return NotFound();

            var moduleId = lesson.ModuleId;
            _context.Lessons.Remove(lesson);
            _context.SaveChanges();

            return RedirectToAction("Index", new { moduleId });
        }

    }
}
