﻿using ImGuiNET;
using JobBars.Data;
using JobBars.UI;

namespace JobBars.Gauges.Procs {
    public struct GaugeProcProps {
        public bool ShowText;
        public ProcConfig[] Procs;
        public GaugeCompleteSoundType ProcSound;
    }

    public class ProcConfig {
        public readonly string Name;
        public readonly Item Trigger;

        public ElementColor Color;
        public int Order;

        public ProcConfig(string name, BuffIds buff, ElementColor color) : this(name, new Item(buff), color) { }
        public ProcConfig(string name, ActionIds action, ElementColor color) : this(name, new Item(action), color) { }
        public ProcConfig(string name, Item trigger, ElementColor color) {
            Name = name;
            Trigger = trigger;
            Color = JobBars.Config.GaugeProcColor.Get(Name, color);
            Order = JobBars.Config.GaugeProcOrder.Get(Name);
        }
    }

    public class GaugeProcsConfig : GaugeConfig {
        private static readonly GaugeVisualType[] ValidGaugeVisualType = new[] { GaugeVisualType.Diamond };
        protected override GaugeVisualType[] GetValidGaugeTypes() => ValidGaugeVisualType;

        public ProcConfig[] Procs { get; private set; }
        public bool ProcsShowText { get; private set; }
        public GaugeCompleteSoundType ProcSound { get; private set; }

        public GaugeProcsConfig(string name, GaugeVisualType type, GaugeProcProps props) : base(name, type) {
            Procs = props.Procs;
            ProcsShowText = JobBars.Config.GaugeShowText.Get(Name, props.ShowText);
            ProcSound = JobBars.Config.GaugeCompletionSound.Get(Name, props.ProcSound);
        }

        public override GaugeTracker GetTracker(int idx) => new GaugeProcsTracker(this, idx);

        protected override void DrawConfig(string id, ref bool newPos, ref bool newVisual, ref bool reset) {
            if (JobBars.Config.GaugeShowText.Draw($"Show Text{id}", Name, ProcsShowText, out var newProcsShowText)) {
                ProcsShowText = newProcsShowText;
                newPos = true;
                newVisual = true;
            }

            if (JobBars.Config.GaugeCompletionSound.Draw($"Proc Sound{id}", Name, ValidSoundType, ProcSound, out var newProcSound)) {
                ProcSound = newProcSound;
            }

            DrawSoundEffect("Proc Sound Effect");

            foreach (var proc in Procs) {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10);

                if (JobBars.Config.GaugeProcOrder.Draw($"Order ({proc.Name})", proc.Name, proc.Order, out var newOrder)) {
                    proc.Order = newOrder;
                    reset = true;
                }

                if (JobBars.Config.GaugeProcColor.Draw($"Color ({proc.Name})", proc.Name, proc.Color, out var newColor)) {
                    proc.Color = newColor;
                    reset = true;
                }
            }
        }
    }
}
