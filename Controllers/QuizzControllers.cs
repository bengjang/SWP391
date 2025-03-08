using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lamlai.DTOs;
using lamlai.Models;


namespace test2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly TestContext _context;

        public QuizController(TestContext context)
        {
            _context = context;
        }

        // Lấy danh sách câu hỏi quiz
        [HttpGet("questions")]
        public async Task<ActionResult<IEnumerable<QuizQuestionDto>>> GetQuizQuestions()
        {
            var questions = await _context.QuizQuestions
                .Include(q => q.QuizAnswers)
                .ToListAsync();

            var questionDtos = questions.Select(q => new QuizQuestionDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                Answers = q.QuizAnswers.Select(a => new QuizAnswerDto
                {
                    AnswerId = a.AnswerId,
                    AnswerText = a.AnswerText,
                    SkinType = a.SkinType
                }).ToList()
            }).ToList();

            return Ok(questionDtos);
        }

        // Người dùng gửi câu trả lời và tính kết quả
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitQuizResponses([FromBody] QuizSubmissionDto submission)
        {
            var user = await _context.Users.FindAsync(submission.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Xóa các câu trả lời trước đó của người dùng nếu có
            var existingResponses = _context.UserQuizResponses
                .Where(r => r.UserId == submission.UserId)
                .ToList();
            _context.UserQuizResponses.RemoveRange(existingResponses);

            // Lưu các câu trả lời vào bảng UserQuizResponses
            foreach (var response in submission.Responses)
            {
                var userQuizResponse = new UserQuizResponse
                {
                    UserId = submission.UserId,
                    QuestionId = response.QuestionId,
                    SelectedAnswerId = response.SelectedAnswerId,  // Dùng SelectedAnswerId ở đây
                    AnsweredAt = DateTime.UtcNow
                };
                _context.UserQuizResponses.Add(userQuizResponse);
            }

            // Cập nhật SkinType cho người dùng dựa trên các câu trả lời
            var skinType = GetSkinType(submission.Responses);
            user.SkinType = skinType;

            // Lưu kết quả quiz vào UserSkinTypeResult
            var result = new UserSkinTypeResult
            {
                UserId = submission.UserId,
                AttemptNumber = _context.UserSkinTypeResults
                    .Count(r => r.UserId == submission.UserId) + 1, // Tăng số lần làm quiz
                SkinType = skinType,
                CreatedAt = DateTime.UtcNow
            };
            _context.UserSkinTypeResults.Add(result);

            await _context.SaveChangesAsync();

            return Ok(new { SkinType = skinType });
        }

        // Làm lại quiz cho người dùng
        [HttpPost("reset")]
        public async Task<IActionResult> ResetQuiz([FromBody] QuizSubmissionDto submission)
        {
            var user = await _context.Users.FindAsync(submission.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Xóa các câu trả lời cũ và kết quả quiz cũ của người dùng
            var existingResponses = _context.UserQuizResponses
                .Where(r => r.UserId == submission.UserId)
                .ToList();
            _context.UserQuizResponses.RemoveRange(existingResponses);

            var existingResults = _context.UserSkinTypeResults
                .Where(r => r.UserId == submission.UserId)
                .ToList();
            _context.UserSkinTypeResults.RemoveRange(existingResults);

            // Lưu lại kết quả quiz mới
            return await SubmitQuizResponses(submission);
        }

        // Hàm tính toán SkinType dựa trên các câu trả lời
        private string GetSkinType(List<QuizResponseDto> responses)
        {
            var skinTypeCount = new Dictionary<string, int>
            {
                { "Da dầu", 0 },
                { "Da thường", 0 },
                { "Da khô", 0 },
                { "Da hỗn hợp", 0 },
                { "Da nhạy cảm", 0 }
            };

            foreach (var response in responses)
            {
                var answer = _context.QuizAnswers
                    .FirstOrDefault(a => a.QuestionId == response.QuestionId && a.AnswerId == response.SelectedAnswerId);

                if (answer != null)
                {
                    var correctedSkinType = CorrectSkinType(answer.SkinType);
                    skinTypeCount[correctedSkinType]++;
                }
            }

            return skinTypeCount.OrderByDescending(kv => kv.Value).FirstOrDefault().Key;
        }

        // Sửa các lỗi SkinType nếu có
        private string CorrectSkinType(string skinType)
        {
            if (skinType == "Da d?u") return "Da dầu";  // Kiểm tra 'Da d?u' và thay thế bằng 'Da dầu'
            if (skinType == "Da thu?ng") return "Da thường"; // Kiểm tra 'Da thu?ng' và thay thế bằng 'Da thường'
            if (skinType == "Da kh?o") return "Da khô";  // Kiểm tra 'Da kh?o' và thay thế bằng 'Da khô'
            if (skinType == "Da h?m hợp") return "Da hỗn hợp"; // Kiểm tra 'Da h?m hợp' và thay thế bằng 'Da hỗn hợp'
            if (skinType == "Da nh?y cảm") return "Da nhạy cảm"; // Kiểm tra 'Da nh?y cảm' và thay thế bằng 'Da nhạy cảm'
            return skinType;  // Trả về skinType nếu không có sự thay đổi
        }
    }
}
