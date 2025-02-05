﻿using Dalamud.Interface.Internal;
using ImGuiNET;
using System.Collections.Generic;
using System;
using System.Numerics;

namespace BossMod
{
    public static class UIMisc
    {
        // button that is disabled unless shift is held, useful for 'dangerous' operations like deletion
        public static bool DangerousButton(string label)
        {
            bool disabled = !ImGui.IsKeyDown(ImGuiKey.ModShift);
            ImGui.BeginDisabled(disabled);
            bool res = ImGui.Button(label);
            ImGui.EndDisabled();
            if (disabled && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                ImGui.SetTooltip("Hold shift");
            return res;
        }

        public static void TextUnderlined(Vector4 colour, string text)
        {
            var size = ImGui.CalcTextSize(text);
            var cur = ImGui.GetCursorScreenPos();
            cur.Y += size.Y;
            ImGui.GetWindowDrawList().PathLineTo(cur);
            cur.X += size.X;
            ImGui.GetWindowDrawList().PathLineTo(cur);
            ImGui.GetWindowDrawList().PathStroke(ImGui.ColorConvertFloat4ToU32(colour));
            ImGui.TextColored(colour, text);
        }

        public static void TextV(string s)
        {
            var cur = ImGui.GetCursorPos();
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0);
            ImGui.Button("");
            ImGui.PopStyleVar();
            ImGui.SameLine();
            ImGui.SetCursorPos(cur);
            ImGui.TextUnformatted(s);
        }

        public static bool ImageToggleButton(IDalamudTextureWrap icon, Vector2 size, bool state)
        {
            var tintColor = new Vector4(1f, 1f, 1f, 1f);
            if (!state)
            {
                tintColor = new Vector4(0.5f, 0.5f, 0.5f, 0.85f);
            }
            return ImGui.ImageButton(icon.ImGuiHandle, size, Vector2.Zero, Vector2.One, 1, Vector4.Zero, tintColor);
        }
    }
}
