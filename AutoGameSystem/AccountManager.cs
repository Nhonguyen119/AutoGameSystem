using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoGameSystem.Models;
using AutoGameSystem.Utilities;

namespace AutoGameSystem.Core
{
    public class AccountManager
    {
        private List<Account> _accounts;

        public AccountManager()
        {
            _accounts = new List<Account>();
        }

        public void LoadAccounts()
        {
            _accounts = ConfigManager.LoadAccounts();
        }

        public void SaveAccounts()
        {
            ConfigManager.SaveAccounts(_accounts);
        }

        public Account GetNextAccount()
        {
            return _accounts
                .Where(a => a.IsActive)
                .OrderBy(a => a.LastRun)
                .FirstOrDefault();
        }

        public void AddAccount(Account account)
        {
            _accounts.Add(account);
            SaveAccounts();
        }

        public void RemoveAccount(string accountId)
        {
            _accounts.RemoveAll(a => a.Id == accountId);
            SaveAccounts();
        }

        public void UpdateAccount(Account updatedAccount)
        {
            var existingAccount = _accounts.FirstOrDefault(a => a.Id == updatedAccount.Id);
            if (existingAccount != null)
            {
                _accounts.Remove(existingAccount);
                _accounts.Add(updatedAccount);
                SaveAccounts();
            }
        }

        public List<Account> GetAllAccounts()
        {
            return _accounts.OrderBy(a => a.Name).ToList();
        }
    }
}
