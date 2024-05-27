using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameEngine.DataEngine {
    public class State {

        public int Count = 0;
        public String Form = "";
        private String LastNonIdleState = "";

        public static int CountLimit = 3600;

        public static String MOVING_RIGHT = "MOVING_RIGHT";
        public static String MOVING_LEFT = "MOVING_LEFT";
        public static String MOVING_UP = "MOVING_UP";
        public static String MOVING_DOWN = "MOVING_DOWN";
        public static String HIT = "HIT";
        public static String IDLE = "IDLE";


        public State() { }

        public void Add() {
            this.Count++;
            if(this.Count > CountLimit) {
                this.Count = 0;
            }
        }

        public void Add(String State) {
            if(this.Form.Equals(State)) {
                Add();
            } else {
                this.LastNonIdleState = this.Form;
                this.Form = State;
                this.Count = 1;
            }
        }

        public String IdleSaveState() {
            if(LastNonIdleState.Equals("")) {
                return "NULL";
            } else {
                return LastNonIdleState;
            }
        }
    }
}
