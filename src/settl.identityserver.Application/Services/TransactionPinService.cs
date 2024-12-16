using AutoMapper;
using Microsoft.EntityFrameworkCore;
using settl.identityserver.Appication.Contracts.RepositoryInterfaces;
using settl.identityserver.Application.Contracts.DTO.TransactionPIN;
using settl.identityserver.Domain.Entities;
using settl.identityserver.Domain.Shared.Enums;
using System.Linq;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Services
{
    public static class TransactionPinService
    {
        public static async Task<bool> ChangeTransactionPin(ChangeTransactionPinDTO changeTransactionPINDTO, IMapper mapper, IGenericRepository<Users> userRepository)
        {
            Users user = await userRepository.Query().Where(x => x.Id == changeTransactionPINDTO.Id).FirstOrDefaultAsync();

            if (user == null || !BCrypt.Net.BCrypt.Verify(changeTransactionPINDTO.OldTransactionPin, user.TransactionPin))
            {
                return false;
            }

            user.TransactionPin = BCrypt.Net.BCrypt.HashPassword(user.TransactionPin);

            var users = mapper.Map<Users>(changeTransactionPINDTO.TransactionPin);

            userRepository.Update(users);
            await userRepository.Save();

            return true;
        }

        public static async Task<bool> VerifyTransactionPin(VerifyTransactionPinDTO verifyTransactionPinDTO, IGenericRepository<Users> userRepository)
        {
            Users user = await userRepository.Query().Where(x => x.Id == verifyTransactionPinDTO.Id).FirstOrDefaultAsync();

            if (user == null || !BCrypt.Net.BCrypt.Verify(verifyTransactionPinDTO.TransactionPin, user.TransactionPin))
            {
                return false;
            }

            return true;
        }

        public static async Task<bool> ResetTransactionPin(ResetTransactionPinDTO resetTransactionPinDTO, IGenericRepository<Users> userRepository)
        {
            Users user = await userRepository.Query().Where(x => x.Id == resetTransactionPinDTO.Id).FirstOrDefaultAsync();

            if (user == null)
            {
                return false;
            }

            user.TransactionPin = BCrypt.Net.BCrypt.HashPassword(Constants.DefaultTransactionPIN);

            userRepository.Update(user);
            await userRepository.Save();

            return true;
        }
    }
}