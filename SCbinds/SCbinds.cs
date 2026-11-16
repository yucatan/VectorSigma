/*
    The original code was written in Shell Script (SCbinds.sh). Since I am not a programmer,
    I asked ChatGPT 3.5 to "port" it to C#. After a few adjustments I got  SCbinds-1.1
    But since I wasn't too happy about the code (it still used 3 temporary files), I asked Blackbox
    to remove the need of those temporary files... and here we are.
    

    (2024/06/24)                                                    Yucatan "Kenjiro" Costa
*/
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

public class VAInline
{
    public void main()
    {
        // Retrieve the variable from VoiceAttack (using VA instead of vaProxy)
        string customProfilePath = VA.GetText("CUSTOMKEYBINDS");

        if (string.IsNullOrEmpty(customProfilePath))
        {
            VA.WriteToLog("The 'CUSTOMKEYBINDS' variable is not set.", "red");
            return;
        }

        if (!File.Exists(customProfilePath))
        {
            VA.WriteToLog("Custom keybinds file not found: " + customProfilePath, "red");
            return;
        }

        ProcessKeybindings(customProfilePath);
    }
    
        public void ProcessKeybindings(string customProfilePath)
    {
        string keyMapFile = @"Sounds\Sigma-SC\keybinds\keyboard-map-en-us.txt";
        string defaultProfile = @"Sounds\Sigma-SC\keybinds\defaultProfile.xml";

        // Load the key map into memory once instead of reading the file hundreds of times
        Dictionary<string, string> keyMap = LoadKeyMap(keyMapFile);

        if (keyMap.Count == 0)
        {
            VA.WriteToLog("Key map file missing or empty: " + keyMapFile, "red");
            return;
        }

        int defaultCount = 0;
        int customCount = 0;

        // 1. Process Default Keybinds
        if (File.Exists(defaultProfile))
        {
            defaultCount = ProcessDefaultKeybinds(defaultProfile, keyMap);
        }
        else
        {
            VA.WriteToLog("Default profile not found: " + defaultProfile, "orange");
        }

        // 2. Process Custom Keybinds (overwrites defaults where customized)
        customCount = ProcessCustomKeybinds(customProfilePath, keyMap);

        VA.WriteToLog(string.Format("Loaded {0} default and {1} custom keybinds.", defaultCount, customCount), "green");
    }
    
        private Dictionary<string, string> LoadKeyMap(string keyMapFile)
    {
        Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(keyMapFile)) return map;

        using (StreamReader sr = new StreamReader(keyMapFile))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.StartsWith("key_") && line.Contains(";"))
                {
                    string[] parts = line.Split(';');
                    // Strip "key_" prefix so we can look up directly by SC key name
                    string scKey = parts[0].Substring(4).Trim();
                    string vaKey = parts[1].Trim();
                    if (!map.ContainsKey(scKey))
                    {
                        map.Add(scKey, vaKey);
                    }
                }
            }
        }
        return map;
    }
    
        private int ProcessDefaultKeybinds(string defaultProfile, Dictionary<string, string> keyMap)
    {
        int count = 0;
        using (StreamReader sr = new StreamReader(defaultProfile))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains("action name=") && line.Contains("keyboard="))
                {
                    string keyBind = ExtractValue(line, "action name=\"", "\"");
                    string keyValue = ExtractValue(line, "keyboard=\"", "\"");

                    if (!string.IsNullOrWhiteSpace(keyBind) && !string.IsNullOrWhiteSpace(keyValue))
                    {
                        string keyCode = FormatKeyCode(keyMap, keyValue);
                        if (!string.IsNullOrEmpty(keyCode))
                        {
                            VA.SetText(keyBind, keyCode);
                            count++;
                        }
                    }
                }
            }
        }
        return count;
    }
    
        private int ProcessCustomKeybinds(string customProfile, Dictionary<string, string> keyMap)
    {
        int count = 0;
        string currentAction = string.Empty;

        using (StreamReader sr = new StreamReader(customProfile))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                // Track the current <action name="..."> block
                if (line.Contains("action name="))
                {
                    currentAction = ExtractValue(line, "action name=\"", "\"");
                }

                // Pair the <rebind input="kb1_..."> with the active action
                if (!string.IsNullOrEmpty(currentAction) && line.Contains("rebind input=\"kb1_"))
                {
                    string rawInput = ExtractValue(line, "rebind input=\"", "\"");
                    string keyValue = rawInput.Replace("kb1_", "").Trim();

                    if (!string.IsNullOrWhiteSpace(keyValue))
                    {
                        string keyCode = FormatKeyCode(keyMap, keyValue);
                        if (!string.IsNullOrEmpty(keyCode))
                        {
                            VA.SetText(currentAction, keyCode);
                            count++;
                        }
                    }
                }
            }
        }
        return count;
    }
    
        private string ExtractValue(string line, string startDelimiter, string endDelimiter)
    {
        int startIndex = line.IndexOf(startDelimiter);
        if (startIndex >= 0)
        {
            startIndex += startDelimiter.Length;
            int endIndex = line.IndexOf(endDelimiter, startIndex);
            if (endIndex > startIndex)
            {
                return line.Substring(startIndex, endIndex - startIndex);
            }
        }
        return string.Empty;
    }

    private string FormatKeyCode(Dictionary<string, string> keyMap, string keyValue)
    {
        // Ignore mouse and HMD inputs
        if (keyValue.Contains("mouse") || keyValue.Contains("mwheel") || keyValue.Contains("HMD_"))
        {
            return string.Empty;
        }

        if (keyValue.Contains("+"))
        {
            string[] parts = keyValue.Split('+');
            string keyCode1 = LookupKey(keyMap, parts[0]);
            string keyCode2 = LookupKey(keyMap, parts[1]);

            if (string.IsNullOrEmpty(keyCode1) && string.IsNullOrEmpty(keyCode2))
                return string.Empty;
            if (string.IsNullOrEmpty(keyCode2))
                return "[" + keyCode1 + "]";
            if (string.IsNullOrEmpty(keyCode1))
                return "[" + keyCode2 + "]";

            // VoiceAttack expects [MOD][KEY] without a '+' sign between brackets
            return "[" + keyCode1 + "][" + keyCode2 + "]";
        }
        else
        {
            string singleKey = LookupKey(keyMap, keyValue);
            return string.IsNullOrEmpty(singleKey) ? string.Empty : "[" + singleKey + "]";
        }
    }
    
        private string LookupKey(Dictionary<string, string> keyMap, string scKey)
    {
        string result;
        if (keyMap.TryGetValue(scKey.Trim(), out result))
        {
            return result;
        }
        return string.Empty;
    }
}