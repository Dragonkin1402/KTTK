namespace EDA.Events
{
    // Bản gốc thiếu ": IEvent" nên không Subscribe/Publish được.
    public class MoneyWithdrawnEvent : IEvent
    {
        public string AccountId { get; set; }
        public double Amount { get; set; }

        public MoneyWithdrawnEvent(string accountId, double amount)
        {
            AccountId = accountId;
            Amount = amount;
        }
    }
}
