using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Fryzjer.Data;
using Fryzjer.Models;

namespace Fryzjer.Pages.Clients
{
    public class IndexModel : PageModel
    {
        private readonly Fryzjer.Data.FryzjerContext _context;

        public IndexModel(Fryzjer.Data.FryzjerContext context)
        {
            _context = context;
        }

        public IList<Client> Client { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Client = await _context.Client.ToListAsync();
        }
    }
}
