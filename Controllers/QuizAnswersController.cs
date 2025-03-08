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
    public class QuizAnswersController : ControllerBase
    {
        private readonly TestContext _context;

        public QuizAnswersController(TestContext context)
        {
            _context = context;
        }

        // GET: api/QuizAnswers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuizAnswer>>> GetQuizAnswers()
        {
            return await _context.QuizAnswers.ToListAsync();
        }

        // GET: api/QuizAnswers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QuizAnswer>> GetQuizAnswer(int id)
        {
            var quizAnswer = await _context.QuizAnswers.FindAsync(id);

            if (quizAnswer == null)
            {
                return NotFound();
            }

            return quizAnswer;
        }

        // PUT: api/QuizAnswers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuizAnswer(int id, QuizAnswer quizAnswer)
        {
            if (id != quizAnswer.AnswerId)
            {
                return BadRequest();
            }

            _context.Entry(quizAnswer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuizAnswerExists(id))
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

        // POST: api/QuizAnswers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<QuizAnswer>> PostQuizAnswer(QuizAnswer quizAnswer)
        {
            _context.QuizAnswers.Add(quizAnswer);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetQuizAnswer", new { id = quizAnswer.AnswerId }, quizAnswer);
        }

        // DELETE: api/QuizAnswers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuizAnswer(int id)
        {
            var quizAnswer = await _context.QuizAnswers.FindAsync(id);
            if (quizAnswer == null)
            {
                return NotFound();
            }

            _context.QuizAnswers.Remove(quizAnswer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QuizAnswerExists(int id)
        {
            return _context.QuizAnswers.Any(e => e.AnswerId == id);
        }
    }
}
