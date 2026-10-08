using EDA.Bus;
using EDA.Events;

namespace EDA.Services
{
    public class AccountService
    {
        private readonly IEventBus _bus;
        private readonly Dictionary<string, double> _accounts = new();

        public AccountService(IEventBus eventBus)
        {
            _bus = eventBus;
        }

        public void CreateAccount(string accountId, string owner)
        {
            _accounts[accountId] = 0;
            Log($"Created account {accountId} for {owner}");
            _bus.Publish(new AccountCreatedEvent(accountId, owner));
        }

        public void Deposit(string accountId, double amount)
        {
            EnsureAccount(accountId);
            EnsurePositive(amount);

            _accounts[accountId] += amount;
            Log($"Deposited {amount} to {accountId} (balance: {_accounts[accountId]})");
            _bus.Publish(new MoneyDepositedEvent(accountId, amount));
        }

        public void Withdraw(string accountId, double amount)
        {
            EnsureAccount(accountId);
            EnsurePositive(amount);
            if (_accounts[accountId] < amount)
                throw new InvalidOperationException($"Account {accountId} không đủ số dư.");

            _accounts[accountId] -= amount;
            Log($"Withdrawn {amount} from {accountId} (balance: {_accounts[accountId]})");
            _bus.Publish(new MoneyWithdrawnEvent(accountId, amount));
        }

        // YÊU CẦU 1: chuyển tiền A -> B, publish TransferMoneyEvent
        public void Transfer(string fromAccountId, string toAccountId, double amount)
        {
            EnsureAccount(fromAccountId);
            EnsureAccount(toAccountId);
            EnsurePositive(amount);
            if (fromAccountId == toAccountId)
                throw new InvalidOperationException("Không thể chuyển tiền cho chính mình.");
            if (_accounts[fromAccountId] < amount)
                throw new InvalidOperationException($"Account {fromAccountId} không đủ số dư.");

            _accounts[fromAccountId] -= amount;
            _accounts[toAccountId] += amount;
            Log($"Transferred {amount} from {fromAccountId} to {toAccountId} " +
                $"(balances: {_accounts[fromAccountId]} / {_accounts[toAccountId]})");
            _bus.Publish(new TransferMoneyEvent(fromAccountId, toAccountId, amount));
        }

        private void EnsureAccount(string id)
        {
            if (!_accounts.ContainsKey(id))
                throw new KeyNotFoundException($"Account {id} không tồn tại.");
        }

        private static void EnsurePositive(double amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Số tiền phải > 0.");
        }

        private static void Log(string msg) =>
            Console.WriteLine($"{DateTime.Now} [AccountService] {msg}");
    }
}
