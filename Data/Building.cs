using System.Collections.Generic;
using System.Drawing;

namespace Data
{
    public class Building
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Visitors { get; set; }

        private List<Point> collPoints;

        public Building(int id, string name, int visitors)
        {
            ID = id;
            Name = name;
            Visitors = visitors;
            collPoints = new List<Point>();
        }
        public void AddPoint(Point p)
        {
            collPoints.Add(p);
        }

        public List<Point> GetCollPoints()
        {
            return collPoints;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
