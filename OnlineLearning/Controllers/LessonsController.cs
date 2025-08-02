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
        public async Task<IActionResult> WatchVideo(int lessonId)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");

            var lesson = await _context.Lessons
                .Include(l => l.Module)
                    .ThenInclude(m => m.Course)
                        .ThenInclude(c => c.Enrollments)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

            if (lesson == null)
                return NotFound();

            var course = lesson.Module?.Course;
            if (course == null)
                return BadRequest("Associated course not found.");

            // Authorization: instructor of the course or enrolled student
            bool canView = false;
            if (role == "Instructor" && course.InstructorId == userId)
            {
                canView = true;
            }
            else if (role == "Student" && userId.HasValue)
            {
                canView = course.Enrollments.Any(e => e.StudentId == userId.Value);
            }

            if (!canView)
            {
                TempData["Alert"] = "You are not authorized to view this video.";
                return RedirectToAction("Details", "Courses", new { id = course.CourseId });
            }

            if (!string.Equals(lesson.Type, "video", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Alert"] = "Requested lesson is not a video.";
                return RedirectToAction("Details", "Courses", new { id = course.CourseId });
            }

            var videoId = ExtractYouTubeId(lesson.LessonContentUrl);
            if (string.IsNullOrEmpty(videoId))
            {
                TempData["Alert"] = "Invalid or unsupported YouTube URL.";
                return RedirectToAction("Details", "Courses", new { id = course.CourseId });
            }

            ViewBag.EmbedUrl = $"https://www.youtube.com/embed/{videoId}";
            ViewBag.CourseId = course.CourseId;
            ViewBag.CanView = true;

            return View(lesson);
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
    }
}
