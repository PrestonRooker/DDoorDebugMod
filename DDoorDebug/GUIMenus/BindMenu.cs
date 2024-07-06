using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DDoorDebug.Model;
using System.Collections;
using BepInEx;
using BepInEx.Configuration;

namespace DDoorDebug.GUIMenus
{
    public static class BindMenu
    {
        public static List<String>[] features = new List<string>[] // { "name in config file", "default bind", "default modifiers", "allow extra modifiers" (t/f) }
   {
            new List<string>() { "Open binding menu", "Tab", "", "t" },
            new List<string>() { "Info menu", "F1", "", "t" },
            new List<string>() { "Show hp", "F2", "", "t" },
            new List<string>() { "Warp menu", "F3", "", "f" },
            new List<string>() { "Warp to selected", "F3", "c", "t" },
            new List<string>() { "Heal to full", "F4", "", "f" },
            new List<string>() { "Auto heal", "", "", "t" },
            new List<string>() { "Inf magic", "F4", "c", "f" },
            new List<string>() { "Toggle godmode", "F4", "s", "f" },
            new List<string>() { "Boss reset", "F5", "", "t" },
            new List<string>() { "Boss reset with cuts", "F5", "s", "t" },
            new List<string>() { "Give soul", "F6", "", "t" },
            new List<string>() { "Unlock weapons", "F7", "", "t" },
            new List<string>() { "Unlock spells", "F7", "s", "t" },
            new List<string>() { "Save pos", "F8", "", "f" },
            new List<string>() { "Load pos", "F9", "", "f" },
            new List<string>() { "Save gpos", "F8", "s", "t" },
            new List<string>() { "Load gpos", "F9", "s", "t" },
            new List<string>() { "Show colliders", "F10", "", "f" },
            new List<string>() { "Load visible colliders", "F10", "c", "t" },
            new List<string>() { "Freecam", "F11", "", "t" },
            new List<string>() { "Pos history", "P", "", "t" },
            new List<string>() { "Velocity graph", "Backspace", "", "t" },
            new List<string>() { "Timescale down", "Insert", "", "t" },
            new List<string>() { "Timescale up", "PageUp", "", "t" },
            new List<string>() { "Reset timescale", "Home", "", "t" },
            new List<string>() { "Rotate cam right", "Delete", "", "t" },
            new List<string>() { "Rotate cam left", "PageDown", "", "t" },
            new List<string>() { "Reset cam", "End", "", "t" },
            new List<string>() { "Mouse tele", "Mouse0", "c", "t" },
            new List<string>() { "Zoom in", "Minus", "", "t" },
            new List<string>() { "Zoom out", "Equals", "", "t" },
            new List<string>() { "Toggle noclip", "U", "", "t" },
            new List<string>() { "Tele up", "H", "", "t" },
            new List<string>() { "Tele down", "J", "", "t" },
            new List<string>() { "Toggle night", "", "", "t" },
            new List<string>() { "Save file", "S", "c", "t" },
            new List<string>() { "Reload file", "O", "c", "t" },
            new List<string>() { "Get gp", "", "", "t" },
            new List<string>() { "Instant textskip", "", "", "t" },
   };

        public static Hashtable featureBinds = new Hashtable(); // { "name in config file", "bind" }
        public static bool bindMenuOpen = false;
        public static List<String> bufferedActions = new List<String>();

        public static bool hasInit = false;
        public static GUIBox.GUIBox bindMenu;

        public static Feature[] featureBoxes;

        public static String listeningForKey = "";
        public static KeyCode foundKey;

