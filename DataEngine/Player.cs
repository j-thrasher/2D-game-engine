using GameEngine.Renderer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rectangle = System.Drawing.Rectangle;

namespace GameEngine.DataEngine {
    public class Player : Entity {


        public Player(Vector2D Pos, Sprite2D Sprite) : base (Pos, "Player", Sprite) {
            
        }

        public override void HandleUpdate() {
            HandleStateUpdate();
        }

        public override void HandleStateUpdate() {
            float XState = this.Momentum.X;
            float YState = this.Momentum.Y;
            if(XState == 0f && YState == 0f) this.State.Add(State.IDLE);
            if (XState > 0f) this.State.Add(State.MOVING_RIGHT);
            else if (XState < 0f) this.State.Add(State.MOVING_LEFT);
            else if (YState > 0f) this.State.Add(State.MOVING_DOWN);
            else if (YState < 0f) this.State.Add(State.MOVING_UP);
            VulnTick();
        }

        public override bool HandleCollision(CollisionEventArgs Collision) {
            if (AlreadyHandled(Collision)) {
                //Console.WriteLine($"Player Collision Event: {Collision.ID} Already Handled");
                return true;
            } else {
                switch(Collision.Sender.Tag) {
                    case ("Mob"):
                        HandleMobCollision(Collision);
                        break;
                }
                return true;
            }
        }

        public void HandleMobCollision(CollisionEventArgs Collision) {
            
            if (this.Vuln) {
                this.AddVulnCoolDown(Collision.Sender.EffectOnVulnTimer());
                Console.WriteLine($"Player Collision Event: {Collision.ID} with Mob: {Collision.Sender.ID}");
            }
            AddHandledCollision(Collision);
        }
    }
}
