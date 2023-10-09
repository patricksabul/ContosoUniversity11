﻿using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace ContosoUniversity.Controllers
{
    public class InstructorsController : Controller
    {
        private readonly SchoolContext _context;
        public InstructorsController(SchoolContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int? id, int? courseID)
        {
            var vm = new InstructorIndexData();
            vm.Instructors = await _context.Instructors
                .Include(i => i.OfficeAssignment).Include(i => i.CourseAssignments).ThenInclude(i => i.Course).ThenInclude(i => i.Enrollments).ThenInclude(i => i.Student).Include(i => i.CourseAssignments).ThenInclude(i => i.Course).ThenInclude(i => i.Department).AsNoTracking().OrderBy(i => i.LastName).ToListAsync();
            if (id != null)
            {
                ViewData["InstructorID"] = id.Value;
                Instructor instructor = vm.Instructors
                    .Where(i => i.ID == id.Value).Single();
                vm.Courses = instructor.CourseAssignments
                    .Select(i => i.Course);
            }
            if (courseID != null)
            {
                ViewData["CourseID"] = courseID.Value;
                vm.Enrollments = vm.Courses
                    .Where(x => x.CourseID == courseID)
                    .Single()
                    .Enrollments;
            }
            return View(vm);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instructor == null)
            {
                return NotFound();
            }
            return View(instructor);
        }
        [HttpGet]
        public IActionResult Create()
        {
            var instructor = new Instructor();
            instructor.CourseAssignments = new List<CourseAssignment>();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(Instructor instructor, string selectedCourses)
        //{
        //    if (selectedCourses == null)
        //    {
        //        instructor.CourseAssignments = new List<CourseAssignment>();
        //        foreach (var course in selectedCourses)
        //        {
        //            var courseToAdd = new CourseAssignment
        //            {
        //                InstructorID = instructor.ID,
        //                CourseID = course
        //            };
        //            instructor.CourseAssignments.Add(courseToAdd);
        //        }
        //    }
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(instructor);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    PopulateAssignedCourseData(instructor);
        //    return View(instructor);
        //}
        public async Task<IActionResult> Create([Bind("HireDate,FirstMidName,LastName")] Instructor instructor)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(instructor);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes" +
                    "Try again, and if the problem persists" +
                    "see your system administrator");
            }
            return View(instructor);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var Instructor = await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                .ThenInclude(i => i.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (Instructor == null)
            {
                return NotFound();
            }
            PopulateAssignedCourseData(Instructor);
            return View(Instructor);
        }
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var instructorToUpdate = await _context.Instructors.FirstOrDefaultAsync(s => s.ID == id);
            if (await TryUpdateModelAsync<Instructor>(instructorToUpdate, "",
                s => s.FirstMidName,
                s => s.LastName,
                s => s.HireDate,
                s => s.OfficeAssignment))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes" +
                    "Try again, and if the problem persists" +
                    "see your system administrator");
                }
            }
            return View(instructorToUpdate);
        }
        //public async Task<IActionResult> Edit(int? id, string[] selectedCourses)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }
        //    var instructorToUpdate = await _context.Instructors
        //        .Include(i => i.OfficeAssignment)
        //        .Include(i => i.CourseAssignments)
        //        .ThenInclude(i => i.Course)
        //        .FirstOrDefaultAsync(s => s.ID == id);
        //    if (await TryUpdateModelAsync<Instructor>(instructorToUpdate, "",
        //        i => i.FirstMidName,
        //        i => i.LastName,
        //        i => i.HireDate,
        //        i => i.OfficeAssignment))
        //    {
        //        if (string.IsNullOrWhiteSpace(instructorToUpdate.OfficeAssignment?.Location))
        //        {
        //            instructorToUpdate.OfficeAssignment = null;
        //        }
        //        UpdateInstructorCourses(selectedCourses, instructorToUpdate);
        //        try
        //        {
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateException)
        //        {
        //            ModelState.AddModelError("", "Unable to save changes. " +
        //                "Try Again, and if the problem persists, " +
        //                "see your local computermonkey.");
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    UpdateInstructorCourses(selectedCourses, instructorToUpdate);
        //    PopulateAssignedCourseData(instructorToUpdate);

        //    return View();
        //}

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instructor == null)
            {
                return NotFound();
            }
            return View(instructor);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    Instructor instructor = await _context.Instructors
        //        .Include(i => i.CourseAssignments)
        //        .SingleAsync(i => i.ID == id);
        //    var departments = await _context.Departments
        //        .Where(d => d.InstructorID == null)
        //        .ToListAsync();
        //    departments.ForEach(d => d.InstructorID = null);

        //    _context.Instructors.Remove(instructor);

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var instructor = await _context.Instructors.FindAsync(id);
            if (instructor == null)
            {
                return RedirectToAction(nameof(Index));
            }
            try
            {
                _context.Instructors.Remove(instructor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }
        }

        private void UpdateInstructorCourses(string[] selectedCourses, Instructor instructorToUpdate)
        {
            if (selectedCourses == null)
            {
                instructorToUpdate.CourseAssignments = new List<CourseAssignment>();
                return;
            }
            var selectedCoursesHS = new HashSet<string>(selectedCourses);
            var instructorCourses = new HashSet<int>(instructorToUpdate.CourseAssignments.Select(c => c.CourseID));
            foreach (var course in _context.Courses)
            {
                if (selectedCoursesHS.Contains(course.CourseID.ToString()))
                {
                    if (!instructorCourses.Contains(course.CourseID))
                    {
                        instructorToUpdate.CourseAssignments.Add(new CourseAssignment { InstructorID = instructorToUpdate.ID, CourseID = course.CourseID });
                    }
                    else
                    {
                        if (instructorCourses.Contains(course.CourseID))
                        {
                            CourseAssignment courseToRemove = instructorToUpdate.CourseAssignments.FirstOrDefault(c => c.CourseID == course.CourseID);
                            _context.Remove(courseToRemove);
                        }
                    }
                }
            }
        }

        private void PopulateAssignedCourseData(Instructor instructor)
        {
            var allCourses = _context.Courses;
            var instructorCourses = new HashSet<int>(instructor.CourseAssignments.Select(c => c.CourseID));
            var vm = new List<AssignedCourseData>();
            foreach (var course in allCourses)
            {
                vm.Add(new AssignedCourseData
                {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    Assigned = instructorCourses.Contains(course.CourseID)
                });
            }
            ViewData["Courses"] = vm;
        }

    }
}