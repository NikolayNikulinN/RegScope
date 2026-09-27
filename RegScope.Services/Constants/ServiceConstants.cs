using Microsoft.Win32;
using RegScope.Presentation.Constants;

namespace RegScope.Services.Constants
{
    public static class ServiceConstants
    {
        public const string DefaultValueName = "(Default)";
        public const int ValueDataDisplayLimit = 500;
        public const int SearchBatchSize = 75;

        public static readonly RegistryHiveOptionData[] HiveOptions =
        [
           new("HKEY_CLASSES_ROOT", RegistryHive.ClassesRoot, false),
            new("HKEY_CURRENT_USER", RegistryHive.CurrentUser, false),
            new("HKEY_LOCAL_MACHINE", RegistryHive.LocalMachine, false),
            new("HKEY_USERS", RegistryHive.Users, false),
            new("HKEY_CURRENT_CONFIG", RegistryHive.CurrentConfig, false),
            new("All Registry", null, true)
       ];
    }
}
