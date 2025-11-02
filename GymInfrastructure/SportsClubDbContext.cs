using System;
using System.Collections.Generic;
using GymDomain.Model;
using Microsoft.EntityFrameworkCore;


namespace GymInfrastructure {

    public partial class SportsClubDbContext : DbContext
    {
        public SportsClubDbContext()
        {
        }

        public SportsClubDbContext(DbContextOptions<SportsClubDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Client> Clients { get; set; }

        public virtual DbSet<Trainer> Trainers { get; set; }

        public virtual DbSet<Training> Trainings { get; set; }

        public virtual DbSet<TrainingRegistration> TrainingRegistrations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
            => optionsBuilder.UseSqlServer("Server=DESKTOP-2CLBBA4\\SQLEXPRESS; Database=SportsClubDB; Trusted_Connection=True; TrustServerCertificate=True; ");



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Clients__3213E83F8E8E2AA7");

                entity.HasIndex(e => e.Email, "UQ__Clients__AB6E61646578AB68").IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .HasColumnName("email");
                entity.Property(e => e.FirstName)
                    .HasMaxLength(50)
                    .HasColumnName("first_name");
                entity.Property(e => e.LastName)
                    .HasMaxLength(50)
                    .HasColumnName("last_name");
                entity.Property(e => e.PasswordHash)
                    .HasMaxLength(255)
                    .HasColumnName("password_hash");
                entity.Property(e => e.Phone)
                    .HasMaxLength(20)
                    .HasColumnName("phone");
                entity.Property(e => e.PhotoUrl)
                    .HasMaxLength(255)
                    .HasColumnName("photo_url");
                entity.Property(e => e.IdentityUserId)
    .HasMaxLength(450)
    .HasColumnName("identity_user_id");

            });

            modelBuilder.Entity<Trainer>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Trainers__3213E83F5FC22A86");

                entity.HasIndex(e => e.Email, "UQ__Trainers__AB6E6164E3AA3C7E").IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .HasColumnName("email");
                entity.Property(e => e.FirstName)
                    .HasMaxLength(50)
                    .HasColumnName("first_name");
                entity.Property(e => e.LastName)
                    .HasMaxLength(50)
                    .HasColumnName("last_name");
                entity.Property(e => e.PasswordHash)
                    .HasMaxLength(255)
                    .HasColumnName("password_hash");
                entity.Property(e => e.Phone)
                    .HasMaxLength(20)
                    .HasColumnName("phone");
                entity.Property(e => e.PhotoUrl)
                    .HasMaxLength(255)
                    .HasColumnName("photo_url");
                entity.Property(e => e.Specialization)
                    .HasMaxLength(100)
                    .HasColumnName("specialization");
                
                    entity.Property(e => e.IdentityUserId)
                          .HasMaxLength(450)
                          .HasColumnName("identity_user_id");
               

            });

            modelBuilder.Entity<Training>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Training__3213E83F7BDEA331");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Date).HasColumnName("date");
                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .HasColumnName("description");
                entity.Property(e => e.DurationMinutes)
                    .HasDefaultValue(60)
                    .HasColumnName("duration_minutes");
                entity.Property(e => e.IsCanceled)
                    .HasDefaultValue(false)
                    .HasColumnName("is_canceled");
                entity.Property(e => e.MaxClients)
                    .HasDefaultValue(20)
                    .HasColumnName("max_clients");
                entity.Property(e => e.PhotoUrl)
                    .HasMaxLength(255)
                    .HasColumnName("photo_url");
                entity.Property(e => e.Price)
                    .HasDefaultValue(0m)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("price");
                entity.Property(e => e.StartTime).HasColumnName("start_time");
                entity.Property(e => e.Title)
                    .HasMaxLength(100)
                    .HasColumnName("title");
                entity.Property(e => e.TrainerId).HasColumnName("trainer_id");

                entity.HasOne(d => d.Trainer).WithMany(p => p.Training)
                    .HasForeignKey(d => d.TrainerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Trainings__train__5FB337D6");
            });

            modelBuilder.Entity<TrainingRegistration>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Training__3213E83FF2C4E1D6");

                entity.HasIndex(e => new { e.ClientId, e.TrainingId }, "UQ_ClientTraining").IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ClientId).HasColumnName("client_id");
                entity.Property(e => e.RegistrationDate)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime")
                    .HasColumnName("registration_date");
                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue("заплановане")
                    .HasColumnName("status");
                entity.Property(e => e.TrainingId).HasColumnName("training_id");

                entity.HasOne(d => d.Client).WithMany(p => p.TrainingRegistrations)
                    .HasForeignKey(d => d.ClientId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__TrainingR__clien__656C112C");

                entity.HasOne(d => d.Training).WithMany(p => p.TrainingRegistrations)
                    .HasForeignKey(d => d.TrainingId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__TrainingR__train__66603565");
            });

            // ✅ Конвертери DateOnly / TimeOnly для EF Core + SQL Server
            modelBuilder
                .Entity<Training>()
                .Property(t => t.Date)
                .HasConversion<DateOnlyConverter, DateOnlyComparer>();

            modelBuilder
                .Entity<Training>()
                .Property(t => t.StartTime)
                .HasConversion<TimeOnlyConverter, TimeOnlyComparer>();


            OnModelCreatingPartial(modelBuilder);


        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}