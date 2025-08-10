using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLearning.Models;
using Microsoft.AspNetCore.Http;
namespace OnlineLearning.Controllers
{
    public class QuizzesController : Controller
    {
        private readonly OnlineLearningContext _context;

        public QuizzesController(OnlineLearningContext context)
        {
            _context = context;
        }

        // GET: Quizzes for a module
        public IActionResult Index(int moduleId)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Instructor")
            {
                return Unauthorized();
            }

            var module = _context.Modules
                .Include(m => m.Quizzes)
                .FirstOrDefault(m => m.ModuleId == moduleId);

            if (module == null) return NotFound();

            ViewBag.ModuleTitle = module.ModuleTitle;
            ViewBag.ModuleId = module.ModuleId;
            return View(module.Quizzes);
        }

        // GET: Create Quiz
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

            return View(new Quiz { ModuleId = moduleId });
        }

        // POST: Create Quiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Question,OptionA,OptionB,OptionC,OptionD,CorrectOption,ModuleId")] Quiz quiz)
        {
            ViewBag.ModuleId = quiz.ModuleId;

            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Instructor")
            {
                return Unauthorized();
            }

            // Question validation
            if (string.IsNullOrWhiteSpace(quiz.Question))
            {
                ModelState.AddModelError("Question", "Question is required.");
            }

            // Options validation
            if (string.IsNullOrWhiteSpace(quiz.OptionA))
            {
                ModelState.AddModelError("OptionA", "Option A is required.");
            }
            if (string.IsNullOrWhiteSpace(quiz.OptionB))
            {
                ModelState.AddModelError("OptionB", "Option B is required.");
            }
            if (string.IsNullOrWhiteSpace(quiz.OptionC))
            {
                ModelState.AddModelError("OptionC", "Option C is required.");
            }
            if (string.IsNullOrWhiteSpace(quiz.OptionD))
            {
                ModelState.AddModelError("OptionD", "Option D is required.");
            }

            // Correct option validation
            if (string.IsNullOrWhiteSpace(quiz.CorrectOption))
            {
                ModelState.AddModelError("CorrectOption", "Correct option is required.");
            }
            else if (!new[] { "A", "B", "C", "D" }.Contains(quiz.CorrectOption.ToUpper()))
            {
                ModelState.AddModelError("CorrectOption", "Correct option must be A, B, C, or D.");
            }

            // If validation fails, return the form with errors
            if (!ModelState.IsValid)
            {
                var module = _context.Modules.Find(quiz.ModuleId);
                ViewBag.ModuleTitle = module?.ModuleTitle;
                return View(quiz);
            }

            // Normalize correct option to uppercase
            quiz.CorrectOption = quiz.CorrectOption.ToUpper();

            // Save quiz if valid
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { moduleId = quiz.ModuleId });
        }

        // GET: Take Quiz (for students)
        [HttpGet]
        public async Task<IActionResult> TakeQuiz(int moduleId)
        {
            try
            {
                var role = HttpContext.Session.GetString("UserRole");
                var userId = HttpContext.Session.GetInt32("UserId");

                if (role != "Student" || !userId.HasValue)
                {
                    return Json(new { error = "Only enrolled students can take quizzes." });
                }

                var module = await _context.Modules
                    .Include(m => m.Course)
                        .ThenInclude(c => c.Enrollments)
                    .Include(m => m.Quizzes)
                    .FirstOrDefaultAsync(m => m.ModuleId == moduleId);

                if (module == null)
                    return Json(new { error = "Module not found." });

                // Check if student is enrolled in the course
                bool isEnrolled = module.Course.Enrollments.Any(e => e.StudentId == userId.Value);
                if (!isEnrolled)
                    return Json(new { error = "You must be enrolled in this course to take the quiz." });

                if (!module.Quizzes.Any())
                {
                    return Content(@"
                        <div class='alert alert-info'>
                            <h5><i class='bi bi-info-circle'></i> No Quiz Available</h5>
                            <p>There is no quiz available for this module yet. Please check back later.</p>
                        </div>
                    ");
                }

                ViewBag.ModuleTitle = module.ModuleTitle;
                ViewBag.ModuleId = moduleId;

                // Return partial view for AJAX loading
                return PartialView("_TakeQuizPartial", module.Quizzes.ToList());
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error in TakeQuiz: {ex.Message}");
                return Json(new { error = "An error occurred while loading the quiz. Please try again." });
            }
        }

        // POST: Submit Quiz (for students)
        // POST: Submit Quiz (for students) - DEBUG VERSION
        // POST: Submit Quiz (for students)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuiz(int moduleId, IFormCollection form)
        {
            try
            {
                var role = HttpContext.Session.GetString("UserRole");
                var userId = HttpContext.Session.GetInt32("UserId");

                if (role != "Student" || !userId.HasValue)
                {
                    return Json(new { error = "Only enrolled students can submit quizzes." });
                }

                var module = await _context.Modules
                    .Include(m => m.Course)
                        .ThenInclude(c => c.Enrollments)
                    .Include(m => m.Quizzes)
                    .FirstOrDefaultAsync(m => m.ModuleId == moduleId);

                if (module == null)
                    return Json(new { error = "Module not found." });

                // Check if student is enrolled
                bool isEnrolled = module.Course.Enrollments.Any(e => e.StudentId == userId.Value);
                if (!isEnrolled)
                    return Json(new { error = "You must be enrolled in this course." });

                var quizzes = module.Quizzes.ToList();
                int correctAnswers = 0;
                var results = new List<object>();

                // Parse answers from form collection - handle the current naming convention
                var answers = new Dictionary<int, string>();

                foreach (var key in form.Keys)
                {
                    // Handle the current naming convention: answers[1].Value
                    if (key.StartsWith("answers[") && key.EndsWith("].Value"))
                    {
                        // Extract quiz ID from key like "answers[1].Value"
                        var startIndex = 8; // Length of "answers["
                        var endIndex = key.IndexOf("].Value");

                        if (endIndex > startIndex)
                        {
                            var quizIdStr = key.Substring(startIndex, endIndex - startIndex);
                            Console.WriteLine($"Extracted quiz ID string: '{quizIdStr}'");

                            if (int.TryParse(quizIdStr, out int quizId))
                            {
                                answers[quizId] = form[key].ToString();
                                Console.WriteLine($"Added answer: Quiz {quizId} = {answers[quizId]}");
                            }
                        }
                    }
                    // Also handle the new naming convention if used: quiz_1
                    else if (key.StartsWith("quiz_"))
                    {
                        var quizIdStr = key.Substring(5);
                        if (int.TryParse(quizIdStr, out int quizId))
                        {
                            answers[quizId] = form[key].ToString();
                        }
                    }
                }

                Console.WriteLine($"Total answers parsed: {answers.Count}");

                foreach (var quiz in quizzes)
                {
                    string userAnswer = answers.ContainsKey(quiz.QuizId) ? answers[quiz.QuizId] : "";
                    bool isCorrect = !string.IsNullOrEmpty(userAnswer) &&
                                   userAnswer.ToUpper() == quiz.CorrectOption?.ToUpper();

                    if (isCorrect) correctAnswers++;

                    results.Add(new
                    {
                        Question = quiz.Question ?? "",
                        UserAnswer = userAnswer,
                        CorrectAnswer = quiz.CorrectOption ?? "",
                        IsCorrect = isCorrect,
                        OptionA = quiz.OptionA ?? "",
                        OptionB = quiz.OptionB ?? "",
                        OptionC = quiz.OptionC ?? "",
                        OptionD = quiz.OptionD ?? ""
                    });
                }

                double percentage = quizzes.Count > 0 ? (double)correctAnswers / quizzes.Count * 100 : 0;

                ViewBag.ModuleTitle = module.ModuleTitle;
                ViewBag.TotalQuestions = quizzes.Count;
                ViewBag.CorrectAnswers = correctAnswers;
                ViewBag.Percentage = percentage;
                ViewBag.Results = results;
                ViewBag.ModuleId = moduleId;

                return PartialView("_QuizResultsPartial");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SubmitQuiz: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { error = $"An error occurred: {ex.Message}" });
            }
        }



        // GET: Edit Quiz
        public IActionResult Edit(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Instructor")
                return Unauthorized();

            var quiz = _context.Quizzes
                .Include(q => q.Module)
                .FirstOrDefault(q => q.QuizId == id);

            if (quiz == null) return NotFound();

            ViewBag.ModuleTitle = quiz.Module?.ModuleTitle;
            return View(quiz);
        }

        // POST: Edit Quiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("QuizId,Question,OptionA,OptionB,OptionC,OptionD,CorrectOption,ModuleId")] Quiz quiz)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Instructor")
            {
                return Unauthorized();
            }

            // Question validation
            if (string.IsNullOrWhiteSpace(quiz.Question))
            {
                ModelState.AddModelError("Question", "Question is required.");
            }

            // Options validation
            if (string.IsNullOrWhiteSpace(quiz.OptionA))
            {
                ModelState.AddModelError("OptionA", "Option A is required.");
            }
            if (string.IsNullOrWhiteSpace(quiz.OptionB))
            {
                ModelState.AddModelError("OptionB", "Option B is required.");
            }
            if (string.IsNullOrWhiteSpace(quiz.OptionC))
            {
                ModelState.AddModelError("OptionC", "Option C is required.");
            }
            if (string.IsNullOrWhiteSpace(quiz.OptionD))
            {
                ModelState.AddModelError("OptionD", "Option D is required.");
            }

            // Correct option validation
            if (string.IsNullOrWhiteSpace(quiz.CorrectOption))
            {
                ModelState.AddModelError("CorrectOption", "Correct option is required.");
            }
            else if (!new[] { "A", "B", "C", "D" }.Contains(quiz.CorrectOption.ToUpper()))
            {
                ModelState.AddModelError("CorrectOption", "Correct option must be A, B, C, or D.");
            }

            if (!ModelState.IsValid)
            {
                var module = _context.Modules.Find(quiz.ModuleId);
                ViewBag.ModuleTitle = module?.ModuleTitle;
                return View(quiz);
            }

            // Normalize correct option to uppercase
            quiz.CorrectOption = quiz.CorrectOption.ToUpper();

            _context.Quizzes.Update(quiz);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { moduleId = quiz.ModuleId });
        }

        // GET: Delete Quiz
        public IActionResult Delete(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Instructor")
                return Unauthorized();

            var quiz = _context.Quizzes
                .Include(q => q.Module)
                .FirstOrDefault(q => q.QuizId == id);

            if (quiz == null) return NotFound();

            return View(quiz);
        }

        // POST: Delete Quiz
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Instructor")
                return Unauthorized();

            var quiz = _context.Quizzes.FirstOrDefault(q => q.QuizId == id);
            if (quiz == null) return NotFound();

            var moduleId = quiz.ModuleId;
            _context.Quizzes.Remove(quiz);
            _context.SaveChanges();

            return RedirectToAction("Index", new { moduleId });
        }
    }
}