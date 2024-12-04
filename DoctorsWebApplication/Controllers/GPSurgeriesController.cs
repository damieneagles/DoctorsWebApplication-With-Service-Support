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
    public class GPSurgeriesController : Controller
    {
        private readonly DoctorsDatabase2023Context _context;
        private readonly PageSearchData _searchData;

        public GPSurgeriesController(DoctorsDatabase2023Context context, IDistributedCache cache)
        {
            _context = context;
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(nameof(_context.GPSurgeries));
            byte[] hashBytes = System.Security.Cryptography.MD5.HashData(inputBytes);
            var uniquePageName = Convert.ToHexString(hashBytes);
            _searchData = new PageSearchData(cache, uniquePageName, Guid.NewGuid());
        }

        // GET: GPSurgeries
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

            var surgeries = (from p in _context.GPSurgeries
                                               .Where(c => c.CurrentFlag == _searchData.CheckCurrent)
                             select p).ToList();

            //Initial navigation to page OR Back to list has been clicked from a subsequent page
            searchString ??= _searchData.SearchString ?? string.Empty;

            string sortDirection = _searchData.SortDirection;
            if (!String.IsNullOrEmpty(searchString))
            {
                surgeries = (from p in surgeries
                                       .Where(q => q.SurgeryName.Contains(searchString))
                             select p).ToList();

                //New Search so back to defaults
                if (searchString != _searchData.CurrentFilter)
                {
                    //Back to search start
                    sortOrder = "SurgeryName";
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
                sortOrder = "SurgeryName";
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
            surgeries = surgeries.AsQueryable<GPSurgery>().Sort(sortOrder, sortDirection).ToDynamicList<GPSurgery>();
#pragma warning restore CS8604 // Possible null reference argument.

            //save previous sort criteria
            _searchData.SortDirection = sortDirection;
            _searchData.SortOrder = sortOrder;
            _searchData.PageIndex = page;
            _searchData.CheckCurrent = checkCurrent ?? true;

            int pageSize = 5;
            int pageNumber = (_searchData.PageIndex ?? 1);
            return View(new PageMetaDataModel<GPSurgery>(surgeries.ToPagedList(pageNumber, pageSize), _searchData));
        }

        // GET: GPSurgeries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.GPSurgeries == null)
            {
                return NotFound();
            }

            var gPSurgery = await _context.GPSurgeries
                .FirstOrDefaultAsync(m => m.GPSurgeryId == id);
            if (gPSurgery == null)
            {
                return NotFound();
            }

            return View(gPSurgery);
        }

        // GET: GPSurgeries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GPSurgeries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SurgeryName,StartConsultDate,EndConsultDate,CurrentFlag")] GPSurgery gPSurgery)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gPSurgery);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(gPSurgery);
        }

        // GET: GPSurgeries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.GPSurgeries == null)
            {
                return NotFound();
            }

            var gPSurgery = await _context.GPSurgeries.FindAsync(id);
            if (gPSurgery == null)
            {
                return NotFound();
            }
            return View(gPSurgery);
        }

        // POST: GPSurgeries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GPSurgeryId,SurgeryName,StartConsultDate,EndConsultDate,CurrentFlag")] GPSurgery gPSurgery)
        {
            if (id != gPSurgery.GPSurgeryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gPSurgery);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GPSurgeryExists(gPSurgery.GPSurgeryId))
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
            return View(gPSurgery);
        }

        // GET: GPSurgeries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.GPSurgeries == null)
            {
                return NotFound();
            }

            var gPSurgery = await _context.GPSurgeries
                .FirstOrDefaultAsync(m => m.GPSurgeryId == id);
            if (gPSurgery == null)
            {
                return NotFound();
            }

            return View(gPSurgery);
        }

        // POST: GPSurgeries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.GPSurgeries == null)
            {
                return Problem("Entity set 'DoctorsDatabase2023Context.GPSurgeries'  is null.");
            }
            var gPSurgery = await _context.GPSurgeries.FindAsync(id);
            if (gPSurgery != null)
            {
                _context.GPSurgeries.Remove(gPSurgery);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GPSurgeryExists(int id)
        {
          return (_context.GPSurgeries?.Any(e => e.GPSurgeryId == id)).GetValueOrDefault();
        }
    }
}
