using settl.identityserver.Application.Contracts.DTO.Admin;
using settl.identityserver.Domain.Entities;
using settl.identityserver.Domain.Shared;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Contracts.IServices
{
    public interface IAdminService
    {
        public Task<(bool, ResponsesDTO)> CreateAdmin(CreateAdminDTO createAdminDTO);

        public Task<(bool, ResponsesDTO)> SignInAdmin(SignInAdminDTO signInAdminDTO);

        Task<Auth> GetUser(string email, string phone);

        Task<tbl_admin> GetAdmin(int authId);

        Task<ReadAdminDTO> GetAdminUser(int authId, string email = null, string phone = null);

        Task<(bool, ResponsesDTO)> ChangePassword(ChangeAdminPassword changeAdminPassword);

        Task<(bool, AdminResponseDTO)> GetAdminManagement(string email);

        Task<(bool, ResponsesDTO)> UpdateStatus(DeactivateAdminDTO changeAdminPassword);

        Task<(bool, ResponsesDTO)> UpdateAdmin(UpdateAdminDto admin);

        Task<(bool, ResponsesDTO)> DeleteUser(DeleteAdminDTO changeAdminPassword);

        Task<(bool, ResponsesDTO)> ForgotPassword(ForgotAdminPasswordDTO forgotAdminPassword);
    }
}