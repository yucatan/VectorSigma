/*
    The original code was written in Shell Script (SCbinds.sh). Since I am not a programmer,
    I asked ChatGPT 3.5 to "port" it to C#. After a few adjustments... here we are.
    Yep, I guess it is not pretty/elegant, but it does what I needed.

    (2024/06/17)                                                    Yucatan "Kenjiro" Costa
*/
using System;
using System.IO;

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
        string tempFile1 = @"Sounds\Sigma-SC\keybinds\customtmp.txt";
        string tempFile2 = @"Sounds\Sigma-SC\keybinds\customtmp2.txt";
        string tempFile3 = @"Sounds\Sigma-SC\keybinds\customtmp3.txt";

        // Default Keybinds
        ProcessDefaultKeybinds(defaultProfile, keyMapFile, defaultOutput);

        // Custom Keybinds
        ProcessCustomKeybinds(customProfilePath, keyMapFile, customOutput, tempFile1, tempFile2, tempFile3);
    }

    private void ProcessDefaultKeybinds(string defaultProfile, string keyMapFile, string defaultOutput)
    {
        using (StreamReader sr = new StreamReader(defaultProfile))
        // In case you want to write the output to a file, uncomment lines 42 and 56
        //using (StreamWriter sw = new StreamWriter(defaultOutput, false))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains("action name=") && line.Contains("keyboard="))
                {
                    string keyBind = ExtractValue(line, "action name=\"", "\"");
                    string keyValue = ExtractValue(line, "keyboard=\"", "\"");

                    if (!string.IsNullOrWhiteSpace(keyValue))
                    {
                        string keyCode = GetKeyCode(keyMapFile, keyValue);
                         VA.SetText(keyBind, keyCode);
                        //sw.WriteLine($"{keyBind}={keyCode}");
                    }
                }
            }
        }
    }

    private void ProcessCustomKeybinds(string customProfile, string keyMapFile, string customOutput, string tempFile1, string tempFile2, string tempFile3)
    {
        // Step 1
        using (StreamReader sr = new StreamReader(customProfile))
        using (StreamWriter sw = new StreamWriter(tempFile1, false))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains("action name") || line.Contains("rebind input"))
                {
                    line = line.Replace("<", "").Replace("/>", "").Replace(">", "").Replace("  action", "\naction").Replace("\r", "");
                    sw.WriteLine(line);
                }
            }
        }

        // Step 2
        using (StreamReader sr = new StreamReader(tempFile1))
        using (StreamWriter sw = new StreamWriter(tempFile2, false))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sw.Write($"{line};");
                }
                else
                {
                    sw.WriteLine();
                }
            }
        }

        // Step 3
        using (StreamReader sr = new StreamReader(tempFile2))
        using (StreamWriter sw = new StreamWriter(tempFile3, false))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains("kb"))
                {
                    sw.WriteLine(line);
                }
            }
        }

        using (StreamReader sr = new StreamReader(tempFile3))
        // In case you want to write the output to a file, uncomment lines 114 and 126
        //using (StreamWriter sw = new StreamWriter(customOutput, false))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                string keyBind = ExtractValue(line, "action name=\"", "\"");
                string keyValue = ExtractValue(line, "rebind input=\"", "\"").Replace("kb1_", "");

                if (!string.IsNullOrWhiteSpace(keyValue))
                {
                    string keyCode = GetKeyCode(keyMapFile, keyValue);
                    VA.SetText(keyBind, keyCode);
                    //sw.WriteLine($"{keyBind}={keyCode}");
                }
            }
        }

        // Cleanup temporary files
        File.Delete(tempFile1);
        File.Delete(tempFile2);
        File.Delete(tempFile3);
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
