#if UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

// Editor-only bridge. No scene, prefab, asset or GUID writes.
[InitializeOnLoad]
public static class UnityConflictLiveBridge
{
    const string Pipe = "UnityConflictMonitorPRO_V4_Live";
    static readonly ConcurrentQueue<string> Queue = new ConcurrentQueue<string>();
    static readonly AutoResetEvent Signal = new AutoResetEvent(false);
    static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    static readonly Thread Worker;
    static volatile bool Running = true;

    static UnityConflictLiveBridge()
    {
        Application.logMessageReceivedThreaded += OnLog;
        CompilationPipeline.assemblyCompilationFinished += OnCompilation;
        EditorApplication.quitting += OnQuit;
        Worker = new Thread(SendLoop) { IsBackground = true, Name = "Unity Conflict Live Bridge" };
        Worker.Start();
    }
    static void OnQuit() { Running = false; Signal.Set(); }
    static void OnCompilation(string assembly, CompilerMessage[] messages)
    {
        foreach (var m in messages)
        {
            var severity = m.type == CompilerMessageType.Error ? "Error" : "Warning";
            var file = string.IsNullOrEmpty(m.file) ? "" : Path.GetFullPath(Path.IsPathRooted(m.file) ? m.file : Path.Combine(Root, m.file));
            Enqueue(severity, "UNITY_COMPILER", file, m.line, m.message, "CompilationPipeline");
        }
    }
    static void OnLog(string message, string stack, LogType type)
    {
        // This callback can be called from worker threads; never touch Unity APIs here.
        if (type == LogType.Log) return;
        var severity = type == LogType.Warning ? "Warning" : type == LogType.Exception || type == LogType.Assert ? "Critical" : "Error";
        var text = message ?? "";
        if (!string.IsNullOrEmpty(stack)) text += "\n" + stack;
        Enqueue(severity, "UNITY_CONSOLE", "", 0, text, "UnityConsole");
    }
    static void Enqueue(string severity, string code, string file, int line, string message, string source)
    {
        // Bound queue size to avoid memory growth while the desktop app is closed.
        if (Queue.Count > 3000) { string ignored; Queue.TryDequeue(out ignored); }
        Queue.Enqueue("{\"ProjectRoot\":\"" + Esc(Root) + "\",\"Severity\":\"" + Esc(severity) +
            "\",\"Code\":\"" + Esc(code) + "\",\"File\":\"" + Esc(file) +
            "\",\"Line\":" + line + ",\"Message\":\"" + Esc(message) +
            "\",\"Source\":\"" + Esc(source) + "\",\"Timestamp\":\"" + DateTime.UtcNow.ToString("o") + "\"}");
        Signal.Set();
    }
    static string Esc(string s) => (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
    static void SendLoop()
    {
        while (Running)
        {
            try
            {
                using (var client = new NamedPipeClientStream(".", Pipe, PipeDirection.Out))
                {
                    client.Connect(1200);
                    using (var writer = new StreamWriter(client, new UTF8Encoding(false)) { AutoFlush = true })
                    {
                        while (Running && client.IsConnected)
                        {
                            string packet;
                            if (Queue.TryDequeue(out packet)) writer.WriteLine(packet);
                            else Signal.WaitOne(500);
                        }
                    }
                }
            }
            catch (Exception) { Signal.WaitOne(1200); }
        }
    }
}
#endif
