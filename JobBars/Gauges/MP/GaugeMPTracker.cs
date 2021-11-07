﻿using JobBars.Gauges.Types.Bar;
using JobBars.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBars.Gauges.MP {
    public class GaugeMPTracker : GaugeTracker, IGaugeBarInterface {
        private GaugeMPConfig Config;

        protected float Value;
        protected string TextValue;

        public GaugeMPTracker(GaugeMPConfig config, int idx) {
            Config = config;
            LoadUI(Config.TypeConfig switch {
                GaugeBarConfig _ => new GaugeBar<GaugeMPTracker>(this, idx),
                _ => null
            });
        }

        public override GaugeConfig GetConfig() => Config;

        public override bool GetActive() => Value < 1f;

        public override void ProcessAction(Item action) { }

        protected override void TickTracker() {
            var mp = JobBars.ClientState.LocalPlayer.CurrentMp;
            Value = mp / 10000f;
            TextValue = ((int)(mp / 100)).ToString();
        }

        public virtual float[] GetBarSegments() => Config.ShowSegments ? Config.Segments : null;

        public virtual bool GetBarTextVisible() => Config.TypeConfig switch {
            GaugeBarConfig barConfig => barConfig.ShowText,
            _ => false
        };

        public virtual bool GetBarTextSwap() => Config.TypeConfig switch {
            GaugeBarConfig barConfig => barConfig.SwapText,
            _ => false
        };

        public virtual bool GetVertical() => Config.TypeConfig switch {
            GaugeBarConfig barConfig => barConfig.Vertical,
            _ => false
        };

        public virtual ElementColor GetColor() => Config.Color;

        public virtual bool GetBarDanger() => false;

        public virtual string GetBarText() => TextValue;

        public virtual float GetBarPercent() => Value;
    }
}