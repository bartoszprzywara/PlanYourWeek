using Windows.ApplicationModel.Resources;

namespace PlanYourWeek.Helpers
{
    public static class LocalizedStrings
    {
        private static ResourceLoader resourceLoader;

        static LocalizedStrings()
        {
            resourceLoader = new ResourceLoader();
        }

        public static string GetString(string resourceName)
        {
            return resourceLoader.GetString(resourceName.ToString());
        }
    }
}