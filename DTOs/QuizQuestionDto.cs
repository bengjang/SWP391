namespace lamlai.DTOs
{
    public class QuizQuestionDto
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public List<QuizAnswerDto> Answers { get; set; }
    }
}
