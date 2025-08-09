using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearning.Models;
using OnlineLearning.Models.DTOs;

namespace OnlineLearning.Controllers
{
    public class PaymentController : Controller
    {
        private readonly OnlineLearningContext _context;

        public PaymentController(OnlineLearningContext context)
        {
            _context = context;
        }

        // GET: Payment/Checkout/{courseId}
        public async Task<IActionResult> Checkout(int courseId)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var studentId = HttpContext.Session.GetInt32("UserId");

            if (role != "Student" || studentId == null)
            {
                TempData["Alert"] = "You must be logged in as a student to make a payment.";
                return RedirectToAction("Login", "Account");
            }

            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
                return NotFound();

            // Check if already enrolled
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);

            if (existingEnrollment != null)
            {
                TempData["Message"] = "You are already enrolled in this course.";
                return RedirectToAction("Details", "Courses", new { id = courseId });
            }

            // Check if course is free
            if (course.CoursePrice <= 0)
            {
                TempData["Message"] = "This course is free. You can enroll directly.";
                return RedirectToAction("Details", "Courses", new { id = courseId });
            }

            ViewBag.Course = course;
            return View(new PaymentDto { CourseId = courseId, Amount = course.CoursePrice });
        }

        // POST: Payment/ProcessPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(PaymentDto paymentDto)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var studentId = HttpContext.Session.GetInt32("UserId");

            if (role != "Student" || studentId == null)
            {
                TempData["Alert"] = "You must be logged in as a student to make a payment.";
                return RedirectToAction("Login", "Account");
            }

            var course = await _context.Courses.FindAsync(paymentDto.CourseId);
            if (course == null)
                return NotFound();

            // Validate payment details (basic validation)
            if (string.IsNullOrWhiteSpace(paymentDto.CardNumber) ||
                string.IsNullOrWhiteSpace(paymentDto.ExpiryDate) ||
                string.IsNullOrWhiteSpace(paymentDto.CVV) ||
                string.IsNullOrWhiteSpace(paymentDto.CardHolderName))
            {
                TempData["Error"] = "Please fill in all payment details.";
                ViewBag.Course = course;
                return View("Checkout", paymentDto);
            }

            // Simulate payment processing (replace with actual payment gateway)
            bool paymentSuccess = ProcessPaymentWithGateway(paymentDto);

            if (paymentSuccess)
            {
                // Create payment record
                var payment = new Payment
                {
                    StudentId = studentId.Value,
                    CourseId = paymentDto.CourseId,
                    PayAmount = paymentDto.Amount,
                    PaymentDate = DateTime.UtcNow,
                    PaymentMethod = paymentDto.PaymentMethod,
                    PaymentStatus = "Completed"
                };
                _context.Payments.Add(payment);

                // Create enrollment with completed payment status
                var enrollment = new Enrollment
                {
                    StudentId = studentId.Value,
                    CourseId = paymentDto.CourseId,
                    EnrollDate = DateTime.UtcNow,
                    PaymentStatus = "Completed",
                    CompletionStatus = false
                };
                _context.Enrollments.Add(enrollment);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Payment successful! You are now enrolled in the course.";
                return RedirectToAction("MyCourses", "Courses");
            }
            else
            {
                // Create failed payment record for tracking
                var failedPayment = new Payment
                {
                    StudentId = studentId.Value,
                    CourseId = paymentDto.CourseId,
                    PayAmount = paymentDto.Amount,
                    PaymentDate = DateTime.UtcNow,
                    PaymentMethod = paymentDto.PaymentMethod,
                    PaymentStatus = "Failed"
                };
                _context.Payments.Add(failedPayment);
                await _context.SaveChangesAsync();

                TempData["Error"] = "Payment failed. Please check your payment details and try again.";
                ViewBag.Course = course;
                return View("Checkout", paymentDto);
            }
        }

        // Mock payment processing - replace with actual payment gateway integration
        private bool ProcessPaymentWithGateway(PaymentDto paymentDto)
        {
            // Simulate payment processing
            // In real implementation, integrate with Stripe, PayPal, etc.

            // For demo purposes, let's say payment fails if card number starts with "4000"
            if (paymentDto.CardNumber.StartsWith("4000"))
                return false;

            // Simulate processing delay
            Thread.Sleep(1000);

            // Return success for other card numbers
            return true;
        }

        // GET: Payment/Success
        public IActionResult Success()
        {
            return View();
        }

        // GET: Payment/Failed
        public IActionResult Failed()
        {
            return View();
        }
    }
}