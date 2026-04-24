using Internal.Scripts.UI.Localization;
using Internal.Scripts.UI.Localization.Generated;

namespace Internal.Scripts.UI.Input
{
    public static class InputDisplayLocalizer
    {
        public static string Localize(string display) => display switch
        {
            "LMB" or "Left Button" or "Mouse Left"           => Res(LocUI.UI_Input_Key_LMB,        "LMB"),
            "RMB" or "Right Button"                          => Res(LocUI.UI_Input_Key_RMB,        "RMB"),
            "MMB" or "Middle Button"                         => Res(LocUI.UI_Input_Key_MMB,        "MMB"),
            "Scroll Up"   or "Scroll/Up"   or "Mouse ScrollWheel Up"   => Res(LocUI.UI_Input_Key_ScrollUp,   "Scroll ↑"),
            "Scroll Down" or "Scroll/Down" or "Mouse ScrollWheel Down" => Res(LocUI.UI_Input_Key_ScrollDown, "Scroll ↓"),
            "Space"                                          => Res(LocUI.UI_Input_Key_Space,      "Space"),
            "Up Arrow"   or "Up"                             => Res(LocUI.UI_Input_Key_ArrowUp,    "↑"),
            "Down Arrow" or "Down"                           => Res(LocUI.UI_Input_Key_ArrowDown,  "↓"),
            "Left Arrow" or "Left"                           => Res(LocUI.UI_Input_Key_ArrowLeft,  "←"),
            "Right Arrow" or "Right"                         => Res(LocUI.UI_Input_Key_ArrowRight, "→"),
            "Left Stick"                                     => Res(LocUI.UI_Input_Key_LeftStick,  "Left Stick"),
            "Right Stick"                                    => Res(LocUI.UI_Input_Key_RightStick, "Right Stick"),
            _                                                => display,
        };

        private static string Res(string key, string fallback) =>
            LocalizationService.Resolve(LocUI.Table, key, fallback);
    }
}
