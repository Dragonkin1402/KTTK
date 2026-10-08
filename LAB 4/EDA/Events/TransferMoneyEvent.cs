namespace EDA.Events
{
    /// <summary>Chuyển tiền từ tài khoản A sang tài khoản B.</summary>
    public class TransferMoneyEvent : IEvent
    {
        public string FromAccountId { get; set; }
        public string ToAccountId { get; set; }
        public double Amount { get; set; }

        public TransferMoneyEvent(string fromAccountId, string toAccountId, double amount)
        {
            FromAccountId = fromAccountId;
            ToAccountId = toAccountId;
            Amount = amount;
        }
    }
}
