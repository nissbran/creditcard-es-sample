namespace Bank.Cards.Processes.InvoiceProcess.DbContext
{
    using Domain;
    using Microsoft.EntityFrameworkCore;

    public class StateDbContext : DbContext
    {
        public DbSet<MonthlyInvoiceState> MonthlyInvoiceStates { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=localhost\SQLEXPRESS;;Database=TestState;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MonthlyInvoiceState>()
                .ForSqlServerIsMemoryOptimized()
                .HasKey(c => c.AccountId);

            modelBuilder.Entity<MonthlyInvoiceState>()
                .Property(p => p.AccountId)
                .ValueGeneratedNever();
        }
    }
}
