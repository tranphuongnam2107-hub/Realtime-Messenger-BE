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
        Task<Account?> UpdateLoginFail(string accId, int? failAttempts, DateTime? lockedUntil);
        Task<Account?> UpdateRefreshToken(string? acc_id, string? refreshToken, DateTime? expiry);
    }
}
