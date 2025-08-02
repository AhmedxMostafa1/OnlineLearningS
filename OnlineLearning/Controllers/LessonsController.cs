using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearning.Models;

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
    }
}
