using System;

namespace Raven.Migrations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MigrationAttribute : Attribute
    {
        public MigrationAttribute(long version)
        {
            Version = version;
        }

        public long Version { get; private set; }
    }
}
