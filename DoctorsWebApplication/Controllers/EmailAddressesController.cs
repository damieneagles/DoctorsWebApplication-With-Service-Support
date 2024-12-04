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
using System.Net.Mail;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;

namespace DoctorsWebApplication.Controllers
{
    public class EmailAddressesController : Controller
    {
        private readonly DoctorsDatabase2023Context _context;
        private readonly PageSearchData _searchData;

        public EmailAddressesController(DoctorsDatabase2023Context context, IDistributedCache cache)
        {
            _context = context;
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(nameof(_context.EmailAddresses));
            byte[] hashBytes = System.Security.Cryptography.MD5.HashData(inputBytes);
            var uniquePageName = Convert.ToHexString(hashBytes);
            _searchData = new PageSearchData(cache, uniquePageName, Guid.NewGuid());
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

        // GET: EmailAddresses (id = PersonId)
        public async Task<IActionResult> Index(int? id)
        {
            var emailAddresses = _context.EmailAddresses.Include(e => e.Person);

            BuildLists(id);
            return View(await emailAddresses.ToListAsync());
        }

        // GET: EmailAddresses/Details/5 (id = EmailAddressId)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.EmailAddresses == null)
            {
                return NotFound();
            }

            var emailAddress = await _context.EmailAddresses
                                                            .Include(e => e.Person)
                                                            .FirstOrDefaultAsync(m => m.EmailAddressId == id);
            if (emailAddress == null)
            {
                return NotFound();
            }

            BuildLists(emailAddress.PersonId);
            return View(emailAddress);
        }

        // GET: EmailAddresses/Create (id = PersonId)
        public IActionResult Create(int? id)
        {
            BuildLists(id);
            return View();
        }

        // POST: EmailAddresses/Create (id = PersonId)
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? id, [Bind("EmailAddress1")] EmailAddress emailAddress)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return NotFound("PersonId is NULL");
                }

                emailAddress.PersonId = (int)id;
                _context.Add(emailAddress);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { id = id });
            }
            
            BuildLists(emailAddress.PersonId);
            return View(emailAddress);
        }

        // GET: EmailAddresses/Edit/5 (id = EmailAddressId)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.EmailAddresses == null)
            {
                return NotFound();
            }

            var emailAddress = await _context.EmailAddresses.Include(c => c.Person).FirstOrDefaultAsync(m => m.EmailAddressId == id);
            if (emailAddress == null)
            {
                return NotFound();
            }
            
            BuildLists(emailAddress.PersonId);
            return View(emailAddress);
        }

        // POST: EmailAddresses/Edit/5 (id = EmailAddressId)
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmailAddressId,EmailAddress1,PersonId")] EmailAddress emailAddress)
        {
            if (id != emailAddress.EmailAddressId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(emailAddress);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmailAddressExists(emailAddress.EmailAddressId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id = emailAddress?.PersonId });
            }
            
            BuildLists(emailAddress.PersonId);
            return View(emailAddress);
        }

        // GET: EmailAddresses/Delete/5 (id = EmailAddressId)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.EmailAddresses == null)
            {
                return NotFound();
            }

            var emailAddress = await _context.EmailAddresses.Include(e => e.Person)
                                                            .FirstOrDefaultAsync(m => m.EmailAddressId == id);
            if (emailAddress == null)
            {
                return NotFound();
            }

            BuildLists(emailAddress?.PersonId);
            return View(emailAddress);
        }

        // POST: EmailAddresses/Delete/5 (id = EmailAddressId)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.EmailAddresses == null)
            {
                return Problem("Entity set 'DoctorsDatabase2023Context.EmailAddresses'  is null.");
            }
            var emailAddress = await _context.EmailAddresses.Include(c => c.Person).FirstOrDefaultAsync(m => m.EmailAddressId == id);
            if (emailAddress != null)
            {
                _context.EmailAddresses.Remove(emailAddress);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id = emailAddress?.PersonId });
        }

        private bool EmailAddressExists(int id)
        {
          return (_context.EmailAddresses?.Any(e => e.EmailAddressId == id)).GetValueOrDefault();
        }
    }
}
