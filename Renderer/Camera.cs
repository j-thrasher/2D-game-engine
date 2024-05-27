using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.Renderer {
    public class Camera {

        public Vector2D Position;
        public Vector2D Size;

        public Camera(Vector2D Position, Vector2D Size) {
            this.Position = Position;
            this.Size = Size;
        }

        public Vector2D EndBound() {
            return Vector2D.StaticAdd(this.Position, this.Size);
        }

        public void UpdatePosition(Vector2D v) {
            this.Position = new Vector2D(v.X , v.Y);
        }

        public Boolean InFOV(Vector2D Check) {
            return (Check.GreaterThan(this.Position)) && (Check.LessThan(this.EndBound()));
        }

        public Vector2D RelativePosition(Vector2D Vector) {
            return Vector2D.StaticSubtract(Vector, this.Position);
        }

        public Vector2D GetCenter() {
            float x = (this.Size.X / 2f) - this.Position.X;
            float y = (this.Size.Y / 2f) - this.Position.Y;
            return new Vector2D(x, y);
        }

        public float XOffset() {
            return (this.Size.X / 2f) - this.Position.X;
        }

        public float YOffset() {
            return (this.Size.Y / 2f) - this.Position.Y;
        }
    }
}
