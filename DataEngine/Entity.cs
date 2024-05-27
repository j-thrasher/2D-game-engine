using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using Rectangle = System.Drawing.Rectangle;
using GameEngine.Renderer;


namespace GameEngine.DataEngine {






    public abstract class Entity {



        public State State;
        private Sprite2D Sprite;
        public Vector2D Position;
        public String Tag = "";
        public String ID = "";
        private Rectangle BoundingBox;
        public Boolean Vuln = false;
        public int VulnCoolDown = 0;
        public int VulnCoolDownEffect = 0;

        public Vector2D Momentum = new Vector2D();
        public static Vector2D BaseMomentum = new Vector2D(10f, 10f);

        public Dictionary<String, CollisionEventArgs> HandledCollisions = new Dictionary<string, CollisionEventArgs>();
        public List<CollisionEventArgs> Collisions = new List<CollisionEventArgs>();
        int CollisionRegisterLimit = 100;

        public Dictionary<String, Animation> Anims = new Dictionary<String, Animation>();


        public abstract Boolean HandleCollision(CollisionEventArgs Collision);
        public abstract void HandleUpdate();
        public abstract void HandleStateUpdate();

        public Entity(Vector2D Position, String Tag, Sprite2D Sprite) {
            this.Position = Position;
            this.Tag = Tag;
            this.Sprite = Sprite;
            this.BoundingBox = this.Sprite.GeneratedHitBox().getBoundBox();
            this.Vuln = true;
            this.State = new State();
        }

        public Sprite2D GetSprite() {
            if (this.State.Form.Equals(State.IDLE)) {
                String LastAnim = this.State.IdleSaveState();
                if (Anims.ContainsKey(LastAnim)) {
                    Sprite2D ReturnVal = this.Anims[LastAnim].GetSprite(0);
                    if (ReturnVal != null) {
                        return ReturnVal;
                    }
                }
            }

            if (Anims.ContainsKey(this.State.Form)) {
                Sprite2D Current = this.Anims[this.State.Form].GetSprite(CurrentFrame());
                if(Current != null) {
                    return Current;
                }
            }
            return this.Sprite;
        }

        public int CurrentFrameCount() {
            if (Anims.ContainsKey(this.State.Form)) {
                return Anims[this.State.Form].FrameCount();
            } else {
                return 0;
            }
        }

        public int CurrentFrame() {
            if (Anims.ContainsKey(this.State.Form)) {
                return Anims[this.State.Form].GetRelativeFrame(this.State.Count);
            } else {
                return 0;
            }
        }

        public String CurrentAnimName() {
            if (Anims.ContainsKey(this.State.Form)) {
                return this.State.Form;
            } else {
                return "NULL";
            }
        }

        public void AddAnim(String State, String Dir, int Layer, int Rows, int Columns, int TicksPerFrame) {
            Animation a = new Animation(TicksPerFrame);
            a.AddSpritesViaSheet(Dir, Layer, Rows, Columns);
            AddAnim(State, a);
        }

        public void AddAnim(String S, Animation A) {
            this.Anims.Add(S, A);
        }

        public Boolean AlreadyHandled(CollisionEventArgs e) {
            return this.HandledCollisions.ContainsKey(e.ID);
        }

        public void AddHandledCollision(CollisionEventArgs e) {
            this.Collisions.Add(e);
            this.HandledCollisions.Add(e.ID, e);
            if(this.Collisions.Count > this.CollisionRegisterLimit) {
                String IDToRemove = this.Collisions[0].ID;
                this.Collisions.RemoveAt(0);
                this.HandledCollisions.Remove(IDToRemove);
            }
        }

        public void AddVulnCoolDown(int Val) {
            this.VulnCoolDown += Val;
            if(VulnCoolDown > 0) {
                this.Vuln = false;
            }
        }

        public int EffectOnVulnTimer() {
            return this.VulnCoolDownEffect;
        }

        public void VulnTick() {
            if(this.VulnCoolDown > 0) {
                this.VulnCoolDown--;
                if(this.VulnCoolDown == 0) {
                    this.Vuln = true;
                }
            }
        }

        public void UpdatePosition() {
            this.Position.Add(Momentum);
        }

        public void UpdateMomentum(Vector2D Vector) {
            this.Momentum = Vector;
        }

        public float SpriteXOffset() {
            return (this.Sprite.Sprite.Width * this.Sprite.XScale) / 2;
        }
        public float SpriteYOffset() {
            return (this.Sprite.Sprite.Height * this.Sprite.YScale) / 2;
        }

        public Vector2D GetCenterOfEntityInSpace(float XOffset, float YOffset) {
            return new Vector2D(XOffset - this.SpriteXOffset(), YOffset - SpriteYOffset());
        }


        public Polygon2D GetGlobalHitbox(float XOffset, float YOffset) {
            List<PointF> points = GetSprite().GeneratedHitBox().GetPointList();
            Polygon2D relativePolygon = new Polygon2D();
            float X = this.Position.X;
            float Y = this.Position.Y;
            foreach (PointF point in points) {
                relativePolygon.AddPoint(new PointF(point.X + X + XOffset, point.Y + Y + YOffset));
            }
            return relativePolygon;
        }

        public Polygon2D GetGlobalHitbox() {
            List<PointF> points = GetSprite().GeneratedHitBox().GetPointList();
            Polygon2D relativePolygon = new Polygon2D();
            float X = this.Position.X;
            float Y = this.Position.Y;
            foreach (PointF point in points) {
                relativePolygon.AddPoint(new PointF(point.X + X, point.Y + Y));
            }
            return relativePolygon;
        }

        public Rectangle GetGlobalBoundingBox() {
            int MinX = (int)(this.BoundingBox.X + this.Position.X);
            int MinY = (int)(this.BoundingBox.Y + this.Position.Y);
            int MaxX = (int)(this.BoundingBox.Width);
            int MaxY = (int)(this.BoundingBox.Height);
            return new Rectangle(MinX, MinY, MaxX, MaxY);
        }
    }
}
