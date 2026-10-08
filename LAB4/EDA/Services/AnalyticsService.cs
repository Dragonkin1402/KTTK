using EDA.Bus;
using EDA.Events;

namespace EDA.Services
{
    public class AnalyticsService
    {
        private double _totalDeposited = 0;
        private double _totalWithdrawn = 0;
        private double _totalTransferred = 0;

        public AnalyticsService(IEventBus bus)
        {
            bus.Subscribe<MoneyDepositedEvent>(HandleDeposit);
            bus.Subscribe<MoneyWithdrawnEvent>(HandleWithdrawn);
            bus.Subscribe<TransferMoneyEvent>(HandleTransfer);
        }

        private void HandleDeposit(MoneyDepositedEvent e)
        {
            _totalDeposited += e.Amount;
            Console.WriteLine($"{DateTime.Now} [AnalyticService] Total Deposited: {_totalDeposited}");
        }

        private void HandleWithdrawn(MoneyWithdrawnEvent e)
        {
            _totalWithdrawn += e.Amount;
            Console.WriteLine($"{DateTime.Now} [AnalyticService] Total Withdrawn: {_totalWithdrawn}");
        }

        private void HandleTransfer(TransferMoneyEvent e)
        {
            _totalTransferred += e.Amount;
            Console.WriteLine($"{DateTime.Now} [AnalyticService] Total Transferred: {_totalTransferred}");
        }
    }
}
