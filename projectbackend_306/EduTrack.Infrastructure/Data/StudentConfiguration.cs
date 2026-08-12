using EduTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduTrack.Infrastructure.Data
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.Property(x => x.StudentCode).HasMaxLength(20).IsRequired();
            builder.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            builder.Property(x => x.Cohort).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Email).HasMaxLength(150).IsRequired();
            builder.Property(x => x.Phone).HasMaxLength(20);
        }
    }
}