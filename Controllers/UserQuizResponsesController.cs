using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lamlai.Models;

namespace test2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserQuizResponsesController : ControllerBase
    {
        private readonly TestContext _context;

        public UserQuizResponsesController(TestContext context)
        {
            _context = context;
        }

        // GET: api/UserQuizResponses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserQuizResponse>>> GetUserQuizResponses()
        {
            return await _context.UserQuizResponses.ToListAsync();
        }

        // GET: api/UserQuizResponses/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserQuizResponse>> GetUserQuizResponse(int id)
        {
            var userQuizResponse = await _context.UserQuizResponses.FindAsync(id);

            if (userQuizResponse == null)
            {
                return NotFound();
            }

            return userQuizResponse;
        }

        // PUT: api/UserQuizResponses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserQuizResponse(int id, UserQuizResponse userQuizResponse)
        {
            if (id != userQuizResponse.ResponseId)
            {
                return BadRequest();
            }

            _context.Entry(userQuizResponse).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserQuizResponseExists(id))
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

        // POST: api/UserQuizResponses
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserQuizResponse>> PostUserQuizResponse(UserQuizResponse userQuizResponse)
        {
            _context.UserQuizResponses.Add(userQuizResponse);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserQuizResponse", new { id = userQuizResponse.ResponseId }, userQuizResponse);
        }

        // DELETE: api/UserQuizResponses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserQuizResponse(int id)
        {
            var userQuizResponse = await _context.UserQuizResponses.FindAsync(id);
            if (userQuizResponse == null)
            {
                return NotFound();
            }

            _context.UserQuizResponses.Remove(userQuizResponse);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserQuizResponseExists(int id)
        {
            return _context.UserQuizResponses.Any(e => e.ResponseId == id);
        }
    }
}