        public static void init(ConfigFile Config)
        {
            hasInit = true;
            foreach (var feature in GUIMenus.BindMenu.features)
            {
                GUIMenus.BindMenu.featureBinds.Add(feature[0], new Bind(
                            Config.Bind(feature[0], "key/button", feature[1]),
                            Config.Bind(feature[0], "modifiers", feature[2]),
                            Config.Bind(feature[0], "allow extra modifiers", feature[3]))
                        );
            }

            var perColumn = Mathf.Floor(features.Length / 3);
            var leftOver = features.Length - 3 * perColumn;

            int indexTracker = 0;
            List<Feature> tmpFeatureBoxes = new List<Feature>();
            List<GUIBox.OptionCategory> columns = new List<GUIBox.OptionCategory>();
            for (var a = 0; a < 3; a++)
            {
                List<GUIBox.HorizontalOptionCategory> optionsInColumn = new List<GUIBox.HorizontalOptionCategory>();
                for (var b = 0; b < perColumn; b++)
                {
                    var f = features[indexTracker];
                    Bind fb = (Bind)featureBinds[f[0]];
                    var nf = new Feature(f[0], fb.keycode.ToString(), fb.modifiers, fb.allowExtraModifiers);
                    tmpFeatureBoxes.Add(nf);
                    optionsInColumn.Add(nf.box);
                    indexTracker++;
                }
                columns.Add(new GUIBox.OptionCategory(subCategories: optionsInColumn.ToArray()));
            }
            var cBox = new GUIBox.HorizontalOptionCategory(subCategories: columns.ToArray(), gapBetweenThings: 0.005f);

            var leftOverHolder = new List<GUIBox.HorizontalOptionCategory>();
            for (var i = 0; i < leftOver; i++)
            {
                var f = features[indexTracker];
                var nf = new Feature(f[0], f[1], f[2], f[3] == "t");
                tmpFeatureBoxes.Add(nf);
                leftOverHolder.Add(nf.box);
                indexTracker++;
            }

            var leftoverCat = new GUIBox.HorizontalOptionCategory(subCategories: leftOverHolder.ToArray(), gapBetweenThings: 0.005f);
            cBox.OnGUI(new Vector2());
            leftoverCat.OnGUI(new Vector2());
            var bigSize = cBox.CalcSize();
            var leftoverSize = leftoverCat.CalcSize();
            var empty = new GUIBox.OptionCategory(options: new GUIBox.BaseOption[] { new GUIBox.EmptyOption((bigSize.x - leftoverSize.x) / 2 / Screen.width, 0)}, gapBetweenThings:0);
            var bottomCat = new GUIBox.HorizontalOptionCategory(subCategories: new GUIBox.OptionCategory[] {empty, leftoverCat}, gapBetweenThings:0);
            
            bindMenu = new GUIBox.GUIBox(new Vector2(0.02f, 0.02f), new GUIBox.OptionCategory(subCategories: new GUIBox.OptionCategory[] { cBox, bottomCat }));
            
            featureBoxes = tmpFeatureBoxes.ToArray();

            for (var i = 0; i < featureBoxes.Length; i++)
            {
                if (featureBoxes[i] == null) { DDoorDebugPlugin.Log.LogWarning("null!"); continue; }
                var b = (Bind)featureBinds[features[i][0]];
                featureBoxes[i].bindButton.SetText(b.keycode.ToString());
            }
        }

        public static void OnGUI()
        {
            if (bindMenu == null) { DDoorDebugPlugin.Log.LogWarning("bind null!"); return; }
            bindMenu.OnGUI();
            for (var i = 0; i < featureBoxes.Length; i++)
            {
                if (featureBoxes[i] == null) { DDoorDebugPlugin.Log.LogWarning("null!"); continue; }
                var b = (Bind)featureBinds[features[i][0]];
                featureBoxes[i].OnGUI(b);
            }
        }

        public static bool CheckIfPressed(String name)
        {
            if (bufferedActions.Contains(name)) { bufferedActions.Remove(name); return true; }
            var raw = featureBinds[name];
            if (!(raw.GetType() == typeof(Bind)))
            {
                return false;
            }
            var b = (Bind)raw;
            if (!b.allowExtraModifiers) { return CheckIfPressedNoExtras(b); }
            if (!Input.GetKeyDown(b.keycode)) { return false; }
            if (b.modifiers != "")
            {
                if (b.modifiers.Contains('s') && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) { return false; }
                if (b.modifiers.Contains('c') && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) { return false; }
                if (b.modifiers.Contains('a') && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) { return false; }
            }
            return true;
        }

