2using Sandbox.ModAPI.Ingame;
using Sandbox.Game.EntityComponents;
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

namespace IngameScript {

    partial class Program : MyGridProgram {
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


        public Program() {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Save() {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public const float DEGREES_PER_RADIAN = 180f;
        public const String MONITOR_PANEL_NAME = "Welder Monitor Panel";
        public const String HINGE = "RobotWelderHinge";
        public const String ROTORH1 = "RobotWelderRotor1";
        public const String ROTORV1 = "RobotWelderRotor2";
        public const String ROTORV2 = "RobotWelderRotor3";
        public readonly List<String> components = new List<String>() { HINGE, ROTORH1, ROTORV1, ROTORV2 };


        public void Main(string argument, UpdateType updateSource) {
            ClearLog();
            foreach (String componentName in components) {
                LogComponentInfo((IMyMotorStator) GetBlock(componentName));
            }
        }

        public IMyEntity GetBlock(String name) {
            return GridTerminalSystem.GetBlockWithName(name);
        }

        public void LogComponentInfo(IMyMotorStator rotorOrHinge) {
            String name = rotorOrHinge.CustomName;
            String upperLimit = MaybeLimit(rotorOrHinge.UpperLimitDeg);
            String lowerLimit = MaybeLimit(rotorOrHinge.LowerLimitDeg);
            String velocity = rotorOrHinge.TargetVelocityRPM.ToString("0.00").PadLeft(6);
            String angle = Rad2Deg(rotorOrHinge.Angle).ToString("0.00").PadLeft(6);
            String torque = rotorOrHinge.Torque.ToString("0.00").PadLeft(6).PadLeft(90);
            String Limits = "[" + lowerLimit + ", " + upperLimit + "]";

            Log(name.PadRight(25) + angle + ", " + velocity + "\n" + torque + "\n" + Limits.PadLeft(90));
        }

        public String MaybeLimit(float limit) {
            if(float.IsNegativeInfinity(limit) || float.IsPositiveInfinity(limit) || limit == float.MaxValue || limit == float.MinValue) {
                return "No Limit";
            } else {
                return limit.ToString("0.00").PadLeft(6);
            }
        }

        public float Rad2Deg(float radians) {
            return radians * (DEGREES_PER_RADIAN / (float)Math.PI);
        }

        public void Log(String text, bool shouldAppend = true, bool shouldNewLine = true) {
            IMyTextSurface logPanel = (IMyTextSurface)GetBlock(MONITOR_PANEL_NAME);
            logPanel.ContentType = ContentType.TEXT_AND_IMAGE;
            String maybeNewLine = shouldNewLine ? "\n" : "";
            logPanel.WriteText(text + maybeNewLine, shouldAppend);
        }

        public void ClearLog() {
            IMyTextSurface logPanel = (IMyTextSurface)GetBlock(MONITOR_PANEL_NAME);
            logPanel.ContentType = ContentType.TEXT_AND_IMAGE;
            logPanel.WriteText("");
        }
    }
}
