namespace Sibedge.Plv8Server
{
    /// <summary> Access levels for authorization </summary>
    public static class AccessLevels
    {
        /// <summary> Anonymous can read </summary>
        public const int AnonymousRead = 0b1;

        /// <summary> Identified user can read </summary>
        public const int UserRead = 0b10;

        /// <summary> Anyone can read </summary>
        public const int AnyRead = AnonymousRead | UserRead;

        /// <summary> Identified user can read only own records </summary>
        public const int UserReadOwn = 0b100;

        /// <summary> Direct access to table is denied, but allowed to read by relations </summary>
        public const int RelatedRead = 0b1000;

        /// <summary> Identified user can change data </summary>
        public const int UserWrite = 0b10000;

        /// <summary> Identified user can read and change data </summary>
        public const int UserAll = UserRead | UserWrite;

        /// <summary> Identified user can change only own records </summary>
        public const int UserWriteOwn = 0b100000;

        /// <summary> Identified user can read and change only own records </summary>
        public const int UserAllOwn = UserReadOwn | UserWriteOwn;

        /// <summary> This is a table of user accounts </summary>
        public const int UserTable = 0b1000000;

        /// <summary> A key for setting default access level </summary>
        public const string DefaultKey = "$default";
    }
}
