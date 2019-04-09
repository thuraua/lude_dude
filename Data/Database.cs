using System;
using System.Collections.Generic;
using System.Drawing;
using Oracle.ManagedDataAccess.Client;

namespace Data
{
    public class Database
    {
        SortedList<int, Building> collBuildings = new SortedList<int, Building>();
        SortedList<int, Visitor> collVisitors = new SortedList<int, Visitor>();
        static Database db = null;
        static OracleConnection conn = null;
        readonly string ip = "212.152.179.117"; //"212.152.179.117" "192.168.128.152"

        private Database()
        {
            conn = new OracleConnection(@"user id=d4b26;password=d4b;data source=" +
                                                     "(description=(address=(protocol=tcp)" +
                                                     "(host=" + ip + ")(port=1521))(connect_data=" +
                                                     "(service_name=ora11g)))");
            conn.Open();
        }

        public static Database GetInstance()
        {
            if (db == null)
                db = new Database();

            return db;
        }

        private void ReadBuildingsFromDB()
        {
            string sqlCmd = "SELECT v.building_id as bId, v.name as bName, v.visitors as visitors, t.X as xCoordinate, t.Y as yCoordinate, t.id as cId FROM village v, TABLE(SDO_UTIL.GETVERTICES(v.building)) t";
            OracleCommand cmd = new OracleCommand(sqlCmd, conn);
            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    if (!collBuildings.ContainsKey(Convert.ToInt32(reader["bId"].ToString())))
                    {
                        Building building = new Building(Convert.ToInt32(reader["bId"].ToString()), reader["bName"].ToString(), Convert.ToInt32(reader["visitors"].ToString()));
                        collBuildings.Add(building.ID, building);
                    }
                    collBuildings[Convert.ToInt32(reader["bId"].ToString())].AddPoint(new Point(Convert.ToInt32(reader["xCoordinate"].ToString()), Convert.ToInt32(reader["yCoordinate"].ToString())));
                }
            }
        }

        private void ReadVisitorsFromDB()
        {
            string sqlCmd = "select v.v_id id, v.v_name name, t.X x, t.Y y from visitors v, TABLE(SDO_UTIL.GETVERTICES(v.POSITION)) t";
            OracleCommand cmd = new OracleCommand(sqlCmd, conn);
            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    collVisitors.Add(Convert.ToInt32(reader["id"]), new Visitor(Convert.ToInt32(reader["id"]), reader["name"].ToString(), new Point(Convert.ToInt32(reader["x"]), Convert.ToInt32(reader["y"]))));                    
                }
            }
        }

        public IList<Visitor> GetVisitors()
        {
            ReadVisitorsFromDB();
            return collVisitors.Values;
        }

        public IList<Building> GetBuildings()
        {
            ReadBuildingsFromDB();
            return collBuildings.Values;
        }
    }
}
