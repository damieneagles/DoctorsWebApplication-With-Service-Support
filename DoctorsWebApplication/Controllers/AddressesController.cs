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
using DoctorsWebApplication.Helpers;

namespace DoctorsWebApplication.Controllers
{
    public class AddressesController : Controller
    {
        private readonly DoctorsDatabase2023Context _context;
        private readonly PageSearchData _searchData;

        public AddressesController(DoctorsDatabase2023Context context, IDistributedCache cache)
        {
            _context = context;
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(nameof(_context.Addresses));
            byte[] hashBytes = System.Security.Cryptography.MD5.HashData(inputBytes);
            var uniquePageName = Convert.ToHexString(hashBytes);
            _searchData = new PageSearchData(cache, uniquePageName, Guid.NewGuid());
        }

        private void BuildLists(string? selectedCountry, int? selectedPersonId)
        {
            //View displays "List of PhoneNumbers for (person name)"
            var person = _context.People.FirstOrDefault(p => p.PersonId == selectedPersonId);
            if (person != null)
            {
                ViewData["CombinedName"] = person.CombinedName;
                ViewData["PersonId"] = person.PersonId;
            }

            ViewData["CountriesList"] = new ListHelper(_context, _searchData).BuildCountriesList(selectedCountry);
        }

        // GET: Addresses (id = PersonId)
        public async Task<IActionResult> Index(int? id)
        {
            var addresses = _context.Addresses.Include(a => a.Person);

            BuildLists(null, id);
            return View(await addresses.ToListAsync());
        }

        // GET: Addresses/Details/5 (id = AddressId)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Addresses == null)
            {
                return NotFound();
            }

            var address = await _context.Addresses
                .Include(a => a.Person)
                .FirstOrDefaultAsync(m => m.AddressId == id);
            if (address == null)
            {
                return NotFound();
            }

            BuildLists(address.Country, address.PersonId);
            return View(address);
        }

        // GET: Addresses/Create (id = PersonId)
        public IActionResult Create(int? id)
        {
            BuildLists("Australia", id);
            return View();
        }

        // POST: Addresses/Create (id = PersonId)
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? id, [Bind("PersonId,AddressLine1,AddressLine2,State,PostCode,City,Country")] Address address)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return NotFound("PersonId is NULL");
                }
                
                address.PersonId = (int)id;
                _context.Add(address);
                await _context.SaveChangesAsync();
#pragma warning disable IDE0037 // Use inferred member name
                return RedirectToAction(nameof(Index), new { id = id });
#pragma warning restore IDE0037 // Use inferred member name
            }

            BuildLists(address.Country, id);
            return View(address);
        }

        // GET: Addresses/Edit/5 (id = AddressId)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Addresses == null)
            {
                return NotFound();
            }

            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
            {
                return NotFound();
            }

            BuildLists(address.Country, address.PersonId);
            return View(address);
        }

        // POST: Addresses/Edit/5 (id = AddressId)
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AddressId,PersonId,AddressLine1,AddressLine2,State,PostCode,City,Country")] Address address)
        {
            if (id != address.AddressId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(address);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AddressExists(address.AddressId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id = address?.PersonId });
            }

            BuildLists(address.Country, address.PersonId);
            return View(address);
        }

        // GET: Addresses/Delete/5 (id = AddressId)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Addresses == null)
            {
                return NotFound();
            }

            var address = await _context.Addresses
                .Include(a => a.Person)
                .FirstOrDefaultAsync(m => m.AddressId == id);
            if (address == null)
            {
                return NotFound();
            }

            BuildLists(address.Country, address.PersonId);
            return View(address);
        }

        // POST: Addresses/Delete/5 (id = AddressId)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Addresses == null)
            {
                return Problem("Entity set 'DoctorsDatabase2023Context.Addresses'  is null.");
            }
            var address = await _context.Addresses.FindAsync(id);
            if (address != null)
            {
                _context.Addresses.Remove(address);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id=address?.PersonId });
        }

        private bool AddressExists(int id)
        {
          return (_context.Addresses?.Any(e => e.AddressId == id)).GetValueOrDefault();
        }

    }
}
