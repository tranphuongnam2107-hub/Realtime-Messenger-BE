using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Model;

namespace Repositories.Interface
{
    public interface IAccountRepository
    {
        Task<Account?> GetAccountByIdentifier(string? identifier);
        Task<List<Account>?> GetAccountsByIdentifier(List<string>? accountIds);
        Task<Account?> UpdateLoginFail(string accId, int? failAttempts, DateTime? lockedUntil);
        Task<Account?> UpdateRefreshToken(string? acc_id, string? refreshToken, DateTime? expiry);
        Task<Account?> SearchUserByIdentify(string? identify);
        Task<List<Account>> GetMutualFriend(string? loggedUserId, string? toUserId);
        Task<List<Chat>> GetMutualGroup(string? loggedUserId, string? toUserId);
    }
}
