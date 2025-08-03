using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearning.Models;

public class AdminsController : Controller
{
    private readonly OnlineLearningContext _context;

    public AdminsController(OnlineLearningContext context)
    {
        _context = context;
    }
    private IActionResult? EnsureAdmin()
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
        return null;
    }

    // Dashboard with optional filtering and section selection
    public async Task<IActionResult> Dashboard(string searchBy, string searchTerm, string userType, string section = "summary")
    {   //check if admin
        var guard = EnsureAdmin();
        if (guard != null) return guard;

        // Base queries (deferred execution)
        IQueryable<Student> studentQuery = _context.Students;
        IQueryable<Instructor> instructorQuery = _context.Instructors;

        // Search filtering
        if (!string.IsNullOrWhiteSpace(searchTerm) && !string.IsNullOrWhiteSpace(userType))
        {
            string term = searchTerm.Trim();
            if (userType.Equals("Student", StringComparison.OrdinalIgnoreCase))
            {
                if (searchBy == "Id" && int.TryParse(term, out int sid))
                {
                    studentQuery = studentQuery.Where(s => s.StuId == sid);
                }
                else if (searchBy == "Name")
                {
                    studentQuery = studentQuery.Where(s => EF.Functions.Like(s.StuFullName, $"%{term}%"));
                }
            }
            else if (userType.Equals("Instructor", StringComparison.OrdinalIgnoreCase))
            {
                if (searchBy == "Id" && int.TryParse(term, out int iid))
                {
                    instructorQuery = instructorQuery.Where(i => i.InstId == iid);
                }
                else if (searchBy == "Name")
                {
                    instructorQuery = instructorQuery.Where(i => EF.Functions.Like(i.InstFullName, $"%{term}%"));
                }
            }
        }

        // Materialize only what’s needed
        var students = await studentQuery.ToListAsync();
        var instructors = await instructorQuery.ToListAsync();

        // Section-specific data
        if (section == "requests")
        {
            var pending = await _context.PendingInstructors.ToListAsync();
            ViewBag.PendingInstructors = pending;
        }

        ViewBag.Section = section;
        ViewBag.UserType = userType;
        ViewBag.Students = students;
        ViewBag.Instructors = instructors;

        // Summary counts (could be optimized/cached if scale demands)
        ViewBag.TotalStudents = await _context.Students.CountAsync();
        ViewBag.TotalInstructors = await _context.Instructors.CountAsync();
        ViewBag.TotalCourses = await _context.Courses.CountAsync();

        return View();
    }

    public async Task<IActionResult> StudentDetails(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null) return NotFound();

        var enrollments = await _context.Enrollments
            .Where(e => e.StudentId == id)
            .Select(e => new
            {
                e.EnrId,
                e.CourseId,
                Course = e.Course
            })
            .ToListAsync();

        ViewBag.AllCourses = await _context.Courses.ToListAsync();
        ViewBag.Enrollments = enrollments;
        return View(student);
    }

    public async Task<IActionResult> InstructorDetails(int id)
    {
        var instructor = await _context.Instructors.FindAsync(id);
        if (instructor == null) return NotFound();

        var courses = await _context.Courses
            .Where(c => c.InstructorId == id)
            .ToListAsync();

        ViewBag.AllCourses = await _context.Courses.ToListAsync();
        ViewBag.Courses = courses;
        return View(instructor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveEnrollment(int enrId, int studentId)
    {
        var enrollment = await _context.Enrollments.FindAsync(enrId);
        if (enrollment != null)
        {
            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(StudentDetails), new { id = studentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveInstructor(int id)
    {
        var pending = await _context.PendingInstructors.FindAsync(id);
        if (pending != null)
        {
            string normalizedEmail = pending.Email?.Trim().ToLowerInvariant() ?? "";

            bool emailConflict = await _context.Students.AnyAsync(s => s.StuEmail.ToLower() == normalizedEmail)
                                 || await _context.Instructors.AnyAsync(i => i.InstEmail.ToLower() == normalizedEmail)
                                 || await _context.Admins.AnyAsync(a => a.AdminEmail.ToLower() == normalizedEmail);

            if (emailConflict)
            {
                TempData["Error"] = "Email already exists in the system.";
                return RedirectToAction(nameof(Dashboard), new { section = "requests" });
            }

            var instructor = new Instructor
            {
                InstFullName = pending.FullName,
                InstEmail = normalizedEmail,
                InstPassword = pending.Password, // already hashed
                Status = "Activated"
            };

            _context.Instructors.Add(instructor);
            _context.PendingInstructors.Remove(pending);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Instructor approved.";
        }

        return RedirectToAction(nameof(Dashboard), new { section = "requests" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectInstructor(int id)
    {
        var pending = await _context.PendingInstructors.FindAsync(id);
        if (pending != null)
        {
            _context.PendingInstructors.Remove(pending);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Instructor request rejected.";
        }

        return RedirectToAction(nameof(Dashboard), new { section = "requests" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleInstructorStatus(int id)
    {
        var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.InstId == id);
        if (instructor != null)
        {
            // Normalize to use consistent status values; here using "Active"/"Deactivated"
            instructor.Status = string.Equals(instructor.Status, "Activated", StringComparison.OrdinalIgnoreCase)
                ? "Deactivated"
                : "Activated";
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Dashboard), new { section = "instructors" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStudentStatus(int id)
    {
        var student = await _context.Students.FirstOrDefaultAsync(s => s.StuId == id);
        if (student != null)
        {
            student.Status = string.Equals(student.Status, "Activated", StringComparison.OrdinalIgnoreCase)
                ? "Deactivated"
                : "Activated";
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Dashboard), new { section = "students" });
    }
}
