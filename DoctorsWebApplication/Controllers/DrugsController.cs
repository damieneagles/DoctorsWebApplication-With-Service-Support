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

namespace DoctorsWebApplication.Controllers
{
    public class DrugsController : Controller
    {
        private readonly DoctorsDatabase2023Context _context;
        private readonly PageSearchData _searchData;

        public DrugsController(DoctorsDatabase2023Context context, IDistributedCache cache)
        {
            _context = context;
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(nameof(_context.Drugs));
            byte[] hashBytes = System.Security.Cryptography.MD5.HashData(inputBytes);
            var uniquePageName = Convert.ToHexString(hashBytes);
            _searchData = new PageSearchData(cache, uniquePageName, Guid.NewGuid());
        }

        // GET: Drugs
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

            var drugs = (from p in _context.Drugs
                         select p).ToList();

            //Initial navigation to page OR Back to list has been clicked from a subsequent page
            searchString ??= _searchData.SearchString ?? string.Empty;

            string sortDirection = _searchData.SortDirection;
            if (!String.IsNullOrEmpty(searchString))
            {
                drugs = (from p in drugs
                                   .Where(q => q.CommonName.Contains(searchString) ||
                                                   q.MedicalName.Contains(searchString))
                         select p).ToList();

                //New Search so back to defaults
                if (searchString != _searchData.CurrentFilter)
                {
                    //Back to search start
                    sortOrder = "CommonName";
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
                sortOrder = "CommonName";
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
            drugs = drugs.AsQueryable<Drug>().Sort(sortOrder, sortDirection).ToDynamicList<Drug>();
#pragma warning restore CS8604 // Possible null reference argument.

            //save previous sort criteria
            _searchData.SortDirection = sortDirection;
            _searchData.SortOrder = sortOrder;
            _searchData.PageIndex = page;
            _searchData.CheckCurrent = checkCurrent ?? true;

            int pageSize = 5;
            int pageNumber = (_searchData.PageIndex ?? 1);
            return View(new PageMetaDataModel<Drug>(drugs.ToPagedList(pageNumber, pageSize), _searchData));
        }

        // GET: Drugs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Drugs == null)
            {
                return NotFound();
            }

            var drug = await _context.Drugs
                .FirstOrDefaultAsync(m => m.DrugId == id);
            if (drug == null)
            {
                return NotFound();
            }

            return View(drug);
        }

        // GET: Drugs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Drugs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DrugId,CommonName,MedicalName,QuantityInStock,ReorderQuantity,ModifiedDate,ModifiedBy,CreatedDate,CreatedBy,rowguid,RetailPrice,WholeSalePrice")] Drug drug)
        {
            if (ModelState.IsValid)
            {
                _context.Add(drug);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(drug);
        }

        // GET: Drugs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Drugs == null)
            {
                return NotFound();
            }

            var drug = await _context.Drugs.FindAsync(id);
            if (drug == null)
            {
                return NotFound();
            }
            return View(drug);
        }

        // POST: Drugs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DrugId,CommonName,MedicalName,QuantityInStock,ReorderQuantity,ModifiedDate,ModifiedBy,CreatedDate,CreatedBy,rowguid,RetailPrice,WholeSalePrice")] Drug drug)
        {
            if (id != drug.DrugId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(drug);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DrugExists(drug.DrugId))
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
            return View(drug);
        }

        // GET: Drugs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Drugs == null)
            {
                return NotFound();
            }

            var drug = await _context.Drugs
                .FirstOrDefaultAsync(m => m.DrugId == id);
            if (drug == null)
            {
                return NotFound();
            }

            return View(drug);
        }

        // POST: Drugs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Drugs == null)
            {
                return Problem("Entity set 'DoctorsDatabase2023Context.Drugs'  is null.");
            }
            var drug = await _context.Drugs.FindAsync(id);
            if (drug != null)
            {
                _context.Drugs.Remove(drug);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DrugExists(int id)
        {
          return (_context.Drugs?.Any(e => e.DrugId == id)).GetValueOrDefault();
        }
    }
}
