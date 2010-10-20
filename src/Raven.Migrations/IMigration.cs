using Raven.Client;

namespace Raven.Migrations
{
    public interface IMigration
    {
        void Up(IDocumentStore store);
        void Down(IDocumentStore store);
    }
}
