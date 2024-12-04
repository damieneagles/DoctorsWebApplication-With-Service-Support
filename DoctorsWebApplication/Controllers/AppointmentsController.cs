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
    public class AppointmentsController : Controller
    {
        private readonly DoctorsDatabase2023Context _context;
        private readonly PageSearchData _searchData;

        public AppointmentsController(DoctorsDatabase2023Context context, IDistributedCache cache)
        {
            _context = context;
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(nameof(_context.Appointments));
            byte[] hashBytes = System.Security.Cryptography.MD5.HashData(inputBytes);
            var uniquePageName = Convert.ToHexString(hashBytes);
            _searchData = new PageSearchData(cache, uniquePageName, Guid.NewGuid());
        }

        // GET: Appointments
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

            var appointments = (from p in _context.Appointments
                                                        .Include(c => c.Patient)
                                                        .Include(c => c.Patient.Person)
                                                        .Include(c => c.Doctor)
                                                        .Include(c => c.Doctor.Person)
                                select p).ToList();

            //Initial navigation to page OR Back to list has been clicked from a subsequent page
            searchString ??= _searchData.SearchString ?? string.Empty;

            string sortDirection = _searchData.SortDirection;
            if (!String.IsNullOrEmpty(searchString))
            {
                appointments = (from p in appointments
                                       .Where(q => q.Doctor.Person.CombinedName.Contains(searchString) ||
                                                   q.Patient.Person.CombinedName.Contains(searchString))
                                select p).ToList();

                //New Search so back to defaults
                if (searchString != _searchData.CurrentFilter)
                {
                    //Back to search start
                    sortOrder = "Doctor.Person.CombinedName";
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
                sortOrder = "Doctor.Person.CombinedName";
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
            appointments = appointments.AsQueryable<Appointment>().Sort(sortOrder, sortDirection).ToDynamicList<Appointment>();
#pragma warning restore CS8604 // Possible null reference argument.

            //save previous sort criteria
            _searchData.SortDirection = sortDirection;
            _searchData.SortOrder = sortOrder;
            _searchData.PageIndex = page;
            _searchData.CheckCurrent = checkCurrent ?? true;

            int pageSize = 5;
            int pageNumber = (_searchData.PageIndex ?? 1);
            return View(new PageMetaDataModel<Appointment>(appointments.ToPagedList<Appointment>(pageNumber, pageSize), _searchData));
        }

        private void BuildLists(int? selectedDoctorId, int? selectedPatientId)
        {
            ViewData["DoctorList"] = new SelectList(_context.Doctors.Include(c => c.Person), "DoctorId", "Person.CombinedName", selectedDoctorId);
            ViewData["PatientList"] = new SelectList(_context.Patients.Include(c => c.Person), "PatientId", "Person.CombinedName", selectedPatientId);
        }

        // GET: Appointments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Appointments == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                                                .Include(c => c.Patient)
                                                .Include(c => c.Patient.Person)
                                                .Include(c => c.Doctor)
                                                .Include(c => c.Doctor.Person)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Appointments/Create
        public IActionResult Create()
        {
            BuildLists(null, null);
            return View();
        }

        // POST: Appointments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StartAppointment,EndAppointment,DoctorId,PatientId,Room,Floor")] Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            BuildLists(appointment?.DoctorId, appointment?.PatientId);
            return View(appointment);
        }

        // GET: Appointments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Appointments == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.Include(c => c.Patient)
                                                        .Include(c => c.Patient.Person)
                                                        .Include(c => c.Doctor)
                                                        .Include(c => c.Doctor.Person).FirstOrDefaultAsync(c => c.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound();
            }

            BuildLists(appointment?.DoctorId, appointment?.PatientId);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AppointmentId,StartAppointment,EndAppointment,DoctorId,PatientId,Room,Floor")] Appointment appointment)
        {
            if (id != appointment.AppointmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.AppointmentId))
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

            BuildLists(appointment?.DoctorId, appointment?.PatientId);
            return View(appointment);
        }

        // GET: Appointments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Appointments == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                                                .Include(c => c.Patient)
                                                .Include(c => c.Patient.Person)
                                                .Include(c => c.Doctor)
                                                .Include(c => c.Doctor.Person)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Appointments == null)
            {
                return Problem("Entity set 'DoctorsDatabase2023Context.Appointments'  is null.");
            }
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return (_context.Appointments?.Any(e => e.AppointmentId == id)).GetValueOrDefault();
        }
    }
}
