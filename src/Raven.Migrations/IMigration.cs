using Raven.Client;

namespace Raven.Migrations
{
    public interface IMigration
    {
        void Up(IDocumentSession session);
        void Down(IDocumentSession session);
    }
}
