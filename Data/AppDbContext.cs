using Microsoft.EntityFrameworkCore;
using OOP_Semester.Models;

namespace OOP_Semester.Data
{
    public class AppDbContext : DbContext
    {
        // --- Danh sách các bảng (Đã cập nhật đầy đủ) ---
        public DbSet<User> Users { get; set; }
        public DbSet<UserFood> UserFoods { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<HabitCategory> HabitCategories { get; set; }
        public DbSet<Habit> Habits { get; set; }
        public DbSet<HabitLog> HabitLogs { get; set; }
        public DbSet<HabitMessenger> HabitMessengers { get; set; }
        public DbSet<Messenger> Messengers { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<PetType> PetTypes { get; set; }
        public DbSet<GoldTransaction> GoldTransactions { get; set; }

        public DbSet<HabitReminder> HabitReminders { get; set; }
        public DbSet<RepeatDay> RepeatDays { get; set; }
        public DbSet<ChallengeTask> ChallengeTasks { get; set; }
        public DbSet<UserChallenge> UserChallenges { get; set; }
        public DbSet<Notify> Notify { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = "server=localhost;database=habit_tracker_db;user=root;password=@Vanmontri123";
                optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình khóa phức hợp (UserFood, HabitMessenger, PetMessenger)
            modelBuilder.Entity<UserFood>().HasKey(uf => new { uf.UserID, uf.FoodID });
            modelBuilder.Entity<HabitMessenger>().HasKey(hm => new { hm.HabitID, hm.MesID });

            // ==> THÊM MỚI: Cấu hình khóa phức hợp cho UserChallenge
            modelBuilder.Entity<UserChallenge>()
                .HasKey(uc => new { uc.UserID, uc.ChallengesID });
        }
    }
}