using SpacetimeDB;

namespace StdbModule.Utils;

public static class Permission
{
    public static class Level
    {
        public static int None = 0;
        public static int Read = 1;
        public static int Write = 2;
        public static int Admin = 3;
        public static int Owner = 4;
    }
}
