namespace EDA.Events
{
    public class AccountCreatedEvent : IEvent
    {
        public string AccountId { get; set; }
        public string Owner { get; set; }

        public AccountCreatedEvent(string accountId, string owner)
        {
            AccountId = accountId;
            Owner = owner;
        }
    }
}
