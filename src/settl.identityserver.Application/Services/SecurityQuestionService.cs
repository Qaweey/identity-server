using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Serilog;
using settl.identityserver.Appication.Contracts.RepositoryInterfaces;
using settl.identityserver.Application.Contracts.DTO.SecurityAnswer;
using settl.identityserver.Application.Contracts.DTO.SecurityQuestion;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Domain.Entities;
using settl.identityserver.Domain.Shared;
using settl.identityserver.Domain.Shared.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Services
{
    public class SecurityQuestionService : ISecurityQuestionService
    {
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Auth> _userRepository;
        private readonly ISecurityAnswerService _securityAnswerService;
        private readonly IGenericRepository<SecurityQuestion> _securityQuestionRepository;
        private readonly IGenericRepository<SecurityAnswer> _securityAnswerRepository;

        public SecurityQuestionService(ISecurityAnswerService securityAnswerService, IMapper mapper, IGenericRepository<SecurityAnswer> securityAnswerRepository, IGenericRepository<SecurityQuestion> securityQuestionRepository, IGenericRepository<Auth> userRepository)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _securityAnswerService = securityAnswerService;
            _securityQuestionRepository = securityQuestionRepository;
            _securityAnswerRepository = securityAnswerRepository;
        }

        public List<SecurityQuestionDTO> GetSecurityQuestion()
        {
            try
            {
                var questions = _securityQuestionRepository.GetAll().ToList();

                var questionsDTO = new List<SecurityQuestionDTO>();

                foreach (var question in questions)
                {
                    questionsDTO.Add(_mapper.Map<SecurityQuestionDTO>(question));
                }

                return questionsDTO;
            }
            catch (CustomException ex)
            {
                Log.Error(ex.Message);
                throw;
            }
        }

        public SecurityQuestion GetSecurityQuestionById(int id)
        {
            try
            {
                var question = _securityQuestionRepository.Get(x => x.Id == id).Result;

                if (question == null) throw new CustomException($"No question with id {id}");

                return question;
            }
            catch (CustomException ex)
            {
                Log.Error(ex.Message);
                throw;
            }
        }

        public async Task<ResponsesDTO> GetSelectedSecurityQuestion(string phone)
        {
            var user = await _userRepository.Query().Where(x => x.phone == phone && x.consumer).FirstOrDefaultAsync();

            if (user == null)
            {
                return Responses.NotFound("Phone number does not exist.");
            }

            var result = new SecurityQuestionSelectedDTO { AuthId = user.Id };

            var question = await _securityAnswerRepository.Query().FirstOrDefaultAsync(x => x.AuthId == user.Id);

            if (question is null) return Responses.NotFound("Security questions do not exist.");

            result.Questions.Add(new SecurityQuestionDTO
            {
                Id = _securityQuestionRepository.Get(x => x.Question == question.First_question).Result.Id,
                Question = question.First_question
            });

            result.Questions.Add(new SecurityQuestionDTO
            {
                Id = _securityQuestionRepository.Get(x => x.Question == question.Second_question).Result.Id,
                Question = question.Second_question
            });

            if (!string.IsNullOrEmpty(question.Third_question))
            {
                result.Questions.Add(new SecurityQuestionDTO
                {
                    Id = _securityQuestionRepository.Get(x => x.Question == question.Third_question).Result.Id,
                    Question = question.Third_question
                });
            }

            return Responses.Ok(result, "Security questions retrieved sucessfully.");
        }

        public async Task<(bool, string)> VerifyAnswers(VerifyAnswerForm model)
        {
            var user = await _userRepository.Query().FirstOrDefaultAsync(x => x.phone == model.phone && x.consumer);

            if (user == null) return (false, "User does not exist.");

            var answersResponse = _securityAnswerService.GetSecurityAnswers(model.phone).Result;

            if (answersResponse is null) return (false, "No questions have been answered by this user");

            var answersToVerify = model.securityAnswers.OrderBy(a => a.questionId).ToList();

            string[] answers = new string[] { answersResponse.First_answer, answersResponse.Second_answer, answersResponse.Third_answer };

            string[] questions = new string[] { answersResponse.First_question, answersResponse.Second_question, answersResponse.Third_question };

            foreach (var answerToVerify in answersToVerify)
            {
                var result = GetSecurityQuestionById(answerToVerify.questionId);

                if (!questions.Contains(result.Question)) return (false, $"You didn't create an answer for {result.Question}");

                if (!answers.Contains(answerToVerify.answer)) return (false, $"Wrong answer for: {result.Question}");
            }

            return (true, "Security answers verified sucessfully.");
        }
    }
}