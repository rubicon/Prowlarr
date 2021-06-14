using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(8)]
    public class alternate_links : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // Switch Config Contract for the indexers that changed
            // Set BaseUrl in settings for existing indexers
        }
    }
}
