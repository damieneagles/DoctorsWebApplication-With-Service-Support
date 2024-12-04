using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoctorsWebApplication.Data;
using DoctorsWebApplication.Models;
using DoctorsWebApplication.Search;
using Microsoft.Extensions.Caching.Distributed;
using System.Linq.Dynamic.Core;
using X.PagedList;
using X.PagedList.Extensions;

namespace DoctorsWebApplication.Controllers
{
    public class PatientsController : Controller
    {
        private readonly DoctorsDatabase2023Context _context;
        private readonly PageSearchData _searchData;

        public PatientsController(DoctorsDatabase2023Context context, IDistributedCache cache)
        {
            _context = context;
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(nameof(_context.Patients));
            byte[] hashBytes = System.Security.Cryptography.MD5.HashData(inputBytes);
            var uniquePageName = Convert.ToHexString(hashBytes);
            _searchData = new PageSearchData(cache, uniquePageName, Guid.NewGuid());
        }

        private void BuildLists(int? selectedDoctorId)
        {
            ViewData["DoctorList"] = new SelectList(_context.Doctors.Include(c => c.Person), "DoctorId", "Person.CombinedName", selectedDoctorId);
        }

        // GET: Patients
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task<IActionResult> Index([Bind("SearchString,CheckCurrent,SortOrder")] string sortOrder, string currentFilter, string? searchString, int? page, bool? checkCurrent)
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            //Set page of pager
            //Page Number
            page ??= _searchData.PageIndex ?? 1;

            //Get current records
            _searchData.CheckCurrent = checkCurrent ?? true;

            var patients = (from p in _context.Patients.Include(c => c.Person).Include(c => c.Doctor).Include(c => c.Doctor.Person)
                            select p).ToList();

            //Initial navigation to page OR Back to list has been clicked from a subsequent page
            searchString ??= _searchData.SearchString ?? string.Empty;

            string sortDirection = _searchData.SortDirection;
            if (!String.IsNullOrEmpty(searchString))
            {
                patients = (from p in patients
                                       .Where(q => q.Person.CombinedName.Contains(searchString) ||
                                                   q.TagId.Contains(searchString))
                            select p).ToList();

                //New Search so back to defaults
                if (searchString != _searchData.CurrentFilter)
                {
                    //Back to search start
                    sortOrder = "Person.CombinedName";
                    sortDirection = "asc";
                    page = 1;
                    _searchData.CurrentFilter = string.Empty;
                }
                else
                {
                    _searchData.SearchString = searchString;
                    //Set current search term
                    _searchData.CurrentFilter = searchString ?? string.Empty;
                }
            }

            if (sortOrder == null && sortDirection == null)
            {
                //Search start
                sortOrder = "Person.CombinedName";
                sortDirection = "asc";
                page = 1;
            }
            else if (sortOrder == null && sortDirection != null)
            {
                //Back to list has been clicked from another page
                sortOrder = _searchData.SortOrder;
                sortDirection = _searchData.SortDirection;
            }
            else if (sortOrder != _searchData.SortOrder && _searchData.PageIndex == page)
            {
                //New column sort
                //Toggle sort direction
                sortDirection = (_searchData.SortDirection.Contains("asc") ? "desc" : "asc");
                //Set new column
                _searchData.SortOrder = sortOrder ?? string.Empty;
            }
            else if (sortOrder == _searchData.SortOrder && _searchData.PageIndex == page)
            {
                //Same column so just change the sort direction
                sortDirection = (_searchData.SortDirection.Contains("asc") ? "desc" : "asc");
                sortOrder = _searchData.SortOrder;
            }
            else if (_searchData.PageIndex != page)
            {
                //Different page so use sortorder and direction as it was
                sortDirection = _searchData.SortDirection;
                sortOrder = _searchData.SortOrder;
            }

            //Sort the records
#pragma warning disable CS8604 // Possible null reference argument.
            patients = patients.AsQueryable<Patient>().Sort(sortOrder, sortDirection).ToDynamicList<Patient>();
#pragma warning restore CS8604 // Possible null reference argument.

            //save previous sort criteria
            _searchData.SortDirection = sortDirection;
            _searchData.SortOrder = sortOrder;
            _searchData.PageIndex = page;
            _searchData.CheckCurrent = checkCurrent ?? true;

            int pageSize = 5;
            int pageNumber = (_searchData.PageIndex ?? 1);
            return View(new PageMetaDataModel<Patient>(patients.ToPagedList(pageNumber, pageSize), _searchData));
        }

        // GET: Patients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Patients == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients.Include(p => p.Person).Include(c => c.Doctor).Include(c => c.Doctor.Person)
                                                 .FirstOrDefaultAsync(m => m.PatientId == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // GET: Patients/Create
        public IActionResult Create()
        {
            BuildLists(null);
            return View();
        }

        // POST: Patients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Person,PersonId,Person.PersonId,TagId,Room,Floor,DoctorId")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            BuildLists(patient.DoctorId);
            return View(patient);
        }

        // GET: Patients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Patients == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients.Include(c => c.Person).FirstOrDefaultAsync(c => c.PatientId == id);
            if (patient == null)
            {
                return NotFound();
            }

            BuildLists(patient?.DoctorId);
            return View(patient);
        }

        // POST: Patients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PatientId,Person,PersonId,Person.PersonId,TagId,Room,Floor,DoctorId")] Patient patient)
        {
            if (id != patient.PatientId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.PatientId))
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

            BuildLists(patient.DoctorId);
            return View(patient);
        }

        // GET: Patients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Patients == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .Include(p => p.Person).Include(c => c.Doctor).Include(c => c.Doctor.Person)
                .FirstOrDefaultAsync(m => m.PatientId == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Patients == null)
            {
                return Problem("Entity set 'DoctorsDatabase2023Context.Patients'  is null.");
            }
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(int id)
        {
            return (_context.Patients?.Any(e => e.PatientId == id)).GetValueOrDefault();
        }
    }
}
