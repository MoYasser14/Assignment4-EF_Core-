using Assignment4_EF_Core_.Models;
using Microsoft.EntityFrameworkCore;

namespace Assignment4_EF_Core_.Data
{
    internal class Program
    {
        static void Main()
        {
            using var context = new BankDbContext();
            context.Database.EnsureCreated();

            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("National Bank Management");
                Console.WriteLine("======================================");
                Console.WriteLine("1) Add a new Customer");
                Console.WriteLine("2) Open a new Account for a Customer");
                Console.WriteLine("3) Update Account Status (Active / Closed)");
                Console.WriteLine("4) Remove an Account from a Customer");
                Console.WriteLine("5) List all Customers (with accounts)");
                Console.WriteLine("0) Exit");
                Console.Write("\nEnter choice: ");

                string? choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        AddNewCustomer(context);
                        break;
                    case "2":
                        OpenNewAccount(context);
                        break;
                    case "3":
                        UpdateAccountStatus(context);
                        break;
                    case "4":
                        RemoveAccountFromCustomer(context);
                        break;
                    case "5":
                        ListAllCustomers(context);
                        break;
                    case "0":
                        exit = true;
                        Console.WriteLine("Exiting application. Goodbye!");
                        break;
                    default:
                        Console.WriteLine("Invalid choice! Please enter a number between 0 and 5.");
                        break;
                }

