namespace EDA.Events
{
    public class MoneyDepositedEvent : IEvent
    {
        public string AccountId { get; set; }
        public double Amount { get; set; }

        public MoneyDepositedEvent(string accountId, double amount)
        {
            AccountId = accountId;
            Amount = amount;
        }
    }
}
