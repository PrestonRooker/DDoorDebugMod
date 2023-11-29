using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DDoorDebug.Model;

namespace DDoorDebug.GUIMenus
{
    public static class BindMenu
    {
        public static bool hasInit = false;
        public static GUIBox.GUIBox bindMenu;

        public static Feature[] featureBoxes;

        public static void init()
        {
            hasInit = true;
            var perColumn = Mathf.Floor(DDoorDebugPlugin.features.Length / 3);
            var leftOver = DDoorDebugPlugin.features.Length - 3 * perColumn;

            int indexTracker = 0;
            List<Feature> tmpFeatureBoxes = new List<Feature>();
            List<GUIBox.OptionCategory> columns = new List<GUIBox.OptionCategory>();
            for (var a = 0; a < 3; a++)
            {
                List<GUIBox.HorizontalOptionCategory> optionsInColumn = new List<GUIBox.HorizontalOptionCategory>();
                for (var b = 0; b < perColumn; b++)
                {
                    var f = DDoorDebugPlugin.features[indexTracker];
                    var nf = new Feature(f[0], "", f[2], f[3] == "t");
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
                var f = DDoorDebugPlugin.features[indexTracker];
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
                var b = (Bind)DDoorDebugPlugin.featureBinds[DDoorDebugPlugin.features[i][0]];
                featureBoxes[i].bindButton.SetText(b.keycode.ToString());
            }
        }

        public static void OnGUI()
        {
            if (!hasInit) { init(); }
            if (bindMenu == null) { DDoorDebugPlugin.Log.LogWarning("bind null!"); return; }
            bindMenu.OnGUI();
            for (var i = 0; i < featureBoxes.Length; i++)
            {
                if (featureBoxes[i] == null) { DDoorDebugPlugin.Log.LogWarning("null!"); continue; }
                var b = (Bind)DDoorDebugPlugin.featureBinds[DDoorDebugPlugin.features[i][0]];
                featureBoxes[i].OnGUI(b);
            }
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
                DDoorDebugPlugin.bufferedActions.Add(name);
            }

            var bindButtonPressed = bindButton.IsPressed();
            if (DDoorDebugPlugin.listeningForKey.Length == 0 && bindButtonPressed)
            {
                DDoorDebugPlugin.listeningForKey = name;
            }
            else if (DDoorDebugPlugin.listeningForKey.Length != 0 && DDoorDebugPlugin.listeningForKey != name) { bindButton.SetText(b.keycode.ToString()); }
            if (DDoorDebugPlugin.listeningForKey == name && DDoorDebugPlugin.foundKey == KeyCode.None && Input.GetKey(KeyCode.Escape) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                b.keycode = KeyCode.None;
                b.keyEntry.BoxedValue = KeyCode.None.ToString();
                DDoorDebugPlugin.listeningForKey = "";
                DDoorDebugPlugin.foundKey = KeyCode.None;
            }
            else if (DDoorDebugPlugin.listeningForKey == name && DDoorDebugPlugin.foundKey == KeyCode.None)
            {
                bindButton.SetText("Waiting...");
            }
            if (DDoorDebugPlugin.listeningForKey == name && DDoorDebugPlugin.foundKey != KeyCode.None)
            {
                b.keycode = DDoorDebugPlugin.foundKey;
                b.keyEntry.BoxedValue = DDoorDebugPlugin.foundKey.ToString();
                bindButton.SetText(DDoorDebugPlugin.foundKey.ToString());
                DDoorDebugPlugin.listeningForKey = "";
                DDoorDebugPlugin.foundKey = KeyCode.None;
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
                    b.extraEntry.BoxedValue = "t";
                }
                else
                {
                    b.extraEntry.BoxedValue = "f";
                }
            }

            DDoorDebugPlugin.featureBinds[name] = b;
        }
    }
}
