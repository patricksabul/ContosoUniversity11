using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Controllers
{
    public class CoursesController : Controller
    {
        private readonly SchoolContext _context;
        public CoursesController(SchoolContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Courses
            .Include(c => c.Department)
            .ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "DepartmentID", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("CourseID", "Title", "Credits", "DepartmentID")] Course course)
        {
            ModelState.Remove("Courses");
            ModelState.Remove("Adminstrator");
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentID"] = new SelectList(_context.Departments, "DepartmentID", "Name", course.DepartmentID);
            return RedirectToAction(nameof(Index));

        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var course = await _context.Courses
                 .Where(course => course.CourseID == id)
                 .Include(course => course.Department)
                 .FirstOrDefaultAsync();
            return View(course);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _context.Courses
                   .Where(course => course.CourseID == id)
                   .Include(course => course.Department)
                   .FirstOrDefaultAsync();


            ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "DepartmentID", "Name");
            return View(course);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(Course course, int? departmentId)
        {
            var courseInst = await _context.Courses
                .Where(c => c.CourseID == course.CourseID)
                .Include(c => c.Department)
                .FirstOrDefaultAsync();

            courseInst.Title = course.Title;
            courseInst.Credits = course.Credits;
            courseInst.Department = await _context.Departments
                .Where(d => d.DepartmentID == departmentId)
                .FirstOrDefaultAsync();

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int? id, bool? concurrencyError)
        {
            if (id == null)
            {
                return NotFound();
            }
            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.CourseID == id);
            if (course == null)
            {
                if (concurrencyError.GetValueOrDefault())
                {
                    return RedirectToAction(nameof(Index));
                }
                return NotFound();
            }
            if (concurrencyError.GetValueOrDefault())
            {
                ViewData["ConcurrencyErrorMessage"] = "The record that you have attempted to edit"
                + "was modified by another user after you got the original value."
                + "The editing operation was cancelled and the current values in the database"
                + "Have been displayed. If you still require to eidt this record. click"
                + "the save button again. otherwise click the back to list hyperlink";
            }
            return View(course);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Course course)
        {
            try
            {
                if (await _context.Courses.AnyAsync(m => m.CourseID == course.CourseID))
                {
                    _context.Courses.Remove(course);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                return RedirectToAction(nameof(Delete),
                    new { concurrencyError = true, id = course.CourseID });
            }
            return RedirectToAction(nameof(Index));
        }
    }
}