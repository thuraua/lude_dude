﻿using System.Drawing;

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

        public Visitor(string text, Point point)
        {
            ID = -1;
            Name = text;
            Position = point;
        }

        public override string ToString()
        {
            return Name + " (ID: " + ID + ")";
        }
    }
}