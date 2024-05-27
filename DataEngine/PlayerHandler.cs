using GameEngine.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.DataEngine {
    public class PlayerHandler {

        private Player P;
        private Window W;

        public PlayerHandler(Window w) {
            this.W = w;
            Vector2D PlayerPos = Vector2D.StaticDivide(W.GetScreenSize(), 2);
            Sprite2D s = new Sprite2D("player", Window.PLAYER);
            this.P = new Player(new Vector2D(0, 0), s);
            this.P.Momentum = new Vector2D(1, 1);
            this.P.AddAnim(State.MOVING_DOWN, "player_down", Window.PLAYER, 1, 8, 5);
            this.P.AddAnim(State.MOVING_RIGHT, "player_right", Window.PLAYER, 1, 8, 5);
            this.P.AddAnim(State.MOVING_LEFT, "player_left", Window.PLAYER, 1, 8, 5);
            this.P.AddAnim(State.MOVING_UP, "player_up", Window.PLAYER, 1, 8, 5);
        }

        public Player GetPlayer() {
            return this.P;
        }

        public void HandleLogicUpdate() {
            UpdatePlayer();
            UpdateCamera();
        }

        public void UpdateCamera() {
            this.W.Cam.UpdatePosition(this.P.Position);
        }

        public void UpdatePlayer() {
            float XSpeed = 0F;
            float YSpeed = 0F;
            Boolean w = this.W.IsKeyPushed("W");
            Boolean s = this.W.IsKeyPushed("S");
            Boolean a = this.W.IsKeyPushed("A");
            Boolean d = this.W.IsKeyPushed("D");
            if (w) YSpeed = (YSpeed < Player.BaseMomentum.Y) ? 0 - Player.BaseMomentum.Y : 0 - this.P.Momentum.Y;
            if (s) YSpeed = (YSpeed < Player.BaseMomentum.Y) ? Player.BaseMomentum.Y : this.P.Momentum.Y;
            if (d) XSpeed = (XSpeed < Player.BaseMomentum.X) ? Player.BaseMomentum.X : this.P.Momentum.X;
            if (a) XSpeed = (XSpeed < Player.BaseMomentum.X) ? 0 - Player.BaseMomentum.X : 0 - this.P.Momentum.X;

            if (w && s) YSpeed = 0;
            if (a && d) XSpeed = 0;

            //this.W.Log($"XSpeed: {XSpeed} - YSpeed {YSpeed} w - {w} s - {s} a - {a} d - {d}");
            this.P.UpdateMomentum(new Vector2D(XSpeed, YSpeed));
            this.P.UpdatePosition();
            this.P.HandleUpdate();
        }
    }
}
