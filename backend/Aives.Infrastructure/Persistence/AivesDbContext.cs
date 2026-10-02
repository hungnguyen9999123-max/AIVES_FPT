using Microsoft.EntityFrameworkCore;
using Aives.Domain.Entities;

namespace Aives.Infrastructure.Persistence;

public class AivesDbContext : DbContext
{
    public AivesDbContext(DbContextOptions<AivesDbContext> options) : base(options)
    {
    }
}
