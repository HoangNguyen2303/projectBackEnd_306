using EduTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduTrack.Application.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<Student> Students { get; }
        DbSet<Teacher> Teachers { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
