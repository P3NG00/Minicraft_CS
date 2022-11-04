using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MinicraftGame.Game.Inventories;
using MinicraftGame.Utils;

namespace MinicraftGame.Game.ItemType
{
    public class Item : IEquatable<Item>
    {
        public readonly int ID;
        public readonly string Name;
        public readonly DrawData DrawData;

        public Texture2D Texture => DrawData.Texture;
        public Color Color => DrawData.Color;

        public Item(string name, DrawData drawData, int id = -1)
        {
            ID = id;
            Name = name;
            DrawData = drawData;
        }

        public virtual void Use(Slot slot, Point blockPosition) {}

        public bool Equals(Item other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return Name.Equals(other.Name) && DrawData.Equals(other.DrawData);
        }

        public sealed override bool Equals(object obj) => Equals(obj as Item);

        public sealed override int GetHashCode() => Name.GetHashCode() ^ DrawData.GetHashCode();

        public static bool operator ==(Item a, Item b)
        {
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;
            if (ReferenceEquals(a, b))
                return true;
            return a.Equals(b);
        }

        public static bool operator !=(Item a, Item b) => !(a == b);
    }
}