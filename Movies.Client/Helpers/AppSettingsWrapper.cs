namespace Movies.Client.Helpers
{
    public static class AppSettingsWrapper
    {
        public static string BaseURL => System.Configuration.ConfigurationManager.AppSettings["BaseURL"] ?? "http://localhost:5001";
    }
}
