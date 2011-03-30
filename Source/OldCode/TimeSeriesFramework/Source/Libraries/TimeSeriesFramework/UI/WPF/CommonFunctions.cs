using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeSeriesFramework.UI.DataModel;
using System.Collections.ObjectModel;
using System.Data;
using TimeSeriesFramework.Data;

namespace TimeSeriesFramework.UI
{
    public static class CommonFunctions
    {
        #region [ Manage Companies Code ]
        
        public static ObservableCollection<Company> GetCompanyList(DataConnection connection)
        {   
            bool createdConnection = false;
            try
            {
                if (connection == null)
                {
                    connection = new DataConnection();
                    createdConnection = true;
                }

                List<Company> companyList = new List<Company>();
                IDbCommand command = connection.Connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT ID, Acronym, MapAcronym, Name, URL, LoadOrder FROM Company ORDER BY LoadOrder";

                DataTable resultTable = new DataTable();
                resultTable.Load(command.ExecuteReader());

                companyList = (from item in resultTable.AsEnumerable()
                               select new Company()
                               {
                                   ID = item.Field<int>("ID"),
                                   Acronym = item.Field<string>("Acronym"),
                                   MapAcronym = item.Field<string>("MapAcronym"),
                                   Name = item.Field<string>("Name"),
                                   URL = item.Field<string>("URL"),
                                   LoadOrder = item.Field<int>("LoadOrder")
                               }).ToList();

                return new ObservableCollection<Company>(companyList);
            }
            finally
            {
                if (createdConnection && connection != null)
                    connection.Dispose();
            }
        }

        #endregion
    }
}
