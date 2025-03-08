using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lamlai.Models;


namespace lamlai.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CancelRequestsController : ControllerBase
    {
        private readonly TestContext _context;

        public CancelRequestsController(TestContext context)
        {
            _context = context;
        }

        // GET: api/CancelRequests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CancelRequest>>> GetCancelRequests()
        {
            return await _context.CancelRequests.ToListAsync();
        }

        // GET: api/CancelRequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CancelRequest>> GetCancelRequest(int id)
        {
            var cancelRequest = await _context.CancelRequests.FindAsync(id);

            if (cancelRequest == null)
            {
                return NotFound();
            }

            return cancelRequest;
        }

        // PUT: api/CancelRequests/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCancelRequest(int id, CancelRequest cancelRequest)
        {
            if (id != cancelRequest.CancelRequestId)
            {
                return BadRequest();
            }

            _context.Entry(cancelRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CancelRequestExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/CancelRequests
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CancelRequest>> PostCancelRequest(CancelRequest cancelRequest)
        {
            _context.CancelRequests.Add(cancelRequest);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCancelRequest", new { id = cancelRequest.CancelRequestId }, cancelRequest);
        }

        // DELETE: api/CancelRequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCancelRequest(int id)
        {
            var cancelRequest = await _context.CancelRequests.FindAsync(id);
            if (cancelRequest == null)
            {
                return NotFound();
            }

            _context.CancelRequests.Remove(cancelRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CancelRequestExists(int id)
        {
            return _context.CancelRequests.Any(e => e.CancelRequestId == id);
        }
    }
}
