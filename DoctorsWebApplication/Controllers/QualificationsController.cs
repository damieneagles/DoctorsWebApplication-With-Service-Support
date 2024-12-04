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

namespace DoctorsWebApplication.Controllers
{
    public class QualificationsController : Controller
    {
        private readonly DoctorsDatabase2023Context _context;
        private readonly PageSearchData _searchData;

        public QualificationsController(DoctorsDatabase2023Context context, IDistributedCache cache)
        {
            _context = context;
            using System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(nameof(_context.EmailAddresses));
            byte[] hashBytes = md5.ComputeHash(inputBytes);
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

        // GET: Qualifications (id = PersonId)
        public async Task<IActionResult> Index(int? id)
        {
            var qualifications = _context.Qualifications.Include(q => q.Doctor).Include(c => c.Doctor.Person);

            BuildLists(id);
            return View(await qualifications.ToListAsync());
        }

        // GET: Qualifications/Details/5 (id = QualificationId)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Qualifications == null)
            {
                return NotFound();
            }

            var qualification = await _context.Qualifications.Include(q => q.Doctor).Include(c => c.Doctor.Person)
                                                             .FirstOrDefaultAsync(m => m.QualificationId == id);
            if (qualification == null)
            {
                return NotFound();
            }

            BuildLists(qualification.Doctor.PersonId);
            return View(qualification);
        }

        // GET: Qualifications/Create (id = PersonId)
        public IActionResult Create(int? id)
        {
            BuildLists(id);
            return View();
        }

        // POST: Qualifications/Create (id = PersonId)
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? id, [Bind("Qualification1,Specialisation,Institution,DoctorId")] Qualification qualification)
        {
            if (ModelState.IsValid)
            {
                var doctor = _context.Doctors.Where(c=> c.PersonId == id).FirstOrDefault();
                if (doctor != null)
                {
                    qualification.DoctorId = doctor.DoctorId;
                }
                _context.Add(qualification);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { id = id });
            }

            BuildLists(id);
            return View(qualification);
        }

        // GET: Qualifications/Edit/5 (id = QualificationId)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Qualifications == null)
            {
                return NotFound();
            }

            var qualification = await _context.Qualifications.Include(c => c.Doctor).Include(c => c.Doctor.Person).FirstOrDefaultAsync(c => c.QualificationId == id);
            if (qualification == null)
            {
                return NotFound();
            }

            BuildLists(qualification.Doctor.PersonId);
            return View(qualification);
        }

        // POST: Qualifications/Edit/5 (id = QualificationId)
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("QualificationId,Qualification1,Specialisation,Institution,DoctorId")] Qualification qualification)
        {
            if (id != qualification.QualificationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(qualification);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QualificationExists(qualification.QualificationId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id = qualification.Doctor.PersonId });
            }

            BuildLists(qualification.Doctor.PersonId);
            return View(qualification);
        }

        // GET: Qualifications/Delete/5 (id = QualificationId)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Qualifications == null)
            {
                return NotFound();
            }

            var qualification = await _context.Qualifications
                                                            .Include(q => q.Doctor)
                                                            .FirstOrDefaultAsync(m => m.QualificationId == id);
            if (qualification == null)
            {
                return NotFound();
            }

            BuildLists(qualification.Doctor.PersonId);
            return View(qualification);
        }

        // POST: Qualifications/Delete/5 (id = QualificationId)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Qualifications == null)
            {
                return Problem("Entity set 'DoctorsDatabase2023Context.Qualifications'  is null.");
            }
            var qualification = await _context.Qualifications.FindAsync(id);
            if (qualification != null)
            {
                _context.Qualifications.Remove(qualification);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id = qualification?.Doctor.PersonId });
        }

        private bool QualificationExists(int id)
        {
          return (_context.Qualifications?.Any(e => e.QualificationId == id)).GetValueOrDefault();
        }
    }
}
