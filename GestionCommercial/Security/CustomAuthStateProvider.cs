using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using GestionCommercial.Models;


namespace GestionCommercial.Security
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        private Utilisateur? _currentUser;

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_currentUser == null)
                return Task.FromResult(new AuthenticationState(_anonymous));

            var identity = CreateIdentityFromUser(_currentUser);
            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }

        private ClaimsIdentity CreateIdentityFromUser(Utilisateur u)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, u.IdUtilisateur.ToString()),
                new Claim(ClaimTypes.Name, u.NomUtilisateur),
                new Claim(ClaimTypes.Role, u.Role)
            };

            return new ClaimsIdentity(claims, "CustomAuth");
        }

        public void MarkUserAsAuthenticated(Utilisateur user)
        {
            _currentUser = user;
            var identity = CreateIdentityFromUser(user);
            var principal = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
        }

        public void MarkUserAsLoggedOut()
        {
            _currentUser = null;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }
    }
}
