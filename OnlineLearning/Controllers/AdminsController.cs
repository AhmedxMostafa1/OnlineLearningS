using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearning.Models;
using System.Linq;

public class AdminsController : Controller
{
    private readonly OnlineLearningContext _context;

    public AdminsController(OnlineLearningContext context)
    {
        _context = context;
    }

    public IActionResult Dashboard(string searchBy, string searchTerm, string userType, string section = "summary")
    {
        var students = _context.Students.ToList();
        var instructors = _context.Instructors.ToList();
        var courses = _context.Courses.ToList();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            if (userType == "Student")
            {
                students = students.Where(s =>
                    (searchBy == "Id" && s.StuId.ToString() == searchTerm) ||
                    (searchBy == "Name" && s.StuFullName.ToLower().Contains(searchTerm.ToLower()))
                ).ToList();
            }
            else if (userType == "Instructor")
            {
                instructors = instructors.Where(i =>
                    (searchBy == "Id" && i.InstId.ToString() == searchTerm) ||
                    (searchBy == "Name" && i.InstFullName.ToLower().Contains(searchTerm.ToLower()))
                ).ToList();
            }
        }

        ViewBag.Section = section;
        ViewBag.UserType = userType;
        ViewBag.Students = students;
        ViewBag.Instructors = instructors;
        ViewBag.TotalStudents = _context.Students.Count();
        ViewBag.TotalInstructors = _context.Instructors.Count();
        ViewBag.TotalCourses = _context.Courses.Count();

        return View();
    }
    public IActionResult StudentDetails(int id)
    {
        var student = _context.Students.Find(id);
        if (student == null) return NotFound();

        var enrollments = _context.Enrollments
            .Where(e => e.StudentId == id)
            .Select(e => new
            {
                e.EnrId,
                e.CourseId,
                Course = e.Course
            }).ToList();

        ViewBag.AllCourses = _context.Courses.ToList(); // for add option
        ViewBag.Enrollments = enrollments;
        return View(student);
    }

    public IActionResult InstructorDetails(int id)
    {
        var instructor = _context.Instructors.Find(id);
        if (instructor == null) return NotFound();

        var courses = _context.Courses
            .Where(c => c.InstructorId == id)
            .ToList();

        ViewBag.AllCourses = _context.Courses.ToList(); // for assigning new courses
        ViewBag.Courses = courses;
        return View(instructor);
    }



    [HttpPost]
    public IActionResult RemoveEnrollment(int enrId, int studentId)
    {
        var enrollment = _context.Enrollments.Find(enrId);
        if (enrollment != null)
        {
            _context.Enrollments.Remove(enrollment);
            _context.SaveChanges();
        }
        return RedirectToAction("StudentDetails", new { id = studentId });
    }

    [HttpPost]
    public IActionResult DeleteStudent(int id)
    {
        var student = _context.Students.Find(id);
        if (student != null)
        {
            _context.Students.Remove(student);
            _context.SaveChanges();
        }
        return RedirectToAction("Dashboard");
    }

    [HttpPost]
    public IActionResult DeleteInstructor(int id)
    {
        var instructor = _context.Instructors.Find(id);
        if (instructor != null)
        {
            _context.Instructors.Remove(instructor);
            _context.SaveChanges();
        }
        return RedirectToAction("Dashboard");
    }


}
