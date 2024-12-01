using System.Threading.Tasks;

namespace WindowsFormsApp1.Database
{
    public static class FirebaseService
    {
        public const string Tag = "Firebase_Service";
        public static string AccessToken { get; private set; }
        public static bool Initialized { get; private set; }
        
        static TokenGenerator _tokenGenerator;
        
        public static async Task Initialize()
        {
            _tokenGenerator = new TokenGenerator(Constants.ServiceAccountKeyPath);
            AccessToken = await _tokenGenerator.GetAccessTokenAsync();
            
            Logger.LogInfo(Tag, $"Initialized. Access token [{AccessToken}]");
            Initialized = true;
        }
        
    }
}