using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineLearning.Models;

namespace OnlineLearning.Controllers
{
    public class ModulesController : Controller
    {
        private readonly OnlineLearningContext _context;

        public ModulesController(OnlineLearningContext context)
        {
            _context = context;
        }

        // GET: Modules for a specific course
        public IActionResult Index(int courseId)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Instructor")
            {
                return Unauthorized();
            }

            var course = _context.Courses
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Lessons) // Include lessons for count
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Quizzes) // Include quizzes for count
                .FirstOrDefault(c => c.CourseId == courseId);

            if (course == null) return NotFound();

            ViewBag.CourseTitle = course.CourseTitle;
            ViewBag.CourseId = course.CourseId;
            return View(course.Modules);
        }

        // GET: Create Module
        public IActionResult Create(int courseId)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Instructor")
            {
                return Unauthorized();
            }

            var course = _context.Courses.Find(courseId);
            if (course == null) return NotFound();

            ViewBag.CourseId = courseId;
            ViewBag.CourseTitle = course.CourseTitle;
            return View(new Module { CourseId = courseId });

        }

        // POST: Create Module
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Module module)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Instructor")
            {
                return Unauthorized();
            }
            if (string.IsNullOrWhiteSpace(module.ModuleTitle))
            {
                ModelState.AddModelError("ModuleTitle", "Module title is required.");
            }
            

            if (!ModelState.IsValid)
            {
                return View(module); // Return to form with error messages
            }

            
                _context.Modules.Add(module);
                _context.SaveChanges();
                return RedirectToAction("Index", new { courseId = module.CourseId });
            

            
        }

        // GET: Edit Module
        public IActionResult Edit(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Instructor")
            {
                
                return Unauthorized();
            }

            var module = _context.Modules
                .Include(m => m.Course)
                .FirstOrDefault(m => m.ModuleId == id);

            if (module == null) return NotFound();

            ViewBag.CourseTitle = module.Course.CourseTitle;
            return View(module);
        }

        // POST: Edit Module
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Module module)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Instructor")
            {
                return Unauthorized();
            }

            if (id != module.ModuleId) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(module);
                _context.SaveChanges();
                return RedirectToAction("Index", new { courseId = module.CourseId });
            }

            ViewBag.CourseTitle = _context.Courses.Find(module.CourseId)?.CourseTitle;
            return View(module);
        }

        // GET: Delete Module
        public IActionResult Delete(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Instructor")
            {
                return Unauthorized();
            }

            var module = _context.Modules
                .Include(m => m.Course)
                .FirstOrDefault(m => m.ModuleId == id);

            if (module == null) return NotFound();

            return View(module);
        }

        // POST: Delete Module
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Instructor")
            {
                return Unauthorized();
            }

            var module = _context.Modules.Find(id);
            if (module != null)
            {
                _context.Modules.Remove(module);
                _context.SaveChanges();
                return RedirectToAction("Index", new { courseId = module.CourseId });
            }

            return NotFound();
        }
    }
}
