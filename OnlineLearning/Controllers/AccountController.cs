using Microsoft.AspNetCore.Mvc;
using OnlineLearning.Models;

public class AccountController : Controller
{
    private readonly OnlineLearningContext _context;

    public AccountController(OnlineLearningContext context)
    {
        _context = context;
    }

    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        // Check Student
        var student = _context.Students.FirstOrDefault(s => s.StuEmail == email && s.StuPassword == password);
        if (student != null)
        {
            HttpContext.Session.SetString("UserRole", "Student");
            HttpContext.Session.SetInt32("UserId", student.StuId);
            return RedirectToAction("Index", "Courses");
        }

        // Check Instructor
        var instructor = _context.Instructors.FirstOrDefault(i => i.InstEmail == email && i.InstPassword == password);
        if (instructor != null)
        {
            HttpContext.Session.SetString("UserRole", "Instructor");
            HttpContext.Session.SetInt32("UserId", instructor.InstId);
            return RedirectToAction("Index", "Courses");
        }

        // Check Admin
        var admin = _context.Admins.FirstOrDefault(a => a.AdminEmail == email && a.AdminPassword == password);
        if (admin != null)
        {
            HttpContext.Session.SetString("UserRole", "Admin");
            HttpContext.Session.SetInt32("UserId", admin.AdminId);
            return RedirectToAction("Dashboard", "Admin");
        }

        ViewBag.Error = "Invalid email or password.";
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
