using AutoMapper.Configuration.Annotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace settl.identityserver.Application.Contracts.DTO.SecurityAnswer
{
    public class CreateSecurityAnswerDTO
    {
        [Required]
        public int QuestionId { get; set; }

        [Required]
        public string Answer { get; set; }

        [Required]
        [Ignore]
        [RegularExpression(@"0([7][0]|[8,9][0,1])\d{8}$", ErrorMessage = "Invalid Phone Number Format")]
        public string Phone { get; set; }
    }

    public class SecurityAnswerDTO
    {
        [Required]
        public int QuestionId { get; set; }

        [Required]
        public string Answer { get; set; }
    }

    public class CreateSecurityAnswerForm
    {
        [Required]
        [RegularExpression(@"0([7][0]|[8,9][0,1])\d{8}$", ErrorMessage = "Invalid Phone Number Format")]
        public string Phone { get; set; }

        public List<SecurityAnswerDTO> SecurityAnswer { get; set; }
    }

    public class ReadSecurityAnswerDTO
    {
        [RegularExpression(@"0([7][0]|[8,9][0,1])\d{8}$", ErrorMessage = "Invalid Phone Number Format")]
        public string Phone { get; set; }

        public int QuestionId { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
    }

    public class VerifyAnswerForm
    {
        public string imei { get; set; }
        public string phoneModelNo { get; set; }

        [Required]
        public string phone { get; set; }

        [Required]
        public Securityanswer[] securityAnswers { get; set; }
    }

    public class Securityanswer
    {
        [Required]
        public int questionId { get; set; }

        [Required]
        public string answer { get; set; }
    }
}