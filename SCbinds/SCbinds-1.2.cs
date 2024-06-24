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
        // Retrieve the variable from VoiceAttack
        string customProfilePath = vaProxy.GetText("CUSTOMKEYBINDS");

        // Check if the variable is set
        if (string.IsNullOrEmpty(customProfilePath))
        {
            vaProxy.WriteToLog("The 'CUSTOMKEYBINDS' variable is not set.", "red");
            return;
        }

        ProcessKeybindings(customProfilePath);
    }

    public void ProcessKeybindings(string customProfilePath)
    {
        string keyMapFile = @"Sounds\Sigma-SC\keybinds\keyboard-map-en-us.txt";
        string defaultProfile = @"Sounds\Sigma-SC\keybinds\defaultProfile.xml";        
        string defaultOutput = @"Sounds\Sigma-SC\keybinds\default-keybinds.txt";
        string customOutput = @"Sounds\Sigma-SC\keybinds\custom-keybinds.txt";

        // Default Keybinds
        ProcessDefaultKeybinds(defaultProfile, keyMapFile, defaultOutput);

        // Custom Keybinds
        ProcessCustomKeybinds(customProfilePath, keyMapFile, customOutput);
    }

    private void ProcessDefaultKeybinds(string defaultProfile, string keyMapFile, string defaultOutput)
    {
        using (StreamReader sr = new StreamReader(defaultProfile))
        {
            string line;
            while ((line = sr.ReadLine())!= null)
            {
                if (line.Contains("action name=") && line.Contains("keyboard="))
                {
                    string keyBind = ExtractValue(line, "action name=\"", "\"");
                    string keyValue = ExtractValue(line, "keyboard=\"", "\"");

                    if (!string.IsNullOrWhiteSpace(keyValue))
                    {
                        string keyCode = GetKeyCode(keyMapFile, keyValue);
                        VA.SetText(keyBind, keyCode);
                    }
                }
            }
        }
    }

    private void ProcessCustomKeybinds(string customProfile, string keyMapFile, string customOutput)
    {
        List<string> lines = new List<string>();

        using (StreamReader sr = new StreamReader(customProfile))
        {
            string line;
            while ((line = sr.ReadLine())!= null)
            {
                if (line.Contains("action name") || line.Contains("rebind input"))
                {
                    line = line.Replace("<", "").Replace("/>", "").Replace(">", "").Replace("  action", "\naction").Replace("\r", "");
                    lines.Add(line);
                }
            }
        }

        StringBuilder sb = new StringBuilder();
        foreach (string line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                sb.Append(line).Append(';');
            }
            else
            {
                sb.AppendLine();
            }
        }

        lines.Clear();
        foreach (string line in sb.ToString().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Contains("kb"))
            {
                lines.Add(line);
            }
        }

        foreach (string line in lines)
        {
            string keyBind = ExtractValue(line, "action name=\"", "\"");
            string keyValue = ExtractValue(line, "rebind input=\"", "\"").Replace("kb1_", "");

            if (!string.IsNullOrWhiteSpace(keyValue))
            {
                string keyCode = GetKeyCode(keyMapFile, keyValue);
                VA.SetText(keyBind, keyCode);
            }
        }
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

    private string GetKeyCode(string keyMapFile, string keyValue)
    {
        string keyCode = string.Empty;

        if (keyValue.Contains("+"))
        {
            string[] parts = keyValue.Split('+');
            string keyCode1 = GetKeyCodeFromMap(keyMapFile, parts[0]);
            string keyCode2 = GetKeyCodeFromMap(keyMapFile, parts[1]);
            if (keyCode2 == "")
            {
                keyCode = keyCode1;
            }
            else
            {
                keyCode = $"[{keyCode1}]+[{keyCode2}]";
            }            
        }
        else
        {
            keyCode = $"[{GetKeyCodeFromMap(keyMapFile, keyValue)}]";
        }

        return keyCode;
    }
 
    
    private string GetKeyCodeFromMap(string keyMapFile, string key)
    {
        string keyCode = string.Empty;

        using (StreamReader sr = new StreamReader(keyMapFile))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (key.Contains($"mouse1") || key.Contains($"mouse1") || key.Contains($"mouse2") || key.Contains($"mouse3") || key.Contains($"mwheel_up") || key.Contains($"mwheel_down") || key.Contains($"HMD_Pitch") || key.Contains($"HMD_Yaw") || key.Contains($"HMD_Roll"))
                {
                    keyCode = "";
                }
                else if (line.Contains($"key_{key};"))
                {
                    keyCode = line.Split(';')[1]; // Accessing the second element directly
                    break;
                }
            }
        }

        return keyCode;
    }
}
