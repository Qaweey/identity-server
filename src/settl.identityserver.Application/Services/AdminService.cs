using AutoMapper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using settl.identityserver.Appication.Contracts.RepositoryInterfaces;
using settl.identityserver.Application.Contracts.DTO;
using settl.identityserver.Application.Contracts.DTO.Admin;
using settl.identityserver.Application.Contracts.IServices;
using settl.identityserver.Application.Contracts.RepositoryInterfaces;
using settl.identityserver.Dapper;
using settl.identityserver.Domain.Entities;
using settl.identityserver.Domain.Shared;
using settl.identityserver.Domain.Shared.Enums;
using settl.identityserver.Domain.Shared.Helpers;
using settl.identityserver.EntityFrameworkCore.AppDbContext;
using System;
using System.Data;
using System.Data.Common;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BCryptNet = BCrypt.Net.BCrypt;

namespace settl.identityserver.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IMapper _mapper;
        private readonly IDapper _dapper;
        private readonly IApplicationDbContext _dbContext;
        private readonly IGenericRepository<Auth> _userRepository;
        private readonly IGenericWithoutBaseEntityRepository<tbl_admin> _adminRepository;

        public AdminService(IDapper dapper, IMapper mapper, IGenericRepository<Auth> userRepository, IGenericWithoutBaseEntityRepository<tbl_admin> adminRepository, IApplicationDbContext dbContext)
        {
            _mapper = mapper;
            _dapper = dapper;
            _dbContext = dbContext;
            _userRepository = userRepository;
            _adminRepository = adminRepository;
        }

        public async Task<Auth> GetUser(string email, string phone)
        {
            var sql = $"Select * from [tbl_auth] where active = 1 and Email = @Email or Phone = @Phone";
            var dbArgs = new DynamicParameters();
            dbArgs.Add("Email", email);
            dbArgs.Add("Phone", phone);
            var admin = await Task.FromResult(_dapper.Get<Auth>(sql, dbArgs, commandType: CommandType.Text));

            return admin;
        }

        public async Task<Auth> GetUser(string phone)
        {
            var sql = $"Select * from [tbl_auth] where phone = @phone and admin = 1";
            var dbArgs = new DynamicParameters();
            dbArgs.Add("phone", phone);
            var adminRole = await Task.FromResult(_dapper.Get<Auth>(sql, dbArgs, commandType: CommandType.Text));

            if (adminRole is null) throw new CustomException("Admin doesn't exist");

            return adminRole;
        }

        public async Task<tbl_admin_role> GetAdminRole(string role)
        {
            var sql = $"Select * from [tbl_admin_roles] where role = @role";
            var dbArgs = new DynamicParameters();
            dbArgs.Add("role", role);
            var adminRole = await Task.FromResult(_dapper.Get<tbl_admin_role>(sql, dbArgs, commandType: CommandType.Text));

            if (adminRole is null) throw new CustomException("Role doesn't exist");

            return adminRole;
        }

        public async Task<tbl_admin> GetAdmin(int authId)
        {
            var sql = $"Select * from [tbl_admin] where auth_id = @id";
            var dbArgs = new DynamicParameters();
            dbArgs.Add("id", authId);
            var admin = await Task.FromResult(_dapper.Get<tbl_admin>(sql, dbArgs, commandType: CommandType.Text));

            if (admin is null) throw new CustomException("Admin user doesn't exist");

            return admin;
        }

        public async Task<ReadAdminDTO> GetAdminUser(int authId, string email = null, string phone = null)
        {
            string whereClause = "(dbo.tbl_auth.id = @id)";

            if (!string.IsNullOrEmpty(email)) whereClause = "(dbo.tbl_auth.email = @email)";

            if (!string.IsNullOrEmpty(phone)) whereClause = "(dbo.tbl_auth.phone = @phone)";

            var sql = @$"SELECT dbo.tbl_auth.id, dbo.tbl_auth.email, dbo.tbl_auth.phone AS PhoneNumber, dbo.tbl_auth.Firstname + ' ' + dbo.tbl_auth.Lastname AS Fullname, dbo.tbl_auth.Department, dbo.tbl_auth.active as IsActive, dbo.tbl_admin_roles.role FROM dbo.tbl_auth INNER JOIN dbo.tbl_admin ON dbo.tbl_auth.id = dbo.tbl_admin.auth_id INNER JOIN dbo.tbl_admin_roles ON dbo.tbl_admin.admin_roles_id = dbo.tbl_admin_roles.id WHERE {whereClause}";

            var dbArgs = new DynamicParameters();
            dbArgs.Add("id", authId);
            dbArgs.Add("email", email);
            dbArgs.Add("phone", phone);
            var admin = await Task.FromResult(_dapper.Get<ReadAdminDTO>(sql, dbArgs, commandType: CommandType.Text));

            if (admin is null) throw new CustomException("Admin user doesn't exist");

            admin.StaffId = "Settl-" + Guid.NewGuid().ToString()[..4];

            return admin;
        }

        public async Task<(bool, ResponsesDTO)> CreateAdmin(CreateAdminDTO createAdminDTO)
        {
            Auth admin = await GetUser(createAdminDTO.Email, createAdminDTO.PhoneNumber);

            if (admin is not null && admin.admin) return (false, Responses.Conflict("Email or Phone Number already taken."));

            var role = await GetAdminRole(createAdminDTO.Role);

            if (admin is null)
            {
                var _admin = _mapper.Map<Auth>(createAdminDTO);
                _admin.admin = true;
                _admin.IsActive = true;
                _admin.enabled = true;
                _admin.Firstname = createAdminDTO.Fullname.Split(" ")[0];
                _admin.Lastname = createAdminDTO.Fullname.Split(" ")[1];
                _admin.phone = createAdminDTO.PhoneNumber;
                _admin.secret = BCryptNet.HashPassword("00000000");
                await _userRepository.Create(_admin);

                await _userRepository.Save();

                admin = await GetUser(createAdminDTO.PhoneNumber);
            }
            else
            {
                admin.Department = createAdminDTO.Department;
                admin.admin = true;
                _userRepository.Update(admin);

                await _userRepository.Save();
            }

            var tblAdmin = new tbl_admin
            {
                auth_id = admin.Id,
                admin_roles_id = role.id
            };

            await _adminRepository.Create(tblAdmin);

            var saved = await _adminRepository.Save();

            var sendEmailDTO = new SendEmailDTO
            {
                Name = createAdminDTO.Fullname,
                Email = createAdminDTO.Email,
                EmailCode = "003",
                FromEmail = "notification@settl.me",
                FromName = "Settl BackOffice",
            };

            HttpClient client = new();
            var emailUrl = $"{Constants.EMAILSERVICES_URL}/email/send";
            var response = await client.PostAsJsonAsync(emailUrl, sendEmailDTO);

            var adminUser = await GetAdminUser(admin.Id);

            return response.IsSuccessStatusCode && saved ? (true, Responses.Ok(adminUser, "You have signed up successfully!")) : (true, Responses.Conflict(message: "Oops! Problem sending email to created admin"));
        }

        public async Task<(bool, ResponsesDTO)> SignInAdmin(SignInAdminDTO signInAdminDTO)
        {
            var admin = await GetUser(signInAdminDTO.Email, "");

            if (admin == null) return (false, Responses.NotFound("User not found."));

            if (!BCryptNet.Verify(signInAdminDTO.Password, admin.secret)) return (false, Responses.Conflict("Invalid password"));

            var result = await GetAdminUser(admin.Id);

            return (true, Responses.Ok(result));
        }

        public async Task<(bool, ResponsesDTO)> ChangePassword(ChangeAdminPassword changeAdminPassword)
        {
            var admin = await GetUser(changeAdminPassword.Email, "");

            if (admin is null) return (false, Responses.NotFound("Admin does not exist"));

            if (!BCryptNet.Verify(changeAdminPassword.OldPassword, admin.secret)) return (false, Responses.Conflict("Invalid password."));

            if (changeAdminPassword.OldPassword.ToLower() == changeAdminPassword.ConfirmPassword.ToLower()) return (false, Responses.Conflict("New password cannot be the same as old password"));

            if (BCryptNet.Verify(changeAdminPassword.ConfirmPassword, admin.secret)) return (false, Responses.Conflict("New password cannot be the same as old password"));

            admin.secret = BCryptNet.HashPassword(changeAdminPassword.Password);

            _userRepository.Update(admin);
            var success = await _adminRepository.Save();

            return success ? (success, Responses.Ok<object>("", "Password changed successfully")) : (success, Responses.Conflict("Could not change password"));
        }

        public async Task<(bool, ResponsesDTO)> UpdateStatus(DeactivateAdminDTO request)
        {
            var superAdmin = await GetAdminUser(0, request.SuperAdminEmail);

            if (superAdmin is null) return (false, Responses.NotFound("Superadmin does not exist"));

            if (superAdmin.Role != "SUPER_ADMIN") return (false, Responses.Conflict("Unauthorized to perform this process"));

            var admin = await GetAdminUser(0, request.Email);

            if (admin is null) return (false, Responses.NotFound("Admin does not exist"));

            var sql = $"UPDATE [tbl_auth] SET admin = @Status, updated_on = GETDATE() WHERE email = @Email";
            var dbArgs = new DynamicParameters();
            dbArgs.Add("Email", request.Email);
            dbArgs.Add("Status", request.Status);
            var rowsUpdated = await Task.FromResult(_dapper.Execute(sql, dbArgs, commandType: CommandType.Text));

            return rowsUpdated > 0 ? (true, Responses.Ok("Admin Status updated successfully")) : (false, Responses.Conflict("Unable to update admin status"));
        }

        public async Task<(bool, ResponsesDTO)> DeleteUser(DeleteAdminDTO request)
        {
            var superAdmin = await GetAdminUser(0, request.SuperAdminEmail);

            if (superAdmin is null) return (false, Responses.NotFound("Superadmin does not exist"));

            if (superAdmin.Role != "SUPER_ADMIN") return (false, Responses.Conflict("Unauthorized to perform this process"));

            var admin = await GetUser(request.Email, "");

            if (admin is null) return (false, Responses.NotFound("Admin does not exist"));

            var sql = $"UPDATE [tbl_auth] SET admin = 0 and updated_on = GETDATE() WHERE Email = @Email and admin = 1";
            var dbArgs = new DynamicParameters();
            dbArgs.Add("Email", request.Email);
            var rowsUpdated = await Task.FromResult(_dapper.Execute(sql, dbArgs, commandType: CommandType.Text));

            return rowsUpdated > 0 ? (true, Responses.Ok("Deleted successfully")) : (false, Responses.Conflict("Unable to delete admin"));
        }

        public async Task<(bool, ResponsesDTO)> ForgotPassword(ForgotAdminPasswordDTO request)
        {
            var admin = await GetUser(request.Email, "");

            if (admin is null) return (false, Responses.NotFound("Admin does not exist"));

            if (request.Otp != "12345") return (false, Responses.Conflict("Invalid verification code"));

            if (BCryptNet.Verify(request.ConfirmPassword, admin.secret)) return (false, Responses.Conflict("New password cannot be the same as old password"));

            admin.secret = BCryptNet.HashPassword(request.ConfirmPassword);

            _userRepository.Update(admin);
            var success = await _adminRepository.Save();

            return success ? (success, Responses.Ok("Password reset successfully")) : (success, Responses.Conflict("Unable to reset password at this time"));
        }

        public async Task<(bool, AdminResponseDTO)> GetAdminManagement(string email)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-RequestId", Guid.NewGuid().ToString());
            client.DefaultRequestHeaders.Add("X-Settl-Api-Token", Constants.SETTL_API_TOKEN);
            var adminUrl = $"{Constants.BACKOFFICE_URL}/adminmanagement/admin?email={email}";
            var response = await client.GetAsync(adminUrl);
            var json = await response.Content.ReadAsStringAsync();
            Log.Information($"Admin Response - {json}");
            var result = JsonConvert.DeserializeObject<AdminResponseDTO>(json);

            return (response.IsSuccessStatusCode, result);
        }

        public async Task<(bool, ResponsesDTO)> UpdateAdmin(UpdateAdminDto admin)
        {
            var adminDetail = await GetUser(admin.Email, "");

            if (adminDetail is null) return (false, Responses.NotFound("Admin does not exist"));

            var sql = $"UPDATE [tbl_auth] SET Department=@Department,updated_on=@UpdatedOn WHERE Email = @Email";
            var dbArgs = new DynamicParameters();
            dbArgs.Add("Email", admin.Email);
            dbArgs.Add("Department", admin.Department);
            dbArgs.Add("UpdatedOn", DateHelper.GetCurrentLocalTime());

            var rowsUpdated = await Task.FromResult(_dapper.Execute(sql, dbArgs, commandType: CommandType.Text));

            var adminUser = await GetAdmin(adminDetail.Id);

            var role = await GetAdminRole(admin.Role);

            adminUser.admin_roles_id = role.id;

            _adminRepository.Update(adminUser);

            var success = await _adminRepository.Save();

            var result = await GetAdminUser(adminDetail.Id);

            return rowsUpdated > 0 && success ? (true, Responses.Ok(result, "Admin Status updated successfully")) : (false, Responses.Conflict("Unable to update admin"));
        }
    }
}