using EduTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduTrack.Infrastructure.Data
{
    public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
    {
        public void Configure(EntityTypeBuilder<Teacher> builder)
        {
            builder.Property(x => x.TeacherCode).HasMaxLength(20).IsRequired();
            builder.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            builder.Property(x => x.Specialization).HasMaxLength(120);
            builder.Property(x => x.Email).HasMaxLength(150).IsRequired();
        }
    }
}