        public static bool CheckIfPressedNoExtras(Bind b)
        {
            return Input.GetKeyDown(b.keycode) && !(b.modifiers.Contains('s') ^ (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) && !(b.modifiers.Contains('c') ^ (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) && !(b.modifiers.Contains('a') ^ (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)));
        }

        public static void listenForKeys()
        {
            if (!Input.anyKeyDown || listeningForKey.Length == 0) { return; }
            foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
            {
                if (k != KeyCode.None && Input.GetKeyDown(k))
                {
                    foundKey = k;
                    return;
                }
            }
        }

        public static bool CheckIfHeld(String name)
        {
            if (bufferedActions.Contains(name)) { bufferedActions.Remove(name); return true; }
            var raw = featureBinds[name];
            if (!(raw.GetType() == typeof(Bind)))
            {
                return false;
            }
            var b = (Bind)raw;
            var result = Input.GetKey(b.keycode);
            if (b.modifiers != "")
            {
                if (b.modifiers.Contains('s') && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) { result = false; }
                if (b.modifiers.Contains('c') && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) { result = false; }
                if (b.modifiers.Contains('a') && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) { result = false; }
            }
            return result;
        }

        public static bool CheckIfModifierHeld(String name)
        {
            var raw = featureBinds[name];
            if (!(raw.GetType() == typeof(Bind)))
            {
                return false;
            }
            var b = (Bind)raw;
            return (!b.modifiers.Contains("s") || (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) && (!b.modifiers.Contains("c") || (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) && (!b.modifiers.Contains("a") || (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)));
        }
    }

    public class Feature
    {
        public string name;

        public GUIBox.HorizontalOptionCategory box;

        public GUIBox.ButtonOption actionButton;
        public GUIBox.ButtonOption bindButton;
        public GUIBox.ToggleOption shiftMod;
        public GUIBox.ToggleOption ctrlMod;
        public GUIBox.ToggleOption altMod;
        public GUIBox.ToggleOption extraButton;

        public int featureIndex;

        public Feature(string bindName, string currentBind, string modifiers, bool extraModifier)
        {
            name = bindName;

            actionButton = new GUIBox.ButtonOption(name, 2.5f, overrideWidth:0.12f);
            bindButton = new GUIBox.ButtonOption(currentBind, 2.5f, overrideWidth: 0.07f);
            shiftMod = new GUIBox.ToggleOption("s", modifiers.Contains("s"), true, 2.5f, overrideButtonWidth: 0.03f);
            ctrlMod = new GUIBox.ToggleOption("c", modifiers.Contains("c"), true, 2.5f, overrideButtonWidth: 0.03f);
            altMod = new GUIBox.ToggleOption("a", modifiers.Contains("a"), true, 2.5f, overrideButtonWidth: 0.03f);
            extraButton = new GUIBox.ToggleOption("Extra", extraModifier, true, 2.5f, overrideButtonWidth: 0.04f);

            box = new GUIBox.HorizontalOptionCategory(options: new GUIBox.BaseOption[] { actionButton, bindButton, shiftMod, ctrlMod, altMod, extraButton }, gapBetweenThings:0.003f);
        }

        public void OnGUI(Bind b)
        {
            if (actionButton.IsPressed())
            {
                BindMenu.bufferedActions.Add(name);
            }

            var bindButtonPressed = bindButton.IsPressed();
            if (BindMenu.listeningForKey.Length == 0 && bindButtonPressed)
            {
                BindMenu.listeningForKey = name;
            }
            else if (BindMenu.listeningForKey.Length != 0 && BindMenu.listeningForKey != name) { bindButton.SetText(b.keycode.ToString()); }
            if (BindMenu.listeningForKey == name && BindMenu.foundKey == KeyCode.None && Input.GetKey(KeyCode.Escape) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                b.keycode = KeyCode.None;
                b.keyEntry.BoxedValue = KeyCode.None.ToString();
                BindMenu.listeningForKey = "";
                BindMenu.foundKey = KeyCode.None;
            }
            else if (BindMenu.listeningForKey == name && BindMenu.foundKey == KeyCode.None)
            {
                bindButton.SetText("Waiting...");
            }
            if (BindMenu.listeningForKey == name && BindMenu.foundKey != KeyCode.None)
            {
                b.keycode = BindMenu.foundKey;
                b.keyEntry.BoxedValue = BindMenu.foundKey.ToString();
                bindButton.SetText(BindMenu.foundKey.ToString());
                BindMenu.listeningForKey = "";
                BindMenu.foundKey = KeyCode.None;
            }

            var modifiers = b.modifiers;
            var oldState = modifiers.Contains('s');
            var newState = shiftMod.GetState();
            if (oldState != newState)
            {
                string n = modifiers;
                if (oldState)
                {
                    n = n.Replace("s", String.Empty);
                }
                else
                {
                    n += "s";
                }
                b.modifiers = n;
                b.modEntry.BoxedValue = n;
            }

            oldState = modifiers.Contains('c');
            newState = ctrlMod.GetState();
            if (oldState != newState)
            {
                string n = modifiers;
                if (oldState)
                {
                    n = n.Replace("c", String.Empty);
                }
                else
                {
                    n += "c";
                }
                b.modifiers = n;
                b.modEntry.BoxedValue = n;
            }

            oldState = modifiers.Contains('a');
            newState = altMod.GetState();
            if (oldState != newState)
            {
                string n = modifiers;
                if (oldState)
                {
                    n = n.Replace("a", String.Empty);
                }
                else
                {
                    n += "a";
                }
                b.modifiers = n;
                b.modEntry.BoxedValue = n;
            }

            oldState = b.allowExtraModifiers;
            newState = extraButton.GetState();
            if (oldState != newState)
            {
                b.allowExtraModifiers = !b.allowExtraModifiers;
                if (newState)
                {
                    b.allowExtraModifiers = true;
                    b.extraEntry.BoxedValue = "t";
                }
                else
                {
                    b.allowExtraModifiers = false;
                    b.extraEntry.BoxedValue = "f";
                }
            }

            BindMenu.featureBinds[name] = b;
        }
    }
}
