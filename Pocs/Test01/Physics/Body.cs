using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test01.Physics
{
    public class Body
    {
        public float Mass { get; set; }

        public Vector2 Position { get; set; }
        public Size2 Size { get; set; }

        public Vector2 Speed { get; set; }

        public Rectangle BoundingBox
        {
            get
            {                
                return new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Size.Width, this.Size.Height);
            }
        }

        public RectangleF BoundingBoxF
        {
            get
            {
                return new RectangleF(this.Position.X, this.Position.Y, this.Size.Width, this.Size.Height);
            }
        }

        public void UpdatePosition()
        {
            float x = this.Position.X + this.Speed.X * Timer.ElapsedTime;
            float y = this.Position.Y + this.Speed.Y * Timer.ElapsedTime;
            this.Position = new Vector2(x, y);
        }

        public bool Colide(Body target)
        {
            return this.BoundingBox.Intersects(target.BoundingBox);
        }
        
    }
}
