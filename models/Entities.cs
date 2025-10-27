namespace WPF_Payment_Project.Models
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public partial class Entities : DbContext
    {
        private static Entities _context;

        public Entities()
            : base("name=Entities")
        {
        }

        public static Entities GetContext()
        {
            if (_context == null)
            {
                _context = new Entities();
            }
            return _context;
        }

        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<Payment> Payment { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>()
                .HasMany(e => e.Payment)
                .WithRequired(e => e.Users)
                .HasForeignKey(e => e.UserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Category>()
                .HasMany(e => e.Payment)
                .WithRequired(e => e.Category)
                .HasForeignKey(e => e.CategoryID)
                .WillCascadeOnDelete(false);
        }
    }
}