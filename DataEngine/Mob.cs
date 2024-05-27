using GameEngine.Renderer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rectangle = System.Drawing.Rectangle;

namespace GameEngine.DataEngine {
    public class Mob : Entity {


        public Mob(Vector2D Pos, String Tag, Sprite2D Sprite) : base (Pos, Tag, Sprite) {

        }

        public override bool HandleCollision(CollisionEventArgs Collision) {

            return true;
        }

        public override void HandleStateUpdate() {
            
        }

        public override void HandleUpdate() {
        }
    }
}
