using AutoMapper;
using Microsoft.EntityFrameworkCore;
using settl.identityserver.Appication.Contracts.RepositoryInterfaces;
using settl.identityserver.Application.Contracts.DTO.Onboarding;
using settl.identityserver.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace settl.identityserver.Application.Services
{
    public static class OnboardingService
    {
        public static async Task<bool> Create(OnboardingDTO onboardingDTO, IMapper mapper, IGenericRepository<tbl_user_on_boarding> userOnboardingRepository)
        {
            var OnbaordingPhoneExist = await userOnboardingRepository.Query().Where(x => x.phone == onboardingDTO.Phone && x.stage == onboardingDTO.Stage).FirstOrDefaultAsync();

            var mappedOnboarding = mapper.Map<tbl_user_on_boarding>(onboardingDTO);

            if (OnbaordingPhoneExist != null)
            {
                OnbaordingPhoneExist.stage = onboardingDTO.Stage;
                userOnboardingRepository.Update(OnbaordingPhoneExist);
                return await userOnboardingRepository.Save();
            }

            await userOnboardingRepository.Add(mappedOnboarding);
            return await userOnboardingRepository.Save();
        }
    }
}