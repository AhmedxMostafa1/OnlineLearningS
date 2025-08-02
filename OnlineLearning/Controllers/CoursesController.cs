using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineLearning.Models;

namespace OnlineLearning.Controllers
{
    public class CoursesController : Controller
    {
        private readonly OnlineLearningContext _context;

        public CoursesController(OnlineLearningContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchQuery, int? categoryId)
        {
            var courses = _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Category)
                .Include(c => c.Enrollments)
                
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                courses = courses.Where(c =>
                    c.CourseTitle.Contains(searchQuery) ||
                    c.Category.CategName.Contains(searchQuery));
            }

            if (categoryId.HasValue)
            {
                courses = courses.Where(c => c.CategoryId == categoryId.Value);
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(await courses.ToListAsync());
        }


        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Enrollments)
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Lessons)
                .FirstOrDefaultAsync(m => m.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            ViewBag.InstructorId = new SelectList(_context.Instructors, "InstId", "InstFullName");
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategId", "CategName");
            return View();
        }


        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseId,CourseTitle,CourseDescription,InstructorId,CategoryId,IsPremium,CreatedAt")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategId", "CategId", course.CategoryId);
            ViewData["InstructorId"] = new SelectList(_context.Instructors, "InstId", "InstId", course.InstructorId);
            return View(course);
        }

        // GET: Courses/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var course = _context.Courses.Find(id);
            if (course == null) return NotFound();

            ViewBag.InstructorId = new SelectList(_context.Instructors, "InstId", "InstFullName", course.InstructorId);
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategId", "CategName", course.CategoryId);

            return View(course);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CourseId,CourseTitle,CourseDescription,InstructorId,CategoryId,IsPremium,CreatedAt")] Course course)
        {
            if (id != course.CourseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.CourseId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.InstructorId = new SelectList(_context.Instructors, "InstId", "InstFullName", course.InstructorId);
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoId", "CategName", course.CategoryId);
            return View(course);
        }

        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(m => m.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }

        public IActionResult MyCourses()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (role == "Instructor")
            {
                var instructorCourses = _context.Courses
                    .Where(c => c.InstructorId == userId)
                    .Include(c => c.Category)
                    .ToList();

                return View("MyCourses", instructorCourses);
            }
            else if (role == "Student")
            {
                var enrolledCourses = _context.Enrollments
                    .Where(e => e.StudentId == userId)
                    .Include(e => e.Course)                     // Include Course first
                        .ThenInclude(c => c.Category)           // Then include Category
                    .Select(e => e.Course)                      // Now project Course
                    .ToList();

                return View("MyCourses", enrolledCourses);
            }


            return RedirectToAction("Index", "Home"); // Fallback
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var studentId = HttpContext.Session.GetInt32("UserId");

            if (role != "Student" || studentId == null)
            {
                TempData["Alert"] = "You must be a logged-in student to enroll.";
                return RedirectToAction("Details", new { id = courseId });
            }

            var course = await _context.Courses
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
                return NotFound();

            bool alreadyEnrolled = course.Enrollments.Any(e => e.StudentId == studentId.Value);
            if (alreadyEnrolled)
            {
                TempData["Message"] = "You are already enrolled in this course.";
                return RedirectToAction("Details", new { id = courseId });
            }

            if (course.IsPremium == true)
            {
                TempData["Alert"] = "This is a premium course. Please complete payment before enrolling.";
                return RedirectToAction("Details", new { id = courseId });
            }

            var enrollment = new Enrollment
            {
                CourseId = courseId,
                StudentId = studentId.Value,
                // EnrollDate and CompletionStatus rely on DB defaults if configured
            };
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Successfully enrolled in the course.";
            return RedirectToAction("MyCourses");
        }
    }
}
