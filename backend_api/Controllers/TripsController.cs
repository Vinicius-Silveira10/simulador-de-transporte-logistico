using LogisticsTrackingAPI.Data;
using LogisticsTrackingAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogisticsTrackingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TripsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Trip>>> GetTrips()
        {
            return await _context.Trips.Include(t => t.Player).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Trip>> GetTrip(int id)
        {
            var trip = await _context.Trips.Include(t => t.Player).FirstOrDefaultAsync(t => t.Id == id);

            if (trip == null)
            {
                return NotFound();
            }

            return trip;
        }

        [HttpPost]
        public async Task<ActionResult<Trip>> PostTrip(Trip trip)
        {
            _context.Trips.Add(trip);
            
            // Adiciona o lucro bruto na conta IMEDIATAMENTE (Tycoon Mode)
            var company = await _context.Players.FindAsync(trip.PlayerId);
            if(company != null) {
                company.NetWorth += trip.NetProfit;
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTrip), new { id = trip.Id }, trip);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTripStatus(int id, [FromBody] string status)
        {
            var trip = await _context.Trips.FindAsync(id);

            if (trip == null)
            {
                return NotFound();
            }

            trip.Status = status;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        public class FinishTripPayload 
        {
            public decimal FinalFuelCost { get; set; }
            public string? IncidentLogs { get; set; }
        }

        [HttpPut("{id}/finish")]
        public async Task<IActionResult> FinishTrip(int id, [FromBody] FinishTripPayload payload)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip == null) return NotFound();

            trip.Status = "Finished";
            trip.KmCosts += payload.FinalFuelCost;
            trip.NetProfit -= payload.FinalFuelCost;
            trip.IncidentLogs = payload.IncidentLogs;
            
            // TYCOON: Desconta APENAS os custos de viagem (frete/combustível/acidentes).
            // O lucro base já cai na assinatura do contrato!
            var company = await _context.Players.FindAsync(trip.PlayerId);
            if(company != null) {
                company.NetWorth -= payload.FinalFuelCost;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
