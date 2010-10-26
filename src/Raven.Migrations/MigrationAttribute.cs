using System;

namespace Raven.Migrations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MigrationAttribute : Attribute
    {
        public MigrationAttribute(long version)
        {
            if (version < 0) throw new ArgumentOutOfRangeException("version", "Migration version must be greater than zero.");
            Version = version;
        }

        public long Version { get; private set; }
    }
}
