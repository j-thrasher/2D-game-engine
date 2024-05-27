using GameEngine.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.DataEngine {

    public class DataHandler {


        private PlayerHandler playerHander;
        private Window W;
        private Dictionary<String, Player> Players = new Dictionary<String, Player>();
        private Dictionary<String, Entity> Entities = new Dictionary<String, Entity>();
        public Dictionary<String, Series> DebugSeries = new Dictionary<String, Series>();


        public DataHandler(Window w) {
            this.W = w;
            playerHander = new PlayerHandler(W);
            AddEntity(this.GetPlayer());

            this.W.AddDebugSeries("PlayerHandler");
            this.W.AddDebugSeries("SpriteUpdates");
            this.W.AddDebugSeries("Collisions");




            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 5; j++) {
                    Vector2D Pos = new Vector2D(i * 100, j * 100);
                    Sprite2D s = new Sprite2D("slime", Window.MOB);
                    Mob slime = new Mob(Pos, "Mob", s);
                    slime.VulnCoolDownEffect = 80;
                    AddEntity(slime);
                }
            }
        }

        private void AddEntity(Object obj) {
            Register(obj);
            W.Register(obj);
        }

        public void Register(Object obj) {
            if (((Entity)obj).Tag.Equals("Mob")) {
                Entity c = (Entity) obj;
                c.ID = Util.SHA256Hash(this.Entities.Count + Environment.TickCount + "");
                this.Entities.Add(c.ID, c);
                //this.W.Log($"Registered {c.Tag}: " + c.ID);
            }
            if (((Entity)obj).Tag.Equals("Player")) {
                Player c = ((Player)obj);
                c.ID = Util.SHA256Hash(this.Entities.Count + Environment.TickCount + "");
                this.Players.Add(c.ID, c);
                this.W.Log("Registered Player: " + c.ID);
            }
        }

        public void Unregister(Object obj) {
            if (obj is Entity) {
                Entity c = (Entity)obj;
                this.Entities.Remove(c.ID);
                this.W.Log("Unregistered Sprite: " + c.ID);
            }
            if (obj is Player) {
                Player c = ((Player)obj);
                this.Players.Remove(c.ID);
                this.W.Log("Unregistered Player: " + c.ID);
            }
        }

        public Player GetPlayer() {
            return this.playerHander.GetPlayer();
        }

        public void HandleLogicUpdate() {
            long beforePlayerHandler = Util.nanoTime();
            this.playerHander.HandleLogicUpdate();
            long beforeSpriteUpdates = Util.nanoTime();
            HandleSpriteUpdates();
            long beforeCollisions = Util.nanoTime();
            HandleCollisions();
            long afterCollisions = Util.nanoTime();

            this.W.AddDebugSeriesTiming("PlayerHandler", beforePlayerHandler, beforeSpriteUpdates);
            this.W.AddDebugSeriesTiming("SpriteUpdates", beforeSpriteUpdates, beforeCollisions);
            this.W.AddDebugSeriesTiming("Collisions", beforeCollisions, afterCollisions);


        }


        private void HandleSpriteUpdates() {
            
            
        }

        private void HandleCollisions() {
            Player p = GetPlayer();
            foreach(Entity s in Entities.Values.ToList()) {
                //this.W.Log($"{p.Position.X}, {p.Position.Y} - {s.Position.X}, {s.Position.Y} | {p.Position.Distance(s.Position) < 100f}");




                if (p.Position.Distance(s.Position) < Window.SimulationDistance) {
                    Polygon2D PlayerHitBox = p.GetGlobalHitbox();
                    Polygon2D SpriteHitBox = s.GetGlobalHitbox();
                    if (PlayerHitBox.Intersects(SpriteHitBox)) {
                        CollisionEventArgs a = new CollisionEventArgs(s, Util.SHA256Hash(DateTime.Now.ToString("s.f") + s.ID));
                        p.HandleCollision(a);
                    }
                }
            }
        }
    }
}
