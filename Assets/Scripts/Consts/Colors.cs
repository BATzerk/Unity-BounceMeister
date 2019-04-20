using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class Colors {
    // Themes!
    private const int TestingGreen = 0;
    private const int BasicGreen = 1;
    private const int MyYachtBlue = 2;
    private const int MauveStorm = 3;
    private const int MrKakhi = 4;
    private const int Hungaria = 5;
    private const int GrayClicks = 6;
    private const int NoirDark = 7;
    private const int NoirLight = 8;
    
    static private int GetTheme(int worldIndex) {
        switch (worldIndex) {
            //case 2: return MauveStorm;
            case 0: return TestingGreen;
            case 1: return BasicGreen;
            case 2: return MyYachtBlue;
            case 3: return MrKakhi;
            case 4: return Hungaria;
            case 5: return GrayClicks;
            case 6: return NoirDark;
            case 7: return NoirLight;
            default: return TestingGreen;
        }
    }
    
    // Getters!
    /// The color of Ground, BEFORE additional isBouncy/doesn't-recharge-Plunge/etc. coloring is applied (that happens in Ground itself).
    static public Color GroundBaseColor(int worldIndex) {
        int theme = GetTheme(worldIndex);
        //switch (worldIndex) {
        //    case 1: color = isBouncy ? new ColorHSB(190/360f, 0.73f, 0.83f).ToColor() : new ColorHSB(190/360f, 0.24f, 0.57f).ToColor(); break; // blue
        //    case 2: color = isBouncy ? new ColorHSB(280/360f, 0.76f, 0.83f).ToColor() : new ColorHSB(280/360f, 0.35f, 0.45f).ToColor(); break; // purple
        //    default: color = isBouncy ? new ColorHSB(76/360f, 0.84f, 0.83f).ToColor() : new ColorHSB(85/360f, 0.37f, 0.42f).ToColor(); break; // green
        //}
        switch (theme) {
            case TestingGreen:  return new Color255( 83,104, 73).ToColor();
            case BasicGreen:    return new Color255( 84,101, 76).ToColor();
            case MyYachtBlue:   return new Color255( 53, 74, 72).ToColor();
            case MauveStorm:    return new Color255( 81, 63, 96).ToColor();
            case MrKakhi:       return new Color255(134,132,102).ToColor();
            case Hungaria:      return new Color255( 67, 95, 81).ToColor();
            case GrayClicks:    return new Color255( 88, 88, 88).ToColor();
            case NoirDark:      return new Color255(  2,  2,  2).ToColor();
            case NoirLight:     return new Color255(253,253,253).ToColor();
            default:            return new Color255( 84,101, 76).ToColor();
        }
    }
    
    static public Color Spikes(int worldIndex) {
        int theme = GetTheme(worldIndex);
        switch (theme) {
            case MauveStorm:    return new Color255( 62,  6, 42).ToColor();
            case MrKakhi:       return new Color255( 67, 52, 26).ToColor();
            case Hungaria:      return new Color255(104,  0, 21).ToColor();
            case GrayClicks:    return new Color255( 80, 38, 31).ToColor();
            case NoirDark:      return new Color255(253,253,253).ToColor();
            case NoirLight:     return new Color255(  2,  2,  2).ToColor();
            default:            return new Color255( 67, 27, 26).ToColor();
        }
    }
    
    
    /// First color is BOTTOM, second is TOP.
    static public List<Color255> GameBackgroundGradient(int worldIndex) {
        int theme = GetTheme(worldIndex);
        switch (theme) {
            case TestingGreen:  return new List<Color255>{
                new Color255(136,190,213), new Color255(237,251,255) };
            case BasicGreen:    return new List<Color255>{
                new Color255(136,190,213), new Color255(237,251,255) };
            case MyYachtBlue:   return new List<Color255>{
                new Color255(130,219,215), new Color255(223,252,237) };
            case MauveStorm:    return new List<Color255>{
                new Color255(167,124,194), new Color255(190,221,255) };
            case MrKakhi:       return new List<Color255>{
                new Color255(213,210,136), new Color255(250,237,225) };
            case Hungaria:      return new List<Color255>{
                new Color255(164,219,152), new Color255(243,251,229) };
            case GrayClicks:    return new List<Color255>{
                new Color255(175,211,138), new Color255(248,249,225) };
            case NoirDark:      return new List<Color255>{
                new Color255( 37, 37, 37), new Color255( 37, 37, 37) };
            case NoirLight:     return new List<Color255>{
                new Color255(164,164,164), new Color255(164,164,164) };
            default:            return new List<Color255>{
                new Color255(136,190,213), new Color255(237,251,255) };
            //case 0:  return new List<Color255>{
            //    new Color255(242,251,139), new Color255(133,185,255) };
            //case 1:  return new List<Color255>{
            //    new Color255(255,174,166), new Color255(205,240,180) };
            //case 2:  return new List<Color255>{
            //    new Color255(133,255,176), new Color255(255,114,160) };
            //case 3:  return new List<Color255>{
            //    new Color255(255,213,133), new Color255(201,177,255) };
            //case 4:  return new List<Color255>{
            //    new Color255(255,166,193), new Color255(220,174,255) };
            //case 5:  return new List<Color255>{
            //    new Color255( 78, 14, 89), new Color255(138,202,191) };
            //default: return new List<Color255>{
                //new Color255(255,255,255), new Color255(128,128,128) };
        }
    }
}
