using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Fryzjer.Models;

namespace Fryzjer.Data
{
    public class FryzjerContext : DbContext
    {
        public FryzjerContext (DbContextOptions<FryzjerContext> options)
            : base(options)
        {
        }

        public DbSet<Fryzjer.Models.Client> Client { get; set; } = default!;
    }
}
