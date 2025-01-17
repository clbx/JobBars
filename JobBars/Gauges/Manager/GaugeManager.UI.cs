﻿using Dalamud.Logging;
using ImGuiNET;
using JobBars.Data;
using System;
using System.Numerics;

namespace JobBars.Gauges.Manager {
    public partial class GaugeManager {
        private bool LOCKED = true;
        private static readonly GaugePositionType[] ValidGaugePositionType = (GaugePositionType[])Enum.GetValues(typeof(GaugePositionType));

        protected override void DrawHeader() {
            if (ImGui.Checkbox("Gauges Enabled" + _ID, ref JobBars.Config.GaugesEnabled)) {
                if (JobBars.Config.GaugesEnabled) JobBars.Builder.ShowGauges();
                else JobBars.Builder.HideGauges();
                JobBars.Config.Save();
            }

            if (ImGui.CollapsingHeader("Position" + _ID + "/Row")) DrawPositionRow();

            if (ImGui.CollapsingHeader("Settings" + _ID + "/Row")) DrawSettingsRow();
        }

        private void DrawPositionRow() {
            ImGui.Indent();

            ImGui.Checkbox("Position Locked" + _ID, ref LOCKED);

            if (JobBars.Config.GaugePositionType != GaugePositionType.Split) {
                if (ImGui.Checkbox("Horizontal Gauges", ref JobBars.Config.GaugeHorizontal)) {
                    UpdatePositionScale();
                    JobBars.Config.Save();
                }

                if (ImGui.Checkbox("Bottom-to-Top", ref JobBars.Config.GaugeBottomToTop)) {
                    UpdatePositionScale();
                    JobBars.Config.Save();
                }

                if (ImGui.Checkbox("Align Right", ref JobBars.Config.GaugeAlignRight)) {
                    UpdatePositionScale();
                    JobBars.Config.Save();
                }
            }

            if (JobBars.DrawCombo(ValidGaugePositionType, JobBars.Config.GaugePositionType, "Gauge Positioning", _ID, out var newPosition)) {
                JobBars.Config.GaugePositionType = newPosition;
                JobBars.Config.Save();

                UpdatePositionScale();
            }

            if (JobBars.Config.GaugePositionType == GaugePositionType.Global) { // GLOBAL POSITIONING
                var pos = JobBars.Config.GaugePositionGlobal;
                if(ImGui.DragFloat2("Position" + _ID, ref pos))
                {
                    SetGaugePositionGlobal(pos);
                }
            }
            else if (JobBars.Config.GaugePositionType == GaugePositionType.PerJob) { // PER-JOB POSITIONING
                var pos = GetPerJobPosition();
                if (ImGui.DragFloat2($"Position ({CurrentJob})" + _ID, ref pos))
                {
                    SetGaugePositionPerJob(CurrentJob, pos);
                }

            }

            if (ImGui.DragFloat("Scale" + _ID, ref JobBars.Config.GaugeScale,0.05f)) {
                UpdatePositionScale();
                JobBars.Config.Save();
            }

            ImGui.Unindent();
        }

        private void DrawSettingsRow() {
            ImGui.Indent();

            if (ImGui.Checkbox("Hide Gauges When Out Of Combat", ref JobBars.Config.GaugesHideOutOfCombat)) {
                if (!JobBars.Config.GaugesHideOutOfCombat && JobBars.Config.GaugesEnabled) { // since they might be hidden
                    JobBars.Builder.ShowGauges();
                }
                JobBars.Config.Save();
            }

            if (ImGui.Checkbox("Pulse Diamond and Arrow Color", ref JobBars.Config.GaugePulse)) {
                JobBars.Config.Save();
            }

            ImGui.SetNextItemWidth(50f);
            if (ImGui.InputFloat("Slidecast Seconds (0 = off)", ref JobBars.Config.GaugeSlidecastTime)) {
                JobBars.Config.Save();
            }

            ImGui.Unindent();
        }

        public void DrawPositionBox() {
            if (LOCKED) return;
            if (JobBars.Config.GaugePositionType == GaugePositionType.Split) {
                foreach (var config in CurrentConfigs) config.DrawPositionBox();
            }
            else if (JobBars.Config.GaugePositionType == GaugePositionType.PerJob) {
                var currentPos = GetPerJobPosition();
                if (JobBars.DrawPositionView($"Gauge Bar ({CurrentJob})##GaugePosition", currentPos, out var pos)) {
                    SetGaugePositionPerJob(CurrentJob, pos);
                }
            }
            else { // GLOBAL
                var currentPos = JobBars.Config.GaugePositionGlobal;
                if (JobBars.DrawPositionView("Gauge Bar##GaugePosition", currentPos, out var pos)) {
                    SetGaugePositionGlobal(pos);
                }
            }
        }

        private static void SetGaugePositionGlobal(Vector2 pos) {
            JobBars.SetWindowPosition("Gauge Bar##GaugePosition", pos);
            JobBars.Config.GaugePositionGlobal = pos;
            JobBars.Config.Save();
            JobBars.Builder.SetGaugePosition(pos);
        }

        private static void SetGaugePositionPerJob(JobIds job, Vector2 pos) {
            JobBars.SetWindowPosition($"Gauge Bar ({job})##GaugePosition", pos);
            JobBars.Config.GaugePerJobPosition.Set($"{job}", pos);
            JobBars.Config.Save();
            JobBars.Builder.SetGaugePosition(pos);
        }

        // ==========================================

        protected override void DrawItem(GaugeConfig item) {
            item.Draw(_ID, out bool newPos, out bool newVisual, out bool reset);
            if (SelectedJob != CurrentJob) return;
            if (newPos) UpdatePositionScale();
            if (newVisual) UpdateVisuals();
            if (reset) Reset();
        }

        protected override string ItemToString(GaugeConfig item) => item.Name;

        protected override bool IsEnabled(GaugeConfig item) => item.Enabled;
    }
}
