namespace Assignment4_EF_Core_.Models
{
    public class CustomerAccount
    {
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;
        public string AccountNumber { get; set; } = string.Empty;
        public Account Account { get; set; } = null!;
        public OwnershipRole OwnershipRole { get; set; }
        public DateTime OwnershipStartDate { get; set; }
    }
}
