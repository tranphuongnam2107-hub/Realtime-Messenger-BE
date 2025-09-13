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

namespace Services.Implement
{
    public class AuthenticationService : Services.Interface.IAuthenticationService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly PasswordHasher _hasher;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AuthenticationService(IAccountRepository accountRepository, PasswordHasher hasher, IMapper mapper, IConfiguration configuration, IRoleRepository roleRepository)
        {
            _accountRepository = accountRepository;
            _hasher = hasher;
            _mapper = mapper;
            _configuration = configuration;
            _roleRepository = roleRepository;
        }

        public async Task<BaseResponseDTO<LoginResponseDTO>?> Login(LoginRequestDTO request)
        {
            if(request.Identifier == null || request.Password == null)
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

        public Task<BaseResponseDTO<LoginResponseDTO>?> Logout(string accountId)
        {
            return null;
        }

        private async Task<BaseResponseDTO<LoginResponseDTO>> GenerateToken(Account account)
        {
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!);
            var tokenValidityMins = _configuration.GetValue<int>("JwtSettings:TokenValidMins");
            var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMins);

            var role = await _roleRepository.GetRoleById(account.RoleId);

            if (role == null)
            {
                role = new Role
                {
                    RoleId = "68c54b43d124de3e61199acb", // RoleId mặc định nếu không tìm thấy
                    RoleName = "USER",              
                    RoleDescription = "user"
                };
            }

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims: new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Name, account.AccountId),
                    new Claim(ClaimTypes.Email, account.Email),
                    new Claim("AccId", account.AccountId),
                    new Claim("RoleId", account.RoleId),
                    new Claim(ClaimTypes.Role, role.RoleName) // Sử dụng tên vai trò trong token
                },
                expires: tokenExpiryTimeStamp,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            var loginResponseDto = _mapper.Map<LoginResponseDTO>(account);
            loginResponseDto.AccessToken = accessToken;
            loginResponseDto.RefreshToken = await GenerateRefreshToken(account);

            return BaseResponseDTO<LoginResponseDTO>.Success("Login successfully.", loginResponseDto, 200);
        }

        private async Task<string?> GenerateRefreshToken(Account account)
        {
            var refreshTokenValidMins = _configuration.GetValue<int>("JwtSettings:RefreshTokenValidMins");

            if (account == null) return null;

            string newRefreshToken = Guid.NewGuid().ToString();
            DateTime newExpiry = DateTime.UtcNow.AddMinutes(refreshTokenValidMins);

            await _accountRepository.UpdateRefreshToken(account.AccountId, newRefreshToken, newExpiry);

            return newRefreshToken;
        }
    }
}
