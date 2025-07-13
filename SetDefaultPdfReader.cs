using Microsoft.Win32;

namespace PDF_EVERYTHING
{
    public static class SetDefaultPdfReader
    {
        public static void SetDefault()
        {
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string appName = "PDF_EVERYTHING";

            RegistryKey key = Registry.ClassesRoot.CreateSubKey(".pdf");
            key.SetValue("", appName);

            key = Registry.ClassesRoot.CreateSubKey(appName);
            key.SetValue("", "PDF Document");

            key = Registry.ClassesRoot.CreateSubKey(appName + @"\shell\open\command");
            key.SetValue("", "\"" + appPath + "\" \"%1\"");
        }
    }
}
