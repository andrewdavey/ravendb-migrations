using Raven.Client;

namespace Raven.Migrations
{
    public static class DocumentSessionExtensions
    {
        /// <summary>
        /// Stores an entity in the session with an explicit type tag, instead of the conventional one derived by Raven.
        /// </summary>
        public static void Store(this IDocumentSession session, dynamic entity, string ravenTypeTag)
        {
            var originalFindTypeTagName = session.Advanced.Conventions.FindTypeTagName;
            try
            {
                session.Advanced.Conventions.FindTypeTagName = type => ravenTypeTag;
                session.Store(entity);
            }
            finally
            {
                session.Advanced.Conventions.FindTypeTagName = originalFindTypeTagName;
            }
        }
    }
}
