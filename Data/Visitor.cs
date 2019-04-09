using System.Drawing;

namespace Data
{
    public class Visitor
    {
        public int ID { get; set; }
        public Point Position { get; set; }
        public string Name { get; set; }

        public Visitor(int id, string name, Point position)
        {
            ID = id;
            Name = name;
            Position = position;
        }

        public override string ToString()
        {
            return Name + " (ID: " + ID + ")";
        }
    }
}