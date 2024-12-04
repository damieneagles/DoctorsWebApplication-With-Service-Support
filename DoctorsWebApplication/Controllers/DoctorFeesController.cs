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

namespace DoctorsWebApplication.Controllers
{
    public class DoctorFeesController : Controller
    {
        private readonly DoctorsDatabase2023Context _context;

        public DoctorFeesController(DoctorsDatabase2023Context context)
        {
            _context = context;
        }

        private void BuildLists(int? selectedDoctorId)
        {
            var doctors = _context.Doctors.Include(c => c.Person).ToList();
            ViewData["DoctorsList"] = new SelectList(doctors.OrderBy(c => c.Person.CombinedName), "DoctorId", "Person.CombinedName", selectedDoctorId);
        }

        // GET: DoctorFees
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<IActionResult> Index()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var doctorfees = (from p in _context.DoctorFees
                                                    .Include(c => c.Doctor)
                                                    .Include(c => c.Fee)
                                                    .Include(c => c.Doctor.Person)
                              select p).ToList();

            return View(doctorfees);
        }

        // GET: DoctorFees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.DoctorFees == null)
            {
                return NotFound();
            }

            var doctorfees = await (from p in _context.DoctorFees
                                                    .Include(c => c.Doctor)
                                                    .Include(c => c.Fee)
                                                    .Include(c => c.Doctor.Person)
                              select p).FirstOrDefaultAsync(c => c.DoctorFeeId == id);

            if (doctorfees == null)
            {
                return NotFound();
            }

            return View(doctorfees);
        }

        // GET: DoctorFees/Create
        public IActionResult Create()
        {
            BuildLists(null);
            return View();
        }

        // POST: DoctorFees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DoctorId,Fee,Fee.DoctorFee,Fee.BulkBilled")] DoctorFee doctorFee)
        {
            if (ModelState.IsValid)
            {
                using (var t = _context.Database.BeginTransaction())
                {
                    //Add the Fee
                    _context.Add(doctorFee.Fee);
                    await _context.SaveChangesAsync();

                    //Add the DoctorFee
                    doctorFee.FeeId = doctorFee.Fee.FeeId;
                    _context.Add(doctorFee);
                    await _context.SaveChangesAsync();

                    t.Commit();
                }
                return RedirectToAction(nameof(Index));
            }

            BuildLists(doctorFee.DoctorId);
            return View(doctorFee);
        }

        // GET: DoctorFees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.DoctorFees == null)
            {
                return NotFound();
            }

            var doctorFee = await _context.DoctorFees
                                          .Include(d => d.Doctor)
                                          .Include(d => d.Doctor.Person)
                                          .Include(d => d.Fee)
                                          .FirstOrDefaultAsync(c => c.DoctorFeeId == id);

            if (doctorFee == null)
            {
                return NotFound();
            }

            BuildLists(doctorFee.DoctorId);
            return View(doctorFee);
        }

        // POST: DoctorFees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DoctorFeeId,Fee,FeeId,Fee.FeeId,Fee.BulkBilled,DoctorId")] DoctorFee doctorFee)
        {
            if (id != doctorFee.DoctorFeeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(doctorFee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorFeeExists(doctorFee.DoctorFeeId))
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

            BuildLists(doctorFee.DoctorId);
            return View(doctorFee);
        }

        // GET: DoctorFees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.DoctorFees == null)
            {
                return NotFound();
            }

            var doctorFee = await _context.DoctorFees
                .Include(d => d.Doctor)
                .Include(d => d.Doctor.Person)
                .Include(d => d.Fee)
                .FirstOrDefaultAsync(m => m.DoctorFeeId == id);

            if (doctorFee == null)
            {
                return NotFound();
            }

            return View(doctorFee);
        }

        // POST: DoctorFees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.DoctorFees == null)
            {
                return Problem("Entity set 'DoctorsDatabase2023Context.DoctorFees'  is null.");
            }
            var doctorFee = await _context.DoctorFees.FindAsync(id);
            if (doctorFee != null)
            {
                _context.DoctorFees.Remove(doctorFee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DoctorFeeExists(int id)
        {
            return (_context.DoctorFees?.Any(e => e.DoctorFeeId == id)).GetValueOrDefault();
        }
    }
}
