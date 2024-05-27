using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GameEngine.DataEngine;
using GameEngine.Renderer;

namespace Game {
    public class EndBorne : Window {

        public int frame = 0;
        private DataHandler dataHandler;

        public EndBorne() : base(new Vector2D(1000, 1000), "Endborne") {



        }

        public override void Onload() {
            this.Bg = Window.DebugBackGroundColor;
            dataHandler = new DataHandler(this);
        }

        public override void OnUpdate() {
            this.frame++;

            dataHandler.HandleLogicUpdate();
        }
    }
}
