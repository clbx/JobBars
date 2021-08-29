﻿using ImGuiNET;
using JobBars.Data;
using JobBars.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace JobBars.Icons {
    public struct IconProps {
        public bool IsDoT;
        public ActionIds[] Icons;
        public IconTriggerStruct[] Triggers;
    }

    public struct IconTriggerStruct {
        public Item Trigger;
        public float Duration;
    }

    public class IconReplacer {
        public enum IconState {
            Inactive,
            Active
        }

        public readonly string Name;
        public bool Enabled;
        private IconState State = IconState.Inactive;
        private IconProps Props;
        private readonly List<uint> Icons;
        
        public IconReplacer(string name, IconProps props) {
            Name = name;
            Enabled = JobBars.Config.IconEnabled.Get(Name);
            Props = props;
            Icons = new List<ActionIds>(props.Icons).Select(x => (uint)x).ToList();
        }

        public void Setup() {
            State = IconState.Inactive;
            JobBars.IconBuilder.Setup(Icons, Props.IsDoT);
        }

        public void Tick() {
            var timeLeft = -1f;
            var maxDuration = 1f;
            foreach(var trigger in Props.Triggers) {
                if(UIHelper.PlayerStatus.TryGetValue(trigger.Trigger, out var value)) {
                    timeLeft = value.RemainingTime;
                    maxDuration = trigger.Duration;
                    break;
                }
            }

            if(timeLeft > 0 && State == IconState.Inactive) {
                State = IconState.Active;
            }

            if(State == IconState.Active) {
                if(timeLeft <= 0) {
                    timeLeft = 0;
                    State = IconState.Inactive;
                }

                if (timeLeft == 0) ResetIcon();
                else SetIcon(timeLeft, maxDuration);
            }
        }

        private void SetIcon(float current, float duration) {
            if (Props.IsDoT) JobBars.IconBuilder.SetTimerProgress(Icons, current, duration);
            else JobBars.IconBuilder.SetBuffProgress(Icons, current);
        }

        private void ResetIcon() {
            if (Props.IsDoT) JobBars.IconBuilder.SetTimerDone(Icons);
            else JobBars.IconBuilder.SetBuffDone(Icons);
        }

        public void Draw(string id, JobIds job) {
            var _ID = id + Name;
            var type = Props.IsDoT ? "DOT" : "BUFF";

            ImGui.TextColored(Enabled ? new Vector4(0, 1, 0, 1) : new Vector4(1, 0, 0, 1), $"{Name} [{type}]");

            if (JobBars.Config.IconEnabled.Draw($"Enabled{_ID}", Name, out var newEnabled)) {
                Enabled = newEnabled;

                if (JobBars.IconManager.CurrentJob == job && JobBars.Config.IconsEnabled) {
                    if (Enabled) Setup();
                    else JobBars.IconBuilder.Remove(Icons);
                }
            }
        }
    }
}
