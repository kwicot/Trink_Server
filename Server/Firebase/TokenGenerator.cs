using System;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;

namespace WindowsFormsApp1.Database
{
    public class TokenGenerator
    {
        private readonly GoogleCredential _credential;

        public TokenGenerator(string serviceAccountJsonPath)
        {
            _credential = GoogleCredential.FromFile(serviceAccountJsonPath)
                .CreateScoped( 
                    "https://www.googleapis.com/auth/userinfo.email",
                    "https://www.googleapis.com/auth/firebase.database");
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var token = await _credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            return token;
        }
    }
}