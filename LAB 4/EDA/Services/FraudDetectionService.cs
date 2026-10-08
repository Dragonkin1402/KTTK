using EDA.Bus;
using EDA.Events;

namespace EDA.Services
{
    /// <summary>Subscribe MoneyWithdrawnEvent, cảnh báo giao dịch rút tiền bất thường (> 10000).</summary>
    public class FraudDetectionService
    {
        public const double Threshold = 10000;

        public FraudDetectionService(IEventBus bus)
        {
            bus.Subscribe<MoneyWithdrawnEvent>(HandleWithdrawn);
        }

        private void HandleWithdrawn(MoneyWithdrawnEvent e)
        {
            if (e.Amount > Threshold)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{DateTime.Now} [FraudDetection] !!! CẢNH BÁO: rút {e.Amount} " +
                                  $"từ tài khoản {e.AccountId} vượt ngưỡng {Threshold}");
                Console.ForegroundColor = color;
            }
            else
            {
                Console.WriteLine($"{DateTime.Now} [FraudDetection] Giao dịch rút {e.Amount} " +
                                  $"từ {e.AccountId} bình thường.");
            }
        }
    }
}
