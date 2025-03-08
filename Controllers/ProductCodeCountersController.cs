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
    public class ProductCodeCountersController : ControllerBase
    {
        private readonly TestContext _context;

        public ProductCodeCountersController(TestContext context)
        {
            _context = context;
        }

        // GET: api/ProductCodeCounters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductCodeCounter>>> GetProductCodeCounters()
        {
            return await _context.ProductCodeCounters.ToListAsync();
        }

        // GET: api/ProductCodeCounters/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductCodeCounter>> GetProductCodeCounter(string id)
        {
            var productCodeCounter = await _context.ProductCodeCounters.FindAsync(id);

            if (productCodeCounter == null)
            {
                return NotFound();
            }

            return productCodeCounter;
        }

        // PUT: api/ProductCodeCounters/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductCodeCounter(string id, ProductCodeCounter productCodeCounter)
        {
            if (id != productCodeCounter.CategoryType)
            {
                return BadRequest();
            }

            _context.Entry(productCodeCounter).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductCodeCounterExists(id))
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

        // POST: api/ProductCodeCounters
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProductCodeCounter>> PostProductCodeCounter(ProductCodeCounter productCodeCounter)
        {
            _context.ProductCodeCounters.Add(productCodeCounter);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProductCodeCounterExists(productCodeCounter.CategoryType))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProductCodeCounter", new { id = productCodeCounter.CategoryType }, productCodeCounter);
        }

        // DELETE: api/ProductCodeCounters/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductCodeCounter(string id)
        {
            var productCodeCounter = await _context.ProductCodeCounters.FindAsync(id);
            if (productCodeCounter == null)
            {
                return NotFound();
            }

            _context.ProductCodeCounters.Remove(productCodeCounter);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductCodeCounterExists(string id)
        {
            return _context.ProductCodeCounters.Any(e => e.CategoryType == id);
        }
    }
}
