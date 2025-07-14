using Microsoft.AspNetCore.Identity;

namespace Lanches.Services
{
    public class SeedUserRoleInitial : ISeedUserRoleInitial
    {
        private readonly UserManager<IdentityUser> _userManger;
        private readonly RoleManager<IdentityRole> _roleManger;

        public SeedUserRoleInitial(UserManager<IdentityUser> userManger, RoleManager<IdentityRole> roleManger)
        {
            _userManger = userManger;
            _roleManger = roleManger;
        }

        public void SeedRoles()
        {
            if(!_roleManger.RoleExistsAsync("Member").Result)
            {
                IdentityRole role = new IdentityRole();
                role.Name = "Member";
                IdentityResult roleResult = _roleManger.CreateAsync(role).Result;
                
            }
            if (!_roleManger.RoleExistsAsync("Admin").Result)
            {
                IdentityRole role = new IdentityRole();
                role.Name = "Admin";
                IdentityResult roleResult = _roleManger.CreateAsync(role).Result;

            }


        }

        public void SeedUsers()
        {
            if(_userManger.FindByEmailAsync("usuario@localhost").Result == null)
            {
                IdentityUser user = new IdentityUser();
                user.UserName = "usuario@localhost";
                user.Email = "usuario@localhost";
                user.NormalizedUserName = "USUARIO@LOCALHOST";
                user.NormalizedEmail = "USUARIO@LOCALHOST";
                user.EmailConfirmed = true;
                user.LockoutEnabled = false;
                user.SecurityStamp = Guid.NewGuid().ToString(); 
                IdentityResult result = _userManger.CreateAsync(user,"Lds148253#").Result;

                if(result.Succeeded)
                {
                    _userManger.AddToRoleAsync(user, "Member").Wait();
                }
            }

            if (_userManger.FindByEmailAsync("admin@msn.com").Result == null)
            {
                IdentityUser user = new IdentityUser();
                user.UserName = "admin";
                user.Email = "admin@msn.com";
                user.NormalizedUserName = "ADMIN";
                user.NormalizedEmail = "ADMIN@MSN.COM";
                user.EmailConfirmed = true;
                user.LockoutEnabled = false;
                user.SecurityStamp = Guid.NewGuid().ToString();
                IdentityResult result = _userManger.CreateAsync(user, "Lds148253#").Result;

                if (result.Succeeded)
                {
                    _userManger.AddToRoleAsync(user, "Admin").Wait();
                }
            }


        }
    }
}
