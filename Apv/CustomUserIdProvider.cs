using Apv.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.SignalR;
using System.Data.Entity;
using System.Linq;

namespace Apv
{
    public class CustomUserIdProvider: IUserIdProvider
    {
        ApplicationDbContext _context = new ApplicationDbContext();
        public string GetUserId(IRequest request)
        {
            // your logic to fetch a user identifier goes here.

            // for example:
            ApplicationUser result = new ApplicationUser();
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var currentUser = manager.FindById(request.User.Identity.GetUserId());

            result = _context.Users.Include(x => x.Unit).SingleOrDefault(x => x.Id == currentUser.Id);


            var userId = result.Id;
            return userId.ToString();
        }
    }
}