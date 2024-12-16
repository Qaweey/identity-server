using AutoMapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using settl.identityserver.Appication.Contracts.RepositoryInterfaces;
using settl.identityserver.Application.Contracts.DTO.Consumer;
using settl.identityserver.Application.Contracts.DTO.SecurityAnswer;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Dapper;
using settl.identityserver.Domain.Entities;
using settl.identityserver.Domain.Shared.Helpers;
using settl.identityserver.Domain.Shared.Helpers.Authentication;
using settl.identityserver.Domain.Shared.Helpers.Cryptography;
using settl.identityserver.EntityFrameworkCore.AppDbContext;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Services
{
    public class SecurityAnswerService : ISecurityAnswerService
    {
        private readonly IMapper _mapper;
        private readonly IDapper _dapper;
        private readonly IApplicationDbContext _dbContext;
        private readonly IConsumerService _consumerService;
        private readonly IGenericRepository<Auth> _userRepository;
        private readonly IGenericRepository<SecurityAnswer> _securityAnswerRepository;

        public SecurityAnswerService(IMapper mapper, IApplicationDbContext dbContext, IDapper dapper, IGenericRepository<SecurityAnswer> securityAnswerRepository,
                                     IGenericRepository<Auth> userRepository, IConsumerService consumerService)
        {
            _mapper = mapper;
            _dapper = dapper;
            _dbContext = dbContext;
            _userRepository = userRepository;
            _consumerService = consumerService;
            _securityAnswerRepository = securityAnswerRepository;
        }

        public async Task<(bool, RefreshTokenRequest)> CreateConsumerSecurityAnswer(CreateSecurityAnswerForm model)
        {
            _dbContext.Connection.Open();
            using (var transaction = _dbContext.Connection.BeginTransaction())
            {
                try
                {
                    _dbContext.Database.UseTransaction(transaction as DbTransaction);
                    var usersql = $"Select * from [tbl_auth] where [Phone] = @Phone AND deleted = 0";

                    var dbArgs = new DynamicParameters();
                    dbArgs.Add("Phone", model.Phone);

                    var mainuser = await Task.FromResult(_dapper.Get<Auth>(usersql, dbArgs, commandType: CommandType.Text));

                    if (mainuser is null) throw new CustomException("User has not completed the first stage of onboarding");

                    bool isUssdUser = mainuser?.phone_name == "USSD";

                    //if (!isUssdUser) throw new CustomException("User has not completed the first stage of onboarding");

                    var questionsQuery = $"Select * from [tbl_security_answers] WHERE auth_id = @Id";

                    dbArgs.Add("Id", mainuser.Id);

                    var securityQuestions = await Task.FromResult(_dapper.Get<SecurityAnswer>(questionsQuery, dbArgs, commandType: CommandType.Text));

                    if (securityQuestions != null) throw new CustomException("User already has existing security questions.");

                    string[] questions = new string[3];

                    string[] answers = new string[3];
                    int count = 0;

                    foreach (var useranswer in model.SecurityAnswer.Select(o => new { o.QuestionId, o.Answer }).Distinct())
                    {
                        if (string.IsNullOrEmpty(useranswer.Answer)) throw new CustomException("Security answer cannot be empty.");

                        var getQst = $"Select * from [tbl_security_questions] where [id] = @qstId";
                        dbArgs.Add("qstId", useranswer.QuestionId);
                        var qst = await Task.FromResult(_dapper.Get<SecurityQuestion>(getQst, dbArgs, commandType: CommandType.Text));

                        if (qst is null) throw new CustomException($"No question for id {useranswer.QuestionId}");

                        questions[count] = qst.Question;
                        answers[count] = useranswer.Answer;
                        count++;
                    }
                    var currentAnswer = new SecurityAnswer
                    {
                        AuthId = mainuser.Id,
                        First_question = questions[0],
                        First_answer = answers[0],
                        Second_question = questions[1],
                        Second_answer = answers[1],
                        Third_question = questions.Length == 3 ? questions[2] : string.Empty,
                        Third_answer = answers.Length == 3 ? answers[2] : string.Empty
                    };

                    await _securityAnswerRepository.Add(currentAnswer);

                    //if (isUssdUser)
                    //{
                    //    if (mainuser is null) throw new CustomException("User has not registered from USSD");
                    //    //_tempUserRepository.Delete(user);
                    //    return (await _securityAnswerRepository.Save(), null);
                    //}

                    if (mainuser.consumer)
                    {
                        var consumer = new CreateConsumerDTO()
                        {
                            FirstName = mainuser.Firstname,
                            LastName = mainuser.Lastname,
                            Email = mainuser.email,
                            Phone = model.Phone,
                            ReferralCode = mainuser.referral_code,
                            UserName = mainuser?.username,
                            Gender = mainuser.gender
                        };

                        mainuser.enabled = true;
                        mainuser.IsActive = true;

                        var (refreshTokenRequest, _) = JWT.GenerateJwtToken(mainuser.phone, "CONSUMER");
                        mainuser.JwtToken = refreshTokenRequest.Token;
                        mainuser.RefreshToken = Hashing.Base64StringEncode($"{refreshTokenRequest.RefreshToken}phone{mainuser.phone}|{mainuser.phone_model_no}|{mainuser.imei_no}");

                        _userRepository.Update(mainuser);
                        var saved = await _userRepository.Save();
                        if (saved)
                        {
                            var (success, response) = await _consumerService.Create(consumer);

                            if (!success) throw new CustomException(response?.Message);

                            transaction.Commit();

                            return (success, refreshTokenRequest);
                        }

                        transaction.Rollback();
                        throw new CustomException("An error occured while saving user entries");
                    }
                    return (true, null);
                    //TODO: call agency endpoint
                }
                catch (CustomException)
                {
                    transaction.Rollback();
                    throw;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    _dbContext.Connection.Close();
                }
            }
        }

        public async Task<SecurityAnswer> GetSecurityAnswers(string phone)
        {
            var sql = @"SELECT tbl_security_answers.* FROM tbl_security_answers JOIN tbl_auth ON tbl_security_answers.auth_id = tbl_auth.id WHERE tbl_auth.phone = @phone";
            var dbArgs = new DynamicParameters();
            dbArgs.Add("Phone", phone);

            var answers = await Task.FromResult(_dapper.Get<SecurityAnswer>(sql, dbArgs, commandType: CommandType.Text));

            // map to ReadSecurityAnswerDTO

            return answers;
        }
    }
}