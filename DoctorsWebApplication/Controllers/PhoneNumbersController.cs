using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoctorsWebApplication.Data;
using DoctorsWebApplication.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DoctorsWebApplication.Controllers
{
    public class PhoneNumbersController : Controller
    {
        private readonly DoctorsDatabase2023Context _context;

        public PhoneNumbersController(DoctorsDatabase2023Context context)
        {
            _context = context;
        }

        private void BuildLists(int? selectedPersonId)
        {
            //View displays "List of PhoneNumbers for (person name)"
            var person = _context.People.FirstOrDefault(p => p.PersonId == selectedPersonId);
            if (person != null)
            {
                ViewData["CombinedName"] = person.CombinedName;
                ViewData["PersonId"] = person.PersonId;
            }
        }

        // GET: PhoneNumbers (id = PersonId)
        public async Task<IActionResult> Index(int? id)
        {
            var phoneNumbers = _context.PhoneNumbers.Include(p => p.Person).Where(p => p.PersonId == id);

            BuildLists(id);
            return View(await phoneNumbers.ToListAsync());
        }

        // GET: PhoneNumbers/Details/5 (id = PhoneNumberId)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PhoneNumbers == null)
            {
                return NotFound();
            }

            var phoneNumber = await _context.PhoneNumbers
                .Include(p => p.Person)
                .FirstOrDefaultAsync(m => m.PhoneNumberId == id);
            if (phoneNumber == null)
            {
                return NotFound();
            }

            BuildLists(phoneNumber.PersonId);
            return View(phoneNumber);
        }

        // GET: PhoneNumbers/Create (id = PersonId)
        public IActionResult Create(int? id)
        {
            BuildLists(id);
            return View();
        }

        // POST: PhoneNumbers/Create (id = PersonId)
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? id, [Bind("PhoneNumberId,PhoneNumber1,PersonId")] PhoneNumber phoneNumber)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return NotFound("PersonId is NULL");
                }
                
                phoneNumber.PersonId = (int)id;
                _context.Add(phoneNumber);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { id = id });
            }

            BuildLists(id);
            return View(phoneNumber);
        }

        // GET: PhoneNumbers/Edit/5 by (id = PhoneNumberId)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PhoneNumbers == null)
            {
                return NotFound();
            }

            var phoneNumber = await _context.PhoneNumbers.FindAsync(id);
            if (phoneNumber == null)
            {
                return NotFound();
            }

            BuildLists(phoneNumber.PersonId);
            return View(phoneNumber);
        }

        // POST: PhoneNumbers/Edit/5 (id = PhoneNumberId)
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PhoneNumberId,PhoneNumber1,PersonId")] PhoneNumber phoneNumber)
        {
            if (id != phoneNumber.PhoneNumberId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phoneNumber);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhoneNumberExists(phoneNumber.PhoneNumberId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id = phoneNumber?.PersonId });
            }

            BuildLists(phoneNumber.PersonId);
            return View(phoneNumber);
        }

        // GET: PhoneNumbers/Delete/5 (id = PhoneNumberId)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PhoneNumbers == null)
            {
                return NotFound();
            }

            var phoneNumber = await _context.PhoneNumbers
                .Include(p => p.Person)
                .FirstOrDefaultAsync(m => m.PhoneNumberId == id);
            if (phoneNumber == null)
            {
                return NotFound();
            }

            BuildLists(phoneNumber.PersonId);
            return View(phoneNumber);
        }

        // POST: PhoneNumbers/Delete/5 (id = PhoneNumberId)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PhoneNumbers == null)
            {
                return Problem("Entity set 'DoctorsDatabase2023Context.PhoneNumbers'  is null.");
            }
            var phoneNumber = await _context.PhoneNumbers.FindAsync(id);
            if (phoneNumber != null)
            {
                _context.PhoneNumbers.Remove(phoneNumber);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id = phoneNumber?.PersonId });
        }

        private bool PhoneNumberExists(int id)
        {
            return (_context.PhoneNumbers?.Any(e => e.PhoneNumberId == id)).GetValueOrDefault();
        }
    }
}
