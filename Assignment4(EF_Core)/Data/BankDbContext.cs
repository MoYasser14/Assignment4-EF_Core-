using Assignment4_EF_Core_.Models;
using Microsoft.EntityFrameworkCore;

namespace Assignment4_EF_Core_.Data
{
    public class BankDbContext : DbContext
    {
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<CustomerAccount> CustomerAccounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=BankManagementDb;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Branch>().HasKey(b => b.Code);
            modelBuilder.Entity<Branch>().Property(b => b.Name).IsRequired().HasMaxLength(150);

            modelBuilder.Entity<Branch>()
                .HasOne(b => b.Manager)
                .WithOne(m => m.Branch)
                .HasForeignKey<Manager>(m => m.BranchCode)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Account>().HasKey(a => a.AccountNumber);
            modelBuilder.Entity<Account>().Property(a => a.CurrentBalance).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Branch)
                .WithMany(b => b.Accounts)
                .HasForeignKey(a => a.BranchCode)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>().HasKey(c => c.Id);
            modelBuilder.Entity<Customer>().Property(c => c.FullName).IsRequired().HasMaxLength(150);
            modelBuilder.Entity<Customer>().Property(c => c.NationalId).IsRequired().HasMaxLength(20);

            modelBuilder.Entity<CustomerAccount>()
                .HasKey(ca => new { ca.CustomerId, ca.AccountNumber });

            modelBuilder.Entity<CustomerAccount>()
                .HasOne(ca => ca.Customer)
                .WithMany(c => c.CustomerAccounts)
                .HasForeignKey(ca => ca.CustomerId);

            modelBuilder.Entity<CustomerAccount>()
                .HasOne(ca => ca.Account)
                .WithMany(a => a.CustomerAccounts)
                .HasForeignKey(ca => ca.AccountNumber);

            modelBuilder.Entity<Transaction>().HasKey(t => t.Id);
            modelBuilder.Entity<Transaction>().Property(t => t.Amount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountNumber)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Branch>().HasData(
                new Branch { Code = "CAI-01", Name = "Cairo Main Branch", Address = "Tahrir Square, Cairo", PhoneNumber = "0223445566" },
                new Branch { Code = "ALX-02", Name = "Alexandria Branch", Address = "Corniche St, Alexandria", PhoneNumber = "0355443322" }
            );

            modelBuilder.Entity<Manager>().HasData(
                new Manager { Id = 1, FullName = "Hassan Ibrahim", Email = "hassan.ibrahim@bank.com", PhoneNumber = "01011223344", HireDate = new DateTime(2015, 5, 1), BranchCode = "CAI-01" },
                new Manager { Id = 2, FullName = "Mona Zaki", Email = "mona.zaki@bank.com", PhoneNumber = "01233445566", HireDate = new DateTime(2018, 8, 15), BranchCode = "ALX-02" }
            );
        }
    }
}