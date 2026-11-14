namespace Assignment4_EF_Core_.Models
{
    public class Transaction
    {
        public int Id { get; set; } 
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = string.Empty; 
        public string? Note { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public Account Account { get; set; } = null!;
    }
}
