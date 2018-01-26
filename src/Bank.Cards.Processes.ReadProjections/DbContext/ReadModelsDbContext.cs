namespace EventSourcing.Examples.EventStore.ReadProjections.DbContext
{
    using Bank.Cards.Processes.ReadProjections.Domain;
    using Bank.Cards.Processes.ReadProjections.Domain.Account;
    using Microsoft.EntityFrameworkCore;

    public class ReadModelsDbContext : DbContext
    {
        public DbSet<AccountBalance> AccountBalances { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=localhost\SQLEXPRESS;;Database=ReadModels;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountBalance>()
                .ForSqlServerIsMemoryOptimized()
                .HasKey(c => c.AccountId);

            modelBuilder.Entity<AccountBalance>()
                .Property(p => p.AccountId)
                .ValueGeneratedNever();
        }
    }
}
