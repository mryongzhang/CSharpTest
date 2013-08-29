using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PetapocoSimpleMembershipProvider.DAL.PetaPoco;

namespace PetapocoSimpleMembershipProvider.DAL
{
    internal class DatabaseWrapper : Database
    {
        public DatabaseWrapper(string connectionStringName)
            : base(connectionStringName)
        {
        }

        public dynamic QuerySingle(string commandText, params object[] parameters)
        {
            return base.FirstOrDefault<dynamic>(commandText, parameters);
        }

        public IEnumerable<dynamic> Query(string commandText, params object[] parameters)
        {
            return base.Query<dynamic>(commandText, parameters);
        }

        public dynamic QueryValue(string commandText, params object[] parameters)
        {
            return base.ExecuteScalar<dynamic>(commandText, parameters);
        }

        public int Execute(string commandText, params object[] parameters)
        {
            return base.Execute(commandText, parameters);
        }

        public void Dispose()
        {
            this.Dispose();
        }
    }
}
