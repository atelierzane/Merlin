using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerlinPointOfSale
{
    public class DatabaseHelper
    {
        public string GetConnectionString()
        {
            string connectionString = @"Server=.\SQLEXPRESS;Database=MerlinDatabase_Base;Trusted_Connection=True;";
            return connectionString;
        }




    }
}
