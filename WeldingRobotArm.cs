using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.
        // 
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        //
        // to learn more about ingame scripts.

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Save(){
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public const float DEGREES_PER_RADIAN = 180f;
        public const String LOGGING_PANEL_NAME = "Welder Logging Panel";
        public const String BOTTOM = "HingeWelderBottom";
        public const String TOP = "HingeWelderTOP";
        public const String ROTOR = "RobotWelderRotor";

        List<List<MyTuple<String, float>>> rotations = new List<List<MyTuple<String, float>>>(){
            new List<MyTuple<String, float>>{
                    MyTuple.Create(ROTOR, 90f),
                    MyTuple.Create(BOTTOM, 0f),
                    MyTuple.Create(TOP, 0f)},
            new List<MyTuple<String, float>>{
                    MyTuple.Create(ROTOR, 90f),
                    MyTuple.Create(BOTTOM, 0f),
                    MyTuple.Create(TOP, 0f)}
        };

        public void Main(string argument, UpdateType updateSource) {
            IMyTextSurface logPanel = SetupLogging();

            List<MyTuple<IMyMotorStator, float>> rotations =
                new List<MyTuple<IMyMotorStator, float>> {
                    MyTuple.Create((IMyMotorStator)rotor, 90f),
                    MyTuple.Create(bottom, 0f),
                    MyTuple.Create(top, 0f)
                };
            Rotate(rotations);
        }

        public IMyTextSurface SetupLogging() {
            IMyTextSurface logPanel = (IMyTextSurface)GetBlock(LOGGING_PANEL_NAME);
            logPanel.ContentType = ContentType.TEXT_AND_IMAGE;
            logPanel.WriteText("");
            return logPanel;
        }

        public IMyEntity GetBlock(String name) {
            return GridTerminalSystem.GetBlockWithName(name);
        }

        public void Rotate(List<MyTuple<IMyMotorStator, float>> rotations) {
            foreach (var rotation in rotations) {
                Rotate(rotation);
            }
        }

        public void Rotate(MyTuple<IMyMotorStator, float> rotation) {
            Rotate(rotation.Item1, rotation.Item2);
        }

        public void Rotate(IMyMotorStator rotater, float target, float velocity = 1.0f) {
            log("Rotating " + rotater.CustomName + " to " + target.ToString() + " degrees");
            rotater.UpperLimitDeg = target;
            rotater.LowerLimitDeg = target;
            float degrees = rad2deg(rotater.Angle);
            if (degrees > target) {
                rotater.TargetVelocityRPM = -velocity;
            }
            else {
                rotater.TargetVelocityRPM = velocity;
            }
        }

        public float rad2deg(float radians) {
            return radians * DEGREES_PER_RADIAN;
        }

        public void log(String text) {
            IMyTextSurface textPanel = (IMyTextSurface)GetBlock(LOGGING_PANEL_NAME);
            textPanel.WriteText(text + "\n", true);
        }
    }
}
