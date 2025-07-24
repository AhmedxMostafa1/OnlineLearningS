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

    public IActionResult Register() => View();

    public IActionResult JoinUs() => View();

    [HttpPost]
    public IActionResult Register(string fullName,string email,string password,string role)
    {
        if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fullName))
        {
            ViewBag.Error = "Please fill all fields.";
            return View();
        }

        if (role == "Student")
        {
            if (_context.Students.Any(s => s.StuEmail == email))
            {
                ViewBag.Error = "Email already exists for Student.";
                return View();
            }

            var student = new Student
            {
                StuFullName = fullName,
                StuEmail = email,
                StuPassword = password
            };
            _context.Students.Add(student);
        }
        else if (role == "Instructor")
        {
            if (_context.Instructors.Any(i => i.InstEmail == email))
            {
                ViewBag.Error = "Email already exists for Instructor.";
                return View();
            }

            var instructor = new Instructor
            {
                InstFullName = fullName,
                InstEmail = email,
                InstPassword = password
            };
            _context.Instructors.Add(instructor);
        }
        else if (role == "Admin")
        {
            if (_context.Admins.Any(a => a.AdminEmail == email))
            {
                ViewBag.Error = "Email already exists for Admin.";
                return View();
            }

            var admin = new Admin
            {
                AdminFullName = fullName,
                AdminEmail = email,
                AdminPassword = password
            };
            _context.Admins.Add(admin);
        }
        else
        {
            ViewBag.Error = "Invalid role selected.";
            return View();
        }
        _context.SaveChanges();
        TempData["SuccessMessage"] = "Registration successful. Please log in.";
        return RedirectToAction("Login");
    }

    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        // Check Student
        var student = _context.Students.FirstOrDefault(s => s.StuEmail == email && s.StuPassword == password);
        if (student != null)
        {
            if (student.Status == "Deactivated")
            {
                ViewBag.Error = "Your account is deactivated. Contact admin.";
                return View();
            }
            HttpContext.Session.SetString("UserRole", "Student");
            HttpContext.Session.SetInt32("UserId", student.StuId);
            return RedirectToAction("Index", "Courses");
        }

        // Check Instructor
        var instructor = _context.Instructors.FirstOrDefault(i => i.InstEmail == email && i.InstPassword == password);
        if (instructor != null)
        {
            if (instructor.Status == "Deactivated")
            {
                ViewBag.Error = "Your account is deactivated. Contact admin.";
                return View();
            }
            HttpContext.Session.SetString("UserRole", "Instructor");
            HttpContext.Session.SetInt32("UserId", instructor.InstId);
            return RedirectToAction("Index", "Courses");
        }

        // Check Admin
        var admin = _context.Admins.FirstOrDefault(a => a.AdminEmail == email && a.AdminPassword == password);
        if (admin != null)
        {
            HttpContext.Session.SetString("UserRole", "Admins");
            HttpContext.Session.SetInt32("UserId", admin.AdminId);
            return RedirectToAction("Dashboard", "Admins");
        }

        ViewBag.Error = "Invalid email or password.";
        return View();
    }
    [HttpPost]
    public IActionResult JoinUs(string fullName, string email, string password)
    {
        var request = new PendingInstructor
        {
            FullName = fullName,
            Email = email,
            Password = password, 
            AppliedAt = DateTime.Now
        };

        _context.PendingInstructors.Add(request);
        _context.SaveChanges();


        TempData["Message"] = "Your request has been submitted!";

        // Redirect to the login page
        return RedirectToAction("Login", "Account");
    }


    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
