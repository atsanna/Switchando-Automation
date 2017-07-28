﻿using HomeAutomation.Objects.Switches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation.Objects.Lights
{
    public interface ILight : ISwitch
    {
        LightType GetLightType();
        string GetName();
        void Pause();
        void Pause(bool status);
        void Dimm(uint percentage, int dimmerIntervals);
    }
    public interface IColorableLight : ILight
    {
        void Set(uint ValueR, uint ValueG, uint ValueB, int DimmerIntervals);
        double[] GetValues();
    }
    public enum LightType
    {
        RGB_LIGHT,
        W_LIGHT,
        LIGHT_SWITCH,
    }
}
