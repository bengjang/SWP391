namespace lamlai2.DTOs
{
    public class QuizSubmissionDto
    {
        public int UserId { get; set; }
        public List<QuizResponseDto> Responses { get; set; }
    }
}

