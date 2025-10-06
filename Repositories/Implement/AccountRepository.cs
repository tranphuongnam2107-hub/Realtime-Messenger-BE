using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.DAOs;
using Models.Model;
using Repositories.Interface;

namespace Repositories.Implement
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AccountDAO _accountDao;

        public AccountRepository(AccountDAO accountDao)
        {
            _accountDao = accountDao;
        }

        public async Task<Account?> GetAccountByIdentifier(string? identifier)
        {
            return await _accountDao.GetAccountByIdentify(identifier);
        }

        public async Task<List<Account>?> GetAccountsByIdentifier(List<string>? accountIds)
        {
            return await _accountDao.GetAccountsByIdentifiersAsync(accountIds);
        }

        public async Task<Account?> UpdateLoginFail(string accId, int? failAttempts, DateTime? lockedUntil)
        {
            return await _accountDao.UpdateLoginFailAsync(accId, failAttempts, lockedUntil);
        }

        public async Task<Account?> UpdateRefreshToken(string? accId, string? refreshToken, DateTime? expiry)
        {
            return await _accountDao.UpdateRefreshTokenAsync(accId, refreshToken, expiry);
        }
    }
}
