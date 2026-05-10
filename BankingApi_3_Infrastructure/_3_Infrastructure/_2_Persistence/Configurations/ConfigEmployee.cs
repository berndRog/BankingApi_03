using BankingApi._2_Core.BuildingBlocks._3_Domain.ValueObjects;
using BankingApi._2_Core.Employees._3_Domain.Entities;
using BankingApi._3_Infrastructure._2_Persistence.Database.Converter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// falls Email/Address/Phone hier liegen

namespace BankingApi._3_Infrastructure._2_Persistence.Configurations;

public sealed class ConfigEmployee : IEntityTypeConfiguration<Employee> {

   public void Configure(EntityTypeBuilder<Employee> builder) {

      // tablename
      builder.ToTable("Employees");
      
      // Primary Key
      builder.HasKey(x => x.Id);
      builder.Property(x => x.Id)
         .ValueGeneratedNever()
         .HasColumnName("Id").HasColumnOrder(0);
      
      // Profile data
      builder.Property(x => x.Firstname)
         .HasMaxLength(80)
         .HasColumnName("Firstname").HasColumnOrder(1)
         .IsRequired();
      
      builder.Property(x => x.Lastname)
         .HasMaxLength(80)
         .HasColumnName("Lastname").HasColumnOrder(2)
         .IsRequired();

      // Phone
      builder.Property(e => e.PhoneVo)
         .HasConversion(vo => vo.Value, s => PhoneVo.FromPersisted(s))
         .HasMaxLength(64)
         .HasColumnName("Phone").HasColumnOrder(3)
         .IsRequired();

      builder.Property(e => e.EmailVo)
         .HasConversion(vo => vo.Value, s => EmailVo.FromPersisted(s))
         .HasMaxLength(254)
         .HasColumnName("Email").HasColumnOrder(4)
         .IsRequired();

      // Scalar properties (Employee-specific)
      builder.Property(x => x.PersonnelNumber)
         .HasMaxLength(32)
         .HasColumnName("PersonnelNumber").HasColumnOrder(5) 
         .IsRequired();
      builder.HasIndex(x => x.PersonnelNumber).IsUnique();

      // AdminRights enum -> int (SQLite friendly)
      builder.Property(x => x.AdminRights)
         .HasConversion<int>()
         .HasColumnName("AdminRights").HasColumnOrder(6) 
         .IsRequired();
      
      // EmployeeStatus enum -> int (SQLite friendly)
      builder.Property(x => x.Status)
         .HasConversion<int>()
         .HasColumnName("Status").HasColumnOrder(7) 
         .IsRequired();
      
      // IsAdmin is computed => not persisted
      builder.Ignore(x => x.IsActive);
      
      builder.Property(x => x.Subject)
         .HasMaxLength(200)
         .HasColumnName("Subject").HasColumnOrder(8)
         .IsRequired();

      builder.Property(x => x.CreatedAt)
         .HasColumnName("CreateAt").HasColumnOrder(9)
         .IsRequired();
      
      builder.Property(o => o.UpdatedAt)
         .HasColumnName("UpdateAt").HasColumnOrder(10)
         .IsRequired();
      
      builder.HasIndex(c => c.PhoneVo);
      builder.HasIndex(c => c.EmailVo).IsUnique();
      builder.HasIndex(x => x.Subject).IsUnique();
      
   }
}