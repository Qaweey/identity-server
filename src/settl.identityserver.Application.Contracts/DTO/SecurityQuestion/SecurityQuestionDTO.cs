using System.Collections.Generic;

namespace settl.identityserver.Application.Contracts.DTO.SecurityQuestion
{
    public class SecurityQuestionDTO
    {
        public int Id { get; set; }
        public string Question { get; set; }
    }

    public class SecurityQuestionSelectedDTO
    {
        public int AuthId { get; set; }

        public List<SecurityQuestionDTO> Questions { get; set; } = new();
    }
}