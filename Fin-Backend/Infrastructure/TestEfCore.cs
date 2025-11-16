using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Test
{
    // Simple test to verify EF Core types can be resolved
    public class TestConfiguration : IEntityTypeConfiguration<string>
    {
        public void Configure(EntityTypeBuilder<string> builder)
        {
            // This is just a test - won't actually work with string type
        }
    }
}