                if (!exit)
                {
                    Console.WriteLine("\nPress any key to return to the menu...");
                    Console.ReadKey();
                }
            }
        }

        private static void AddNewCustomer(BankDbContext context)
        {
            Console.WriteLine("Add New Customer");
            Console.Write("Full Name       : ");
            string name = Console.ReadLine() ?? string.Empty;

            Console.Write("National ID     : ");
            string nationalId = Console.ReadLine() ?? string.Empty;

            DateTime dob = GetValidDate("Date of Birth (yyyy-MM-dd): ");

            Console.Write("Email: ");
            string email = Console.ReadLine() ?? string.Empty;

            Console.Write("Phone: ");
            string phone = Console.ReadLine() ?? string.Empty;

            Console.Write("Address: ");
            string address = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("Customer Type:");
            Console.WriteLine("1) Individual");
            Console.WriteLine("2) Business");
            int typeChoice = GetValidInt("Choice: ");

            CustomerType cType = (typeChoice == 2) ? CustomerType.Business : CustomerType.Individual;

            var customer = new Customer
            {
                FullName = name,
                NationalId = nationalId,
                DateOfBirth = dob,
                Email = email,
                PhoneNumber = phone,
                Address = address,
                CustomerType = cType
            };

            context.Customers.Add(customer);
            context.SaveChanges();

            Console.WriteLine($"\nCustomer created successfully. CustomerId = {customer.Id}");
        }

        private static void OpenNewAccount(BankDbContext context)
        {
            Console.WriteLine("Open New Account");
            Console.Write("Account Number: ");
            string accNumber = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("Account Type:");
            Console.WriteLine("1) Savings");
            Console.WriteLine("2) Current");
            Console.WriteLine("3) Business");
            int typeChoice = GetValidInt("Choice: ");
            AccountType accType = typeChoice switch
            {
                2 => AccountType.Current,
                3 => AccountType.Business,
                _ => AccountType.Savings
            };

            Console.Write("Branch Code : ");
            string branchCode = Console.ReadLine() ?? string.Empty;

            int customerId = GetValidInt("Customer Id : ");

            Console.WriteLine("Ownership Role:");
            Console.WriteLine("1) Primary");
            Console.WriteLine("2) CoHolder");
            int roleChoice = GetValidInt("Choice: ");
            OwnershipRole role = (roleChoice == 2) ? OwnershipRole.CoHolder : OwnershipRole.Primary;

            Console.WriteLine($"Validating branch '{branchCode}' and customer #{customerId}...");

            var branch = context.Branches.Find(branchCode);
            var customer = context.Customers.Find(customerId);

            if (branch == null || customer == null)
            {
                Console.WriteLine("Error: Branch or Customer does not exist in the database!");
                return;
            }

            var account = context.Accounts.Find(accNumber);
            if (account == null)
            {
                account = new Account
                {
                    AccountNumber = accNumber,
                    AccountType = accType,
                    BranchCode = branchCode,
                    CurrentBalance = 0.00m,
                    OpeningDate = DateTime.Now,
                    AccountStatus = AccountStatus.Active
                };
                context.Accounts.Add(account);
            }

            var customerAccount = new CustomerAccount
            {
                CustomerId = customerId,
                AccountNumber = accNumber,
                OwnershipRole = role,
                OwnershipStartDate = DateTime.Now
            };

            context.CustomerAccounts.Add(customerAccount);
            context.SaveChanges();

            Console.WriteLine($"Account '{accNumber}' created and linked to customer {customerId} as {role} owner.");
        }

        private static void UpdateAccountStatus(BankDbContext context)
        {
            Console.WriteLine("Update Account Status");
            Console.Write("Account Number: ");
            string accNumber = Console.ReadLine() ?? string.Empty;

            int customerId = GetValidInt("Customer Id   : ");

            Console.WriteLine("New Status:");
            Console.WriteLine("1) Active");
            Console.WriteLine("2) Closed");
            int statusChoice = GetValidInt("Choice: ");
            AccountStatus newStatus = (statusChoice == 2) ? AccountStatus.Closed : AccountStatus.Active;

            var customerAccount = context.CustomerAccounts
                .Include(ca => ca.Account)
                .FirstOrDefault(ca => ca.AccountNumber == accNumber && ca.CustomerId == customerId);

            if (customerAccount == null)
            {
                Console.WriteLine("Error: Account link with this customer was not found!");
                return;
            }

            customerAccount.Account.AccountStatus = newStatus;
            context.SaveChanges();

            Console.WriteLine($"Status updated to {newStatus}.");
        }

        private static void RemoveAccountFromCustomer(BankDbContext context)
        {
            Console.WriteLine("Remove Account From Customer");
            Console.Write("Account Number: ");
            string accNumber = Console.ReadLine() ?? string.Empty;

            int customerId = GetValidInt("Customer Id   : ");

            var customerAccount = context.CustomerAccounts
                .Include(ca => ca.Account)
                .ThenInclude(a => a.CustomerAccounts)
                .FirstOrDefault(ca => ca.AccountNumber == accNumber && ca.CustomerId == customerId);

            if (customerAccount == null)
            {
                Console.WriteLine("Error: Account link not found.");
                return;
            }

            var account = customerAccount.Account;
            context.CustomerAccounts.Remove(customerAccount);
            context.SaveChanges();

            Console.WriteLine("Ownership link deleted.");

            if (!account.CustomerAccounts.Any())
            {
                context.Accounts.Remove(account);
                context.SaveChanges();
                Console.WriteLine($"That was the last owner, account '{accNumber}' was also removed.");
            }
        }

        private static void ListAllCustomers(BankDbContext context)
        {
            Console.WriteLine("All Customers");
            var customers = context.Customers
                .Include(c => c.CustomerAccounts)
                .ThenInclude(ca => ca.Account)
                .ThenInclude(a => a.Branch)
                .ToList();

            if (!customers.Any())
            {
                Console.WriteLine("No customers found.");
                return;
            }

            foreach (var c in customers)
            {
                Console.WriteLine($"#{c.Id} {c.FullName} ({c.CustomerType})");

                if (!c.CustomerAccounts.Any())
                {
                    Console.WriteLine("(no accounts)");
                }
                else
                {
                    foreach (var ca in c.CustomerAccounts)
                    {
                        var acc = ca.Account;
                        string year = acc.OpeningDate.Year.ToString();
                        string typeShort = acc.AccountType.ToString().ToUpper();
                        string branchName = acc.Branch?.Name ?? "Main Branch";

                        Console.WriteLine($"  {year}-{typeShort} Current Balance: {acc.CurrentBalance:N2} {ca.OwnershipRole} {acc.AccountStatus} @{branchName}");
                    }
                }
            }
        }

        #region Helper Validation Methods
        private static int GetValidInt(string prompt)
        {
            int result;
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out result))
                {
                    return result;
                }
                Console.WriteLine("Invalid input! Please enter a valid number.");
            }
        }

        private static DateTime GetValidDate(string prompt)
        {
            DateTime result;
            while (true)
            {
                Console.Write(prompt);
                if (DateTime.TryParseExact(Console.ReadLine(), "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out result))
                {
                    return result;
                }
                Console.WriteLine("Invalid date format! Please use yyyy-MM-dd.");
            }
        }
        #endregion
    }
}