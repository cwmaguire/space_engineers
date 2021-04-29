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

        // Example of old-school setter:
        // rotor.SetValue<float>("UpperLimit", target);
        // However, rotors are, oddly enough, IMyMotorStator, not IMyMotorRotor;
        // IMyMotorStator has built-in properties whereas IMyMotorRotor does not.

        public const float DEGREES_PER_RADIAN = 180f;
        public const String LOGGING_PANEL_NAME = "Welder Logging Panel";
        public const String HINGE = "RobotWelderHinge";
        public const String ROTORH1 = "RobotWelderRotor1";
        public const String ROTORV1 = "RobotWelderRotor2";
        public const String ROTORV2 = "RobotWelderRotor3";
        public const int ROTATION_WAIT_RUNS = 20;
        public const float DEFAULT_VELOCITY = 10f;

        public int runCount = 0;
        public List<List<MyTuple<String, float>>> resetRotations;
        readonly List<List<MyTuple<String, float>>> rotations = new List<List<MyTuple<String, float>>>(){
            //new List<MyTuple<String, float>>{
            //        MyTuple.Create(HINGE, 0f),
            //        MyTuple.Create(ROTORH1, 90f),
            //        MyTuple.Create(ROTORV1, 0f),
            //        MyTuple.Create(ROTORV2, 0f)},
            //new List<MyTuple<String, float>>{
            //        MyTuple.Create(ROTORH1, 100f),
            //        MyTuple.Create(ROTORV2, 75f)},
            //new List<MyTuple<String, float>>{
            //        MyTuple.Create(HINGE, 15f),
            //        MyTuple.Create(ROTORV1, -15f)}
        };
        public bool isFinished = true;
        public bool isResetFinished = false;

        public void Main(string argument, UpdateType updateSource) {
            if (ShouldStop(updateSource)) {
                Log("Stopping because finished.");
                Runtime.UpdateFrequency = UpdateFrequency.None;
                return;
            } else {
                Runtime.UpdateFrequency = UpdateFrequency.Update100;
                isFinished = false;
            }
            if(runCount == 0 && !isResetFinished) {
                ClearLog();
            }
            if (runCount % ROTATION_WAIT_RUNS == 0) {
                Log(ROTATION_WAIT_RUNS.ToString() + " ticks (or zero): (" + runCount.ToString() + ")");
            } else {
                Log(runCount.ToString() + ",", shouldNewLine: false);
                runCount++;
                return;
            }

            int currentRotationsList = runCount / ROTATION_WAIT_RUNS;

            if (isResetFinished) {
                if (currentRotationsList > rotations.LongCount() - 1) {
                    Log("Finished rotation list; setting isFinished to true.");
                    isFinished = true;
                    isResetFinished = false;
                    runCount = 0;
                    return;
                }
                Log("Running rotations " + currentRotationsList.ToString());
                Rotate(rotations[currentRotationsList]);
            } else {
                if(runCount == 0) {
                    resetRotations = ResetRobotSafely();
                }
                if (currentRotationsList > resetRotations.LongCount() - 1) {
                    Log("Finished reset rotation list; setting runCount to 0 and isResetFinished to true.");
                    isResetFinished = true;
                    resetRotations = null;
                    runCount = 0;
                    return;
                }
                Log("Running reset rotations " + currentRotationsList.ToString());
                Rotate(resetRotations[currentRotationsList]);
            }
            runCount++;
        }

        public bool ShouldStop(UpdateType updateSource) {
            UpdateType manualRun = UpdateType.None | UpdateType.Terminal | UpdateType.Trigger;
            bool isManualRun = (updateSource & manualRun) != 0;
            //Log("Is manual run? " + isManualRun.ToString() + "; " +
            //    "is None? " + (updateSource == 0).ToString() + "; " +
            //    "is trigger? " + (updateSource == UpdateType.Trigger).ToString() + "; " +
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

        public List<List<MyTuple<String, float>>> ResetRobotSafely() {
            return new List<List<MyTuple<String, float>>>() {
            // Move arm 1 back to origin while moving the other arms to stay relatively still
                ResetArm1(),
                ResetArm2(),
                ResetArm3()
            };
        }

        // I need to return the arms back to origin without banging into anything; if I move arm 2 in the opposite way from arm 1 then they
        // should start to "scissor" together. At the same time, I need to move arm three in the _opposite_ direction, since the angle of arm 2 is changing.
        // Arm 3 will try to stay at the same angle 

        // Or, I could "straighten out" if I've got wide angles (e.g. arm 2 is > 90 deg from arm 1)
        // or "curl up" if I've got narrow angles
        public List<MyTuple<String, float>> ResetArm1() {
            // if arm 2 is > 90 away from arm 1 then we're extended, so fully extend by going to -180
            // else we've got a very sharp elbow formed by arms 1 and 2 and we should take arm 2 back towards 0/360
            
            return new List<MyTuple<String, float>>() {
                MyTuple.Create(HINGE, 0f),
            };
        }
        public List<MyTuple<String, float>> ResetArm2() {
            return new List<MyTuple<String, float>>() {

            };

        }
        public List<MyTuple<String, float>> ResetArm3() {
            return new List<MyTuple<String, float>>() {

            };

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
            bool isHinge = IsHinge(rotater);
            if (IsHinge(rotater)) {
                RotateHinge(rotater, target, velocity);
            } else {
                RotateRotor(rotater, target, velocity);
            }
        }

        public void RotateHinge(IMyMotorStator hinge, float rawTarget, float velocity) {
            float currDegrees = Rad2Deg(hinge.Angle);
            //float currVelocity = hinge.TargetVelocityRPM;
            float target = Clamp(rawTarget, 90f, -90f);
            Log("Rotating hinge " + hinge.CustomName +
                " from " + currDegrees.ToString() + 
                " to " + target.ToString() + " (clamped: " + target.ToString() + ") " +
                " degrees at velocity " + velocity.ToString());
            if (currDegrees < target) {
                hinge.UpperLimitDeg = target;
                hinge.LowerLimitDeg = currDegrees;
                hinge.TargetVelocityRPM = velocity;
            } else {
                hinge.UpperLimitDeg = currDegrees;
                hinge.LowerLimitDeg = target;
                hinge.TargetVelocityRPM = -velocity;
            }
        }

        public void RotateRotor(IMyMotorStator rotor, float target, float velocity) {
            float currDegrees = Rad2Deg(rotor.Angle);

            Log("Rotating " + rotor.CustomName +
                " from " + currDegrees.ToString() +
                " to " + target.ToString() +
                " degrees at velocity " + velocity.ToString());
            if (currDegrees < target) {
                rotor.UpperLimitDeg = target;
                rotor.LowerLimitDeg = currDegrees;
                rotor.TargetVelocityRPM = velocity;
            } else {
                rotor.UpperLimitDeg = currDegrees;
                rotor.LowerLimitDeg = target;
                rotor.TargetVelocityRPM = -velocity;
            }
        }

        //public bool IsBackwardsCloser(float targetDegrees, float currentDegrees) {
        //    float correctedTargetDegrees = AbsDegrees(targetDegrees);
        //    float correctedCurrentDegrees = AbsDegrees(currentDegrees);
        //    return Math.Abs(correctedTargetDegrees - correctedCurrentDegrees) > 180f;
        //}

        //public float AbsDegrees(float degrees) {
        //    if (degrees < 0) {
        //        return 360f + degrees;
        //    } else {
        //        return degrees;
        //    }
        //}

        public float Rad2Deg(float radians) {
            return radians * (DEGREES_PER_RADIAN / (float) Math.PI);
        }

        public float Clamp(float toClamp, float Max, float Min) {
            return Math.Min(90f, Math.Max(-90f, toClamp));
        }

        public bool IsHinge(IMyMotorStator motor) {
            // TODO: find a better way to do this. SE doesn't seem to allow type comparisons
            return motor.CustomName.Contains("Hinge");
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
