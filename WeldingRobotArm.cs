using Sandbox.ModAPI.Ingame;
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
            Log("Compiling Welding Robot Arm script. runCount = " + runCount.ToString());
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
        public const String HINGE = "RobotWelderHinge";
        public const String ROTORH1 = "RobotWelderRotor1";
        public const String ROTORV1 = "RobotWelderRotor2";
        public const String ROTORV2 = "RobotWelderRotor3";
        public const int ROTATION_WAIT_RUNS = 10;
        public const float DEFAULT_VELOCITY = 1.0f;

        public int runCount = 0;
        readonly List<List<MyTuple<String, float>>> rotations = new List<List<MyTuple<String, float>>>(){
            new List<MyTuple<String, float>>{
                    MyTuple.Create(HINGE, -20f),
                    MyTuple.Create(ROTORH1, 100f),
                    MyTuple.Create(ROTORV1, 20f),
                    MyTuple.Create(ROTORV2, 20f)}
        };
        public bool isFinished = true;

        public void Main(string argument, UpdateType updateSource) {
            if (ShouldStop(updateSource)) {
                Log("Stopping because finished.");
                Runtime.UpdateFrequency = UpdateFrequency.None;
                return;
            } else {
                Runtime.UpdateFrequency = UpdateFrequency.Update100;
                isFinished = false;
            }
            if(runCount == 0) {
                ClearLog();
            }
            if (runCount % 10 == 0) {
                Log("10 ticks (or zero): (" + runCount.ToString() + ")");
            } else {
                Log(runCount.ToString() + ",", shouldNewLine: false);
                runCount++;
                return;
            }
            int currentRotationsList = runCount / ROTATION_WAIT_RUNS;
            if (currentRotationsList > rotations.LongCount() - 1) {
                Log("Finished rotation list; setting isFinished to true.");
                isFinished = true;
                runCount = 0;
                return;
            }
            Log("Running rotations " + currentRotationsList.ToString());
            Rotate(rotations[currentRotationsList]);
            runCount++;
        }

        public bool ShouldStop(UpdateType updateSource) {
            UpdateType manualRun = UpdateType.None | UpdateType.Terminal;
            bool isManualRun = (updateSource & manualRun) != 0;
            //Log("Is manual run? " + isManualRun.ToString() + "; " +
            //    "is None? " + (updateSource == 0).ToString() + "; " +
            //    "is terminal? " + (updateSource == UpdateType.Terminal).ToString() + "; " +
            //    "update type: " + updateSource.ToString());
            return isFinished && !isManualRun;
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

        public void Rotate(List<MyTuple<String, float>> rotations) {
            IMyMotorStator rotater;
            foreach (var rotation in rotations) {
                rotater = (IMyMotorStator)GetBlock(rotation.Item1);
                Rotate(rotater, rotation.Item2);
            }
        }

        public void Rotate(IMyMotorStator rotater, float target, float velocity = DEFAULT_VELOCITY) {
            float currDegrees = Rad2Deg(rotater.Angle);
            float currVelocity = rotater.TargetVelocityRPM;
            Log("Rotating " + rotater.CustomName + " from " + currDegrees.ToString() + " to " + target.ToString() + " degrees at velocity " + currVelocity.ToString());
            if (currDegrees < target) {
                rotater.UpperLimitDeg = target;
                rotater.LowerLimitDeg = currDegrees;
                rotater.TargetVelocityRPM = velocity;
            } else {
                rotater.UpperLimitDeg = currDegrees;
                rotater.LowerLimitDeg = target;
                rotater.TargetVelocityRPM = -velocity;
            }
        }

        public float Rad2Deg(float radians) {
            return radians * (DEGREES_PER_RADIAN / (float) Math.PI);
        }

        public void Log(String text, bool shouldAppend = true, bool shouldNewLine = true) {
            IMyTextSurface logPanel = (IMyTextSurface)GetBlock(LOGGING_PANEL_NAME);
            logPanel.ContentType = ContentType.TEXT_AND_IMAGE;
            String maybeNewLine = shouldNewLine ? "\n" : "";
            logPanel.WriteText(text + maybeNewLine, shouldAppend);
        }

        public void ClearLog() {
            IMyTextSurface logPanel = (IMyTextSurface)GetBlock(LOGGING_PANEL_NAME);
            logPanel.ContentType = ContentType.TEXT_AND_IMAGE;
            logPanel.WriteText("");
        }
    }
}
