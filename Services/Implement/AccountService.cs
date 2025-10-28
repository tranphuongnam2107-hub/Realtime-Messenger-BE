using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using Models.DTO;
using Models.DTO.Response;
using Repositories.Interface;
using Services.Interface;

namespace Services.Implement
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IUserContextService _userContextService;
        private readonly IMapper _mapper;

        public AccountService(IAccountRepository accountRepository, IUserContextService userContextService, IMapper mapper)
        {
            _accountRepository = accountRepository;
            _userContextService = userContextService;
            _mapper = mapper;
        }

        public async Task<BaseResponseDTO<ProfileResponseDTO>> SearchUser(string? request)
        {
            var loggedUserId = _userContextService.GetAccountIdFromToken();

            var isValidIdentify = IsValidIdentify(request);
            if (!isValidIdentify)
                return BaseResponseDTO<ProfileResponseDTO>.Fail("Identify must be a valid email or phone number.", null, null, 400);

            var searchUser = await _accountRepository.SearchUserByIdentify(request);

            if (searchUser == null)
                return BaseResponseDTO<ProfileResponseDTO>.Fail("Not found this user", null, null, 404);

            //Mapping account to profile DTO
            var searchUserProfile = _mapper.Map<ProfileResponseDTO>(searchUser);

            //Lấy bạn chung và nhóm chung với loggedUserId
            var mutualFriends = await _accountRepository.GetMutualFriend(loggedUserId, searchUser.AccountId);
            var mutualGroups = await _accountRepository.GetMutualGroup(loggedUserId, searchUser.AccountId);

            // Mapping List<Account> -> List<ProfileResponseDTO>
            var mutualFriendProfiles = _mapper.Map<List<ProfileResponseDTO>>(mutualFriends);

            searchUserProfile.MutualFriends = mutualFriendProfiles;
            searchUserProfile.MutualChatGroups = mutualGroups;

            return BaseResponseDTO<ProfileResponseDTO>.Success("Search user successfully", searchUserProfile, 200);
        }

        private bool IsValidIdentify(string? identify)
        {
            if (string.IsNullOrWhiteSpace(identify))
                return false;

            identify = identify.Trim().Replace(" ", string.Empty);

            // Regex kiểm tra email
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            // Regex kiểm tra số điện thoại (chấp nhận +84 hoặc 0 đầu, tối đa 15 số)
            var phonePattern = @"^(?:\+84|0)(?:\d){8,14}$";

            bool isEmail = Regex.IsMatch(identify, emailPattern, RegexOptions.IgnoreCase);
            bool isPhone = Regex.IsMatch(identify, phonePattern);

            return isEmail || isPhone;
        }
    }
}
