using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string fullName, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Please fill all fields.";
            return View();
        }

        string normalizedEmail = email.Trim().ToLowerInvariant();

        bool emailExists = await _context.Students.AnyAsync(s => s.StuEmail.ToLower() == normalizedEmail)
            || await _context.Instructors.AnyAsync(i => i.InstEmail.ToLower() == normalizedEmail)
            || await _context.Admins.AnyAsync(a => a.AdminEmail.ToLower() == normalizedEmail);

        if (emailExists)
        {
            ViewBag.Error = "Email already exists.";
            return View();
        }

        // Hash student password
        string hashed = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

        var student = new Student
        {
            StuFullName = fullName,
            StuEmail = normalizedEmail,
            StuPassword = hashed,
            Status = "Active"
        };
        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        HttpContext.Session.SetString("UserRole", "Student");
        HttpContext.Session.SetInt32("UserId", student.StuId);

        return RedirectToAction("Index","Home");
    }

    private static bool IsBcryptHash(string pwd) =>
    !string.IsNullOrEmpty(pwd) &&
    (pwd.StartsWith("$2a$") || pwd.StartsWith("$2b$") || pwd.StartsWith("$2y$"));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        string normalizedEmail = email.Trim().ToLowerInvariant();

        // Student
        var student = await _context.Students.FirstOrDefaultAsync(s => s.StuEmail.ToLower() == normalizedEmail);
        if (student != null)
        {
            if (student.Status != null && student.Status.Equals("Deactivated", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "Your account is deactivated. Contact admin.";
                return View();
            }

            bool passwordMatches = false;
            if (IsBcryptHash(student.StuPassword))
            {
                passwordMatches = BCrypt.Net.BCrypt.Verify(password, student.StuPassword);
            }
            else if (student.StuPassword == password) // legacy plaintext
            {
                passwordMatches = true;
                // upgrade: hash and persist
                student.StuPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
                await _context.SaveChangesAsync();
            }

            if (passwordMatches)
            {
                HttpContext.Session.SetString("UserRole", "Student");
                HttpContext.Session.SetInt32("UserId", student.StuId);
                return RedirectToAction("Index", "Courses");
            }
        }

        // Instructor
        var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.InstEmail.ToLower() == normalizedEmail);
        if (instructor != null)
        {
            if (instructor.Status != null && instructor.Status.Equals("Deactivated", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "Your account is deactivated. Contact admin.";
                return View();
            }

            bool passwordMatches = false;
            if (IsBcryptHash(instructor.InstPassword))
            {
                passwordMatches = BCrypt.Net.BCrypt.Verify(password, instructor.InstPassword);
            }
            else if (instructor.InstPassword == password)
            {
                passwordMatches = true;
                instructor.InstPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
                await _context.SaveChangesAsync();
            }

            if (passwordMatches)
            {
                HttpContext.Session.SetString("UserRole", "Instructor");
                HttpContext.Session.SetInt32("UserId", instructor.InstId);
                return RedirectToAction("Mycourses", "Courses");
            }
        }

        // Admin
        var admin = await _context.Admins.FirstOrDefaultAsync(a => a.AdminEmail.ToLower() == normalizedEmail);
        if (admin != null)
        {
            bool passwordMatches = false;
            if (IsBcryptHash(admin.AdminPassword))
            {
                passwordMatches = BCrypt.Net.BCrypt.Verify(password, admin.AdminPassword);
            }
            else if (admin.AdminPassword == password)
            {
                passwordMatches = true;
                admin.AdminPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
                await _context.SaveChangesAsync();
            }

            if (passwordMatches)
            {
                HttpContext.Session.SetString("UserRole", "Admin");
                HttpContext.Session.SetInt32("UserId", admin.AdminId);
                return RedirectToAction("Dashboard", "Admins");
            }
        }

        ViewBag.Error = "Invalid email or password.";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> JoinUs(string fullName, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Please fill all fields.";
            return View();
        }

        string normalizedEmail = email.Trim().ToLowerInvariant();

        bool alreadyPending = await _context.PendingInstructors.AnyAsync(p => p.Email.ToLower() == normalizedEmail);
        if (alreadyPending)
        {
            ViewBag.Error = "You already have a pending request.";
            return View();
        }

        bool emailTaken = await _context.Students.AnyAsync(s => s.StuEmail.ToLower() == normalizedEmail)
                          || await _context.Instructors.AnyAsync(i => i.InstEmail.ToLower() == normalizedEmail)
                          || await _context.Admins.AnyAsync(a => a.AdminEmail.ToLower() == normalizedEmail);
        if (emailTaken)
        {
            ViewBag.Error = "Email already exists in the system.";
            return View();
        }

        string hashed = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

        var request = new PendingInstructor
        {
            FullName = fullName,
            Email = normalizedEmail,
            Password = hashed,
            AppliedAt = DateTime.UtcNow
        };

        _context.PendingInstructors.Add(request);
        await _context.SaveChangesAsync();

        return RedirectToAction("Login", "Account");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
