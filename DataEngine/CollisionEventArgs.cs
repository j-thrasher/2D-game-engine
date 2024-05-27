using System;

namespace GameEngine.DataEngine {
    public class CollisionEventArgs {

        public Entity Sender;
        public string ID = "";

        public int VulnCoolDown = 0;

        public CollisionEventArgs(Entity Sender, string ID) {
            this.Sender = Sender;
            this.ID = ID;
        }

        public Boolean Same(CollisionEventArgs CE) {
            return CE.ID.Equals(this.ID);
        }
    }
}