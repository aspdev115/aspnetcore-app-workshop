using FrontEnd.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrontEnd.Services
{
  public class AdminService : IAdminService
  {

    private readonly IServiceProvider _ServiceProvider;
    private bool _adminExists;

    public AdminService(IServiceProvider serviceProvider)
    {
      _ServiceProvider = serviceProvider;
    }

    public async Task<bool> AllowAdminUserCreationAsync()
    {
      if (_adminExists)
      {
        return false;
      }
      else
      {
        using (var scope = _ServiceProvider.CreateScope())
        {
          var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

          if (await dbContext.Users.AnyAsync(user => user.IsAdmin))
          {
            // There are already admin users so disable admin creation
            _adminExists = true;
            return false;
          }

          // There are no admin users so enable admin creation
          return true;
        }
      }
    }

  }
}
