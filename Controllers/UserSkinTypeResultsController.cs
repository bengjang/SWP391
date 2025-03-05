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
    public class UserSkinTypeResultsController : ControllerBase
    {
        private readonly TestContext _context;

        public UserSkinTypeResultsController(TestContext context)
        {
            _context = context;
        }

        // GET: api/UserSkinTypeResults
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserSkinTypeResult>>> GetUserSkinTypeResults()
        {
            return await _context.UserSkinTypeResults.ToListAsync();
        }

        // GET: api/UserSkinTypeResults/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserSkinTypeResult>> GetUserSkinTypeResult(int id)
        {
            var userSkinTypeResult = await _context.UserSkinTypeResults.FindAsync(id);

            if (userSkinTypeResult == null)
            {
                return NotFound();
            }

            return userSkinTypeResult;
        }

        // PUT: api/UserSkinTypeResults/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserSkinTypeResult(int id, UserSkinTypeResult userSkinTypeResult)
        {
            if (id != userSkinTypeResult.ResultId)
            {
                return BadRequest();
            }

            _context.Entry(userSkinTypeResult).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserSkinTypeResultExists(id))
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

        // POST: api/UserSkinTypeResults
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserSkinTypeResult>> PostUserSkinTypeResult(UserSkinTypeResult userSkinTypeResult)
        {
            _context.UserSkinTypeResults.Add(userSkinTypeResult);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserSkinTypeResult", new { id = userSkinTypeResult.ResultId }, userSkinTypeResult);
        }

        // DELETE: api/UserSkinTypeResults/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserSkinTypeResult(int id)
        {
            var userSkinTypeResult = await _context.UserSkinTypeResults.FindAsync(id);
            if (userSkinTypeResult == null)
            {
                return NotFound();
            }

            _context.UserSkinTypeResults.Remove(userSkinTypeResult);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserSkinTypeResultExists(int id)
        {
            return _context.UserSkinTypeResults.Any(e => e.ResultId == id);
        }
    }
}
