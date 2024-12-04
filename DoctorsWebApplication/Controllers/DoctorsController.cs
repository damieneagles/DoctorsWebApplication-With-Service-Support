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
using X.PagedList.Extensions;
using X.PagedList.EntityFramework;

namespace DoctorsWebApplication.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly DoctorsDatabase2023Context _context;
        private readonly PageSearchData _searchData;

        public DoctorsController(DoctorsDatabase2023Context context, IDistributedCache cache)
        {
            _context = context;
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(nameof(_context.Doctors));
            byte[] hashBytes = System.Security.Cryptography.MD5.HashData(inputBytes);
            var uniquePageName = Convert.ToHexString(hashBytes);
            _searchData = new PageSearchData(cache, uniquePageName, Guid.NewGuid());
        }

        // GET: Doctors
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task<IActionResult> Index([Bind("SearchString,CheckCurrent,SortOrder")] string sortOrder, string currentFilter, string? searchString, int? page, bool? checkCurrent)
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            //Set page of pager
            //Page Number
            page ??= _searchData.PageIndex ?? 1;

            //Get current employees
            _searchData.CheckCurrent = checkCurrent ?? true;

            var doctors = (from p in _context.Doctors.Include(c => c.GPSurgery).Include(c => c.Person)
                           select p).ToList();

            //Initial navigation to page OR Back to list has been clicked from a subsequent page
            searchString ??= _searchData.SearchString ?? string.Empty;

            string sortDirection = _searchData.SortDirection;
            if (!String.IsNullOrEmpty(searchString))
            {
                doctors = (from p in doctors
                                     .Where(q => q.GPSurgery.SurgeryName.Contains(searchString) ||
                                                 q.Person.CombinedName.Contains(searchString))
                           select p).ToList();

                //New Search so back to defaults
                if (searchString != _searchData.CurrentFilter)
                {
                    //Back to search start
                    sortOrder = "GPSurgery.SurgeryName";
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
                sortOrder = "GPSurgery.SurgeryName";
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
            doctors = doctors.AsQueryable<Doctor>().Sort(sortOrder, sortDirection).ToDynamicList<Doctor>();
#pragma warning restore CS8604 // Possible null reference argument.

            //save previous sort criteria
            _searchData.SortDirection = sortDirection;
            _searchData.SortOrder = sortOrder;
            _searchData.PageIndex = page;
            _searchData.CheckCurrent = checkCurrent ?? true;

            int pageSize = 5;
            int pageNumber = (_searchData.PageIndex ?? 1);
            return View(new PageMetaDataModel<Doctor>(doctors.ToPagedList(pageNumber, pageSize), _searchData));
        }

        // GET: Doctors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Doctors == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .Include(d => d.GPSurgery)
                .Include(d => d.Person)
                .FirstOrDefaultAsync(m => m.DoctorId == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        private void BuildLists(int? selectedGPSurgeryId)
        {
            ViewData["GPSurgeryList"] = new SelectList(_context.GPSurgeries, "GPSurgeryId", "SurgeryName", selectedGPSurgeryId);
        }

        // GET: Doctors/Create (id = PersonId)
        public IActionResult Create()
        {
            BuildLists(null);
            return View();
        }

        // POST: Doctors/Create 
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Person,PersonId,Person.PersonId,Title,Abbreviations,GPSurgeryId,GPSurgery,GPSurgery.GPSurgeryId")] Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(doctor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            BuildLists(doctor.GPSurgeryId);
            return View(doctor);
        }

        // GET: Doctors/Edit/5 (id = DoctorId)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Doctors == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors.Include(c => c.Person).FirstOrDefaultAsync(c => c.DoctorId == id);
            if (doctor == null)
            {
                return NotFound();
            }
            
            BuildLists(doctor.GPSurgeryId);
            return View(doctor);
        }

        // POST: Doctors/Edit/5 (id = DoctorId)
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DoctorId,Person,PersonId,Person.PersonId,Title,Abbreviations,GPSurgery,GPSurgeryId,GPSurgery.GPSurgeryId")] Doctor doctor)
        {
            if (id != doctor.DoctorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.DoctorId))
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
            
            BuildLists(doctor.GPSurgeryId);
            return View(doctor);
        }

        // GET: Doctors/Delete/5 (id = DoctorId)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Doctors == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                                                .Include(d => d.GPSurgery)
                                                .Include(d => d.Person)
                                                .FirstOrDefaultAsync(m => m.DoctorId == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: Doctors/Delete/5 (id = DoctorId)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Doctors == null)
            {
                return Problem("Entity set 'DoctorsDatabase2023Context.Doctors'  is null.");
            }
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DoctorExists(int id)
        {
          return (_context.Doctors?.Any(e => e.DoctorId == id)).GetValueOrDefault();
        }
    }
}
