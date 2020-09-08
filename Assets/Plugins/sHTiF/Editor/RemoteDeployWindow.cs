/*
 *	Created by Peter Stefcek. All rights reserved.
 */

using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

/*
 *    Unity editor window to setup and manage wifi adb connections. 
 */
public class RemoteDeployWindow : EditorWindow
{
    private string androidSdkPath = "C:/Program Files/Unity/Hub/Editor/2019.4.9f1/Editor/Data/PlaybackEngines/AndroidPlayer/SDK";
    private string applicationPath;
    private string port = "5555";
    private string remoteIp = "192.168.0.5";
    private string debug;
    private Vector2 scrollPos;
    private bool debugToConsole = false;

    private bool advancedGroup = false;
    private string wlan = "wlan0";
    
    [MenuItem("Tools/RemoteDeployWindow")]
    static void Init()
    {
        GetWindow<RemoteDeployWindow>().Show();
    }

    void OnGUI()
    {
        applicationPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
        
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        androidSdkPath = EditorGUILayout.TextField("Android SDK Path", androidSdkPath);
        if (GUILayout.Button("Fetch Editor Prefs", GUILayout.Width(120)))
        {
            androidSdkPath = EditorPrefs.GetString("AndroidSdkRoot");
        }
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFolderPanel("Android SDK Path", "", "");
            if (path.Length != 0)
                androidSdkPath = path;
        }
        GUILayout.EndHorizontal();
        
        remoteIp = EditorGUILayout.TextField("Remote IP", remoteIp);
        debugToConsole = EditorGUILayout.Toggle("Debug to Console", debugToConsole);

        GUILayout.Space(10);
        
        advancedGroup = EditorGUILayout.BeginToggleGroup("Advanced Settings", advancedGroup);
        port = EditorGUILayout.TextField("Port", port);
        wlan = EditorGUILayout.TextField("WLan", wlan);
        EditorGUILayout.EndToggleGroup();
        
        GUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Setup TCP Bridge"))
        {
            StartRemoteConnection();
        }
        
        if (GUILayout.Button("Fetch IP Address"))
        {
            FetchIPAddress();
        }
        
        if (GUILayout.Button("List Devices"))
        {
            Devices();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Connect"))
        {
            Connect();
        }
        
        if (GUILayout.Button("Disconnect"))
        {
            Disconnect();
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Start Monitor"))
        {
            StartMonitor();
        }

        GUI.enabled = false;
        if (GUILayout.Button("Start Logging"))
        {
            StartLogcat();
        }
        GUI.enabled = true;
        
        GUILayout.Space(10);
        
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUILayout.Label(debug);
        GUILayout.EndScrollView();

        if (GUILayout.Button("Clear"))
        {
            debug = "";
        }
    }

    void StartRemoteConnection()
    {
        debug+=("Starting TCPIP bridge...\n");
        var process = new Process();
        var startInfo = new ProcessStartInfo(androidSdkPath+"/platform-tools/adb.exe", "tcpip 5555");
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        process.StartInfo = startInfo;
        process.OutputDataReceived += (sender, argsx) => DebugData(argsx.Data);
        process.Start();
        process.BeginOutputReadLine();
        process.WaitForExit();
    }
    
    void StartMonitor()
    {
        var process = new Process();
        var startInfo = new ProcessStartInfo(androidSdkPath+"/tools/monitor.bat");
        process.StartInfo = startInfo;
        process.Start();
        debug+=("Android Monitor Starting...\n");
    }

    /**
     * Very heavy to render this out so watch out, disabled for now
     */
    void StartLogcat()
    {
        var process = new Process();
        var startInfo = new ProcessStartInfo(androidSdkPath+"/platform-tools/adb.exe", "logcat");
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = false;
        process.StartInfo = startInfo;
        process.OutputDataReceived += (sender, argsx) => DebugData(argsx.Data);
        process.Start();
        process.BeginOutputReadLine();
        //process.WaitForExit();
    }

    void Connect()
    {
        var process = new Process();
        var startInfo = new ProcessStartInfo(androidSdkPath+"/platform-tools/adb.exe", "connect "+remoteIp);
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        process.StartInfo = startInfo;
        process.OutputDataReceived += (sender, argsx) => DebugData(argsx.Data);
        process.Start();
        process.BeginOutputReadLine();
        process.WaitForExit();
    }

    void Disconnect()
    {
        var process = new Process();
        var startInfo = new ProcessStartInfo(androidSdkPath+"/platform-tools/adb.exe", "disconnect");
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        process.StartInfo = startInfo;
        process.OutputDataReceived += (sender, argsx) => DebugData(argsx.Data);
        process.Start();
        process.BeginOutputReadLine();
        process.WaitForExit();
    }

    void Devices()
    {
        var process = new Process();
        var startInfo = new ProcessStartInfo(androidSdkPath+"/platform-tools/adb.exe", "devices");
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        process.StartInfo = startInfo;
        process.OutputDataReceived += (sender, argsx) => DebugData(argsx.Data);
        process.Start();
        process.BeginOutputReadLine();
        process.WaitForExit();
    }

    void DebugData(string p_data, bool p_removeEmptyLine = true)
    {
        if (p_data != null)
        {
            if (!p_removeEmptyLine || p_data.Length != 0)
            {
                debug += p_data + "\n";
                if (debugToConsole)
                {
                    Debug.Log(p_data);
                }
            }
        }
    }

    void FetchIPAddress()
    {
        var process = new Process();
        var startInfo = new ProcessStartInfo(androidSdkPath+"/platform-tools/adb.exe", "shell ip -f inet addr show "+wlan);
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        process.StartInfo = startInfo;
        process.OutputDataReceived += (sender, argsx) =>
        {
            if (argsx.Data.IndexOf("    inet") == 0)
            {
                remoteIp = argsx.Data.Substring(9, argsx.Data.IndexOf("/") - 9);
                DebugData("Fetched IP: "+remoteIp);
            }
        };
        process.Start();
        process.BeginOutputReadLine();
        process.WaitForExit();
    }
}
