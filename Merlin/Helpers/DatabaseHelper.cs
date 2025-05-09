using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace MerlinAdministrator
{
   public class DatabaseHelper
    {
        public string GetConnectionString()
        {
            string connectionString = Properties.Settings.Default.DatabaseConnection;
            return connectionString;
        }
    }
}
