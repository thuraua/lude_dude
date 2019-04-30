using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Data
{
    public class Database
    {
        private SortedList<int, Building> collBuildings = new SortedList<int, Building>();
        private SortedList<int, Visitor> collVisitors = new SortedList<int, Visitor>();
        private static Database db = null;
        private static OracleConnection conn = null;
        private readonly string ip = "192.168.128.152"; //"212.152.179.117" "192.168.128.152"

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
            return db == null ? new Database() : db;
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
                while (reader.Read())
                    collVisitors.Add(Convert.ToInt32(reader["id"]), new Visitor(Convert.ToInt32(reader["id"]), reader["name"].ToString(), new Point(Convert.ToInt32(reader["x"]), Convert.ToInt32(reader["y"]))));
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

        public void addVisitor(Visitor newVisitor)
        {
            string sqlCmd = "INSERT INTO visitors VALUES(visitors_seq.nextval, :name, SDO_GEOMETRY(2001, NULL, SDO_POINT_TYPE(:x, :y, NULL), NULL, NULL))";
            OracleCommand cmd = new OracleCommand(sqlCmd, conn);
            cmd.Parameters.Add("name", newVisitor.Name);
            cmd.Parameters.Add("x", newVisitor.Position.X);
            cmd.Parameters.Add("y", newVisitor.Position.Y);
            cmd.ExecuteNonQuery();
        }

        public IList<Visitor> ReadVisitorOfBuilding(Building building)
        {
            string sqlCmd = "Select v.v_id id, v.v_name name, t.X x, t.Y y from visitors v , table (SDO_UTIL.GETVERTICES(v.position))t WHERE v.v_id IN (SELECT v2.v_id FROM visitors v2 INNER JOIN village b ON SDO_CONTAINS(b.BUILDING, v2.POSITION) = 'TRUE', TABLE(SDO_UTIL.GETVERTICES(v2.POSITION)) t where building_id = :buildingId )";
            OracleCommand cmd = new OracleCommand(sqlCmd, conn);
            cmd.Parameters.Add("buildingId", building.ID);
            OracleDataReader reader = cmd.ExecuteReader();
            List<Visitor> collVisitors = new List<Visitor>();
            if (reader.HasRows)
                while (reader.Read())
                    collVisitors.Add(new Visitor(Convert.ToInt32(reader["id"]), reader["name"].ToString(), new Point(Convert.ToInt32(reader["x"]), Convert.ToInt32(reader["y"]))));
            return collVisitors;
        }

        public string ReadBuildingWhereVisitorOccurs(Visitor selectedItem)
        {
            string rgw = "";
            string selcmd = "SELECT  b.name FROM visitors v " +
               "INNER JOIN village b ON SDO_CONTAINS(b.BUILDING, v.POSITION) = 'TRUE'," +
               "TABLE(SDO_UTIL.GETVERTICES(v.POSITION)) t where v.v_name = :name";
            OracleCommand cmd = new OracleCommand(selcmd, conn);
            cmd.Parameters.Add(new OracleParameter("name", selectedItem.Name));
            OracleDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
                while (reader.Read())
                    rgw += reader["name"].ToString();
            return rgw;
        }
    }
}