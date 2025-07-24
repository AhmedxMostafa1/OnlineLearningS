using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace OnlineLearning.Models;

public partial class OnlineLearningContext : DbContext
{
    public OnlineLearningContext()
    {
    }

    public OnlineLearningContext(DbContextOptions<OnlineLearningContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Instructor> Instructors { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<Module> Modules { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)

        => optionsBuilder.UseSqlServer("Server=desktop-uv4h5kf\\SQLEXPRESS;Database=OnlineLearning;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("PK__Admins__4A3006F745BDA96A");

            entity.HasIndex(e => e.AdminEmail, "UQ__Admins__6066AA652B5EF69B").IsUnique();

            entity.Property(e => e.AdminId).HasColumnName("Admin_Id");
            entity.Property(e => e.AdminEmail)
                .HasMaxLength(100)
                .HasColumnName("Admin_Email");
            entity.Property(e => e.AdminFullName)
                .HasMaxLength(100)
                .HasColumnName("Admin_FullName");
            entity.Property(e => e.AdminPassword).HasColumnName("Admin_Password");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategId).HasName("PK__Categori__4FEA3E93ACEB723F");

            entity.HasIndex(e => e.CategName, "UQ__Categori__FF9D5C09E7161731").IsUnique();

            entity.Property(e => e.CategId).HasColumnName("Categ_Id");
            entity.Property(e => e.CategName)
                .HasMaxLength(100)
                .HasColumnName("Categ_Name");
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.CertiId).HasName("PK__Certific__0C82C6EC5E964109");

            entity.Property(e => e.CertiId).HasColumnName("Certi_Id");
            entity.Property(e => e.CertiIssuedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Certi_IssuedDate");

            entity.HasOne(d => d.Course).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Certifica__Cours__787EE5A0");

            entity.HasOne(d => d.Student).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Certifica__Stude__778AC167");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__37E005DB42A30675");

            entity.Property(e => e.CourseId).HasColumnName("Course_Id");
            entity.Property(e => e.CourseDescription).HasColumnName("Course_Description");
            entity.Property(e => e.CourseTitle)
                .HasMaxLength(200)
                .HasColumnName("Course_Title");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsPremium).HasDefaultValue(false);

            entity.HasOne(d => d.Category).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Courses__Categor__5629CD9C");

            entity.HasOne(d => d.Instructor).WithMany(p => p.Courses)
                .HasForeignKey(d => d.InstructorId)
                .HasConstraintName("FK__Courses__Instruc__5535A963");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrId).HasName("PK__Enrollme__F09645E010463FAD");

            entity.Property(e => e.EnrId).HasColumnName("Enr_Id");
            entity.Property(e => e.CompletionStatus).HasDefaultValue(false);
            entity.Property(e => e.EnrollDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Course).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Enrollmen__Cours__6E01572D");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Enrollmen__Stude__6D0D32F4");
        });

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(e => e.InstId).HasName("PK__Instruct__4ECD04F1B6E3F6CA");

            entity.HasIndex(e => e.InstEmail, "UQ__Instruct__2CFB52C5DBCC83CE").IsUnique();

            entity.Property(e => e.InstId).HasColumnName("Inst_Id");
            entity.Property(e => e.InstEmail)
                .HasMaxLength(100)
                .HasColumnName("Inst_Email");
            entity.Property(e => e.InstFullName)
                .HasMaxLength(100)
                .HasColumnName("Inst_FullName");
            entity.Property(e => e.InstPassword).HasColumnName("Inst_Password");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.LessonId).HasName("PK__Lessons__EC18F7F0ADC41903");

            entity.Property(e => e.LessonId).HasColumnName("Lesson_Id");
            entity.Property(e => e.LessonContentUrl).HasColumnName("Lesson_ContentUrl");
            entity.Property(e => e.LessonTitle)
                .HasMaxLength(200)
                .HasColumnName("Lesson_Title");
            entity.Property(e => e.Type).HasMaxLength(10);

            entity.HasOne(d => d.Module).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("FK__Lessons__ModuleI__66603565");
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasKey(e => e.ModuleId).HasName("PK__Modules__1DE4E0C8579E0E9F");

            entity.Property(e => e.ModuleId).HasColumnName("Module_Id");
            entity.Property(e => e.ModuleTitle)
                .HasMaxLength(200)
                .HasColumnName("Module_Title");

            entity.HasOne(d => d.Course).WithMany(p => p.Modules)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Modules__CourseI__628FA481");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PayId).HasName("PK__Payments__6F1375657CCDE604");

            entity.Property(e => e.PayId).HasColumnName("Pay_Id");
            entity.Property(e => e.PayAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("Pay_Amount");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Course).WithMany(p => p.Payments)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Payments__Course__73BA3083");

            entity.HasOne(d => d.Student).WithMany(p => p.Payments)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Payments__Studen__72C60C4A");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.QuizId).HasName("PK__Quizzes__10974DAA54FCB1AE");

            entity.Property(e => e.QuizId).HasColumnName("Quiz_Id");
            entity.Property(e => e.CorrectOption)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.OptionA).HasMaxLength(200);
            entity.Property(e => e.OptionB).HasMaxLength(200);
            entity.Property(e => e.OptionC).HasMaxLength(200);
            entity.Property(e => e.OptionD).HasMaxLength(200);

            entity.HasOne(d => d.Module).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("FK__Quizzes__ModuleI__06CD04F7");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StuId).HasName("PK__Students__DD8D49E172A93E88");

            entity.HasIndex(e => e.StuEmail, "UQ__Students__4DB502FB611F9922").IsUnique();

            entity.Property(e => e.StuId).HasColumnName("Stu_Id");
            entity.Property(e => e.StuEmail)
                .HasMaxLength(100)
                .HasColumnName("Stu_Email");
            entity.Property(e => e.StuFullName)
                .HasMaxLength(100)
                .HasColumnName("Stu_FullName");
            entity.Property(e => e.StuPassword).HasColumnName("Stu_Password");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
