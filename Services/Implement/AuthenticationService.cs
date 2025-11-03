using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Models.DTO.Response;
using Models.DTO;
using Services.Interface;
using Models.DTO.Request;
using Repositories.Interface;
using Services.PaswordHashing;
using AutoMapper;
using Models.Model;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Services.Config.JwtConfig;
using Microsoft.Extensions.Options;

namespace Services.Implement
{
    public class AuthenticationService : Services.Interface.IAuthenticationService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly PasswordHasher _hasher;
        private readonly IMapper _mapper;
        private readonly JwtSetting _jwtSetting;
        private readonly IUserContextService _userContextService;

        public AuthenticationService(IAccountRepository accountRepository, PasswordHasher hasher, IMapper mapper, IRoleRepository roleRepository, IOptions<JwtSetting> jwtSettings, IUserContextService userContextService)
        {
            _accountRepository = accountRepository;
            _hasher = hasher;
            _mapper = mapper;
            _roleRepository = roleRepository;
            _jwtSetting = jwtSettings.Value;
            _userContextService = userContextService;
        }

        public async Task<BaseResponseDTO<LoginResponseDTO>?> Login(LoginRequestDTO request)
        {
            if (request.Identifier == null || request.Password == null)
                return BaseResponseDTO<LoginResponseDTO>.Fail("Request is invalid.", null, null, 400);

            var account = await _accountRepository.GetAccountByIdentifier(request.Identifier);
            if (account == null)
                return BaseResponseDTO<LoginResponseDTO>.Fail("Account does not exist or has been deleted.", null, null, 404);

            //KIỂM TRA XEM TÀI KHOẢN CÓ BỊ KHÓA LOGIN HAY KHÔNG
            if (account.LockedUntil != null && account.LockedUntil > DateTime.UtcNow)
            {
                return BaseResponseDTO<LoginResponseDTO>.Fail("This account is currently locked.", null, null, 423);
            }

            //Kiểm tra xem có đúng password hay không
            if (!_hasher.VerifyPassword(request.Password, account.PasswordHash))
            {
                //Kiểm tra xem trường FailedAttempts có bị null hay không
                int failNumb = account.FailedAttempts ?? 0;
                failNumb++;

                if (account.FailedAttempts >= 5)
                {
                    //Reset lại số lần thất bại thành 0 và cập nhật thời gian khóa mới
                    var lockedUntil = DateTime.UtcNow.AddMinutes(3);
                    var accAfterLock = await _accountRepository.UpdateLoginFail(account.AccountId, 0, lockedUntil);

                    var loginResponseDto = _mapper.Map<LoginResponseDTO>(accAfterLock);

                    return BaseResponseDTO<LoginResponseDTO>.Fail("Your username or password is incorrect.", loginResponseDto, null, 401);

                }
                else
                {
                    await _accountRepository.UpdateLoginFail(account.AccountId, failNumb, null);
                    return BaseResponseDTO<LoginResponseDTO>.Fail("Your username or password is incorrect.", null, null, 401);
                }
            }

            // Reset số lần fail nếu đăng nhập thành công
            await _accountRepository.UpdateLoginFail(account.AccountId, 0, null);

            return await GenerateToken(account);
        }

        public async Task<BaseResponseDTO<LoginResponseDTO>?> Logout()
        {
            var accountId = _userContextService.GetAccountIdFromToken();

            if (accountId == null)
                return BaseResponseDTO<LoginResponseDTO>.Fail("Unauthorized: Missing user context.", null, null, 403);

            await _accountRepository.DeleteRefreshToken(accountId);
            return BaseResponseDTO<LoginResponseDTO>.Success("Logout successfully", null, 200);
        }

        public async Task<BaseResponseDTO<LoginResponseDTO>> ValidateRefreshToken(string? refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return BaseResponseDTO<LoginResponseDTO>.Fail("Unauthorized: Missing user context.", null, null, 403);

            //Lấy account dựa trên refreshToken
            var account = await _accountRepository.GetAccountByRefreshToken(refreshToken);

            //Nếu refreshToken không có hoặc có nhưng hết hạn thì return null
            if (account == null || account.TokenExpiry < DateTime.UtcNow) 
                return BaseResponseDTO<LoginResponseDTO>.Fail("Unauthorized: Missing user context.", null, null, 403);

            var response = await GenerateToken(account);

            //Reset value 2 field RefreshToken và Expiry trước khi tạo mới
            await _accountRepository.UpdateRefreshToken(
                account.AccountId,
                response.DataRes.RefreshToken,
                response.DataRes.RefreshTokenExpiry
            );

            //Gọi GenerateToken để tạo accessToken và refreshToken mới
            return response;
        }

        private async Task<BaseResponseDTO<LoginResponseDTO>> GenerateToken(Account account)
        {
            // Tính thời gian hết hạn token
            var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(_jwtSetting.TokenValidMins);

            // Lấy role từ DB
            var role = await _roleRepository.GetRoleById(account.RoleId);

            // Nếu không tìm thấy role -> set mặc định
            if (role == null)
            {
                role = new Role
                {
                    RoleId = "68c54b43d124de3e61199acb",
                    RoleName = "USER",
                    RoleDescription = "Default user role"
                };
            }

            // Tạo danh sách claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Name, account.AccountId),
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.MobilePhone, account.PhoneNumber),
                new Claim("RoleId", account.RoleId),
                new Claim(ClaimTypes.Role, role.RoleName)
            };

            // Tạo signing credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Tạo JWT token
            var token = new JwtSecurityToken(
                issuer: _jwtSetting.Issuer,
                audience: _jwtSetting.Audience,
                claims: claims,
                expires: tokenExpiryTimeStamp,
                signingCredentials: creds
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            // Mapping sang DTO trả về
            var loginResponseDto = _mapper.Map<LoginResponseDTO>(account);
            loginResponseDto.AccessToken = accessToken;
            loginResponseDto.RefreshToken = await GenerateRefreshToken(account);
            loginResponseDto.RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSetting.RefreshTokenValidMins);

            return BaseResponseDTO<LoginResponseDTO>.Success("Login successfully.", loginResponseDto, 200);
        }

        private async Task<string?> GenerateRefreshToken(Account account)
        {
            var refreshTokenValidMins = _jwtSetting.RefreshTokenValidMins;

            if (account == null) return null;

            string newRefreshToken = Guid.NewGuid().ToString();
            DateTime newExpiry = DateTime.UtcNow.AddMinutes(refreshTokenValidMins);

            await _accountRepository.UpdateRefreshToken(account.AccountId, newRefreshToken, newExpiry);

            return newRefreshToken;
        }

        
    }
}
