using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using FluentMigrator;
using NzbDrone.Core.Applications;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(8)]
    public class app_profiles_applications : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("AppSyncProfiles").AddColumn("ApplicationIDs").AsString().WithDefaultValue("[]");

            Execute.WithConnection(AddApplications);
        }

        private void AddApplications(IDbConnection conn, IDbTransaction tran)
        {
            var existing = conn.Query<AppId>("SELECT Id FROM Applications").ToList();

            Console.WriteLine(existing);

            var updateSql = "UPDATE AppSyncProfiles SET ApplicationIDs = @Id";
            conn.Execute(updateSql, existing, transaction: tran);
        }

        private class AppId
        {
            public int Id { get; set; }
        }
    }
}
