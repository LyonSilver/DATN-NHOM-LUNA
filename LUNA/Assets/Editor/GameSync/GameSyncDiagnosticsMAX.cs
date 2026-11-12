#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GameSyncPRO
{
    // Read-only full-project audit. No GUID rewrites, no auto-wiring of gameplay methods.
    public sealed class GameSyncDiagnosticsMAX : EditorWindow
    {
        [Serializable] public class Issue
        {
            public string severity, group, asset, hierarchy, description;
            public Issue(string s,string g,string a,string h,string d) { severity=s;group=g;asset=a;hierarchy=h;description=d; }
        }
        readonly List<Issue> issues = new List<Issue>();
        Vector2 scroll;
        bool includePackages=false, scanScenes=true, scanPrefabs=true, scanAssets=true;
        string filter="";
        string lastReport="";
        [MenuItem("Tools/Game Sync PRO/Full Project Audit MAX")]
        static void Open() { GetWindow<GameSyncDiagnosticsMAX>("Sync Audit MAX").minSize=new Vector2(780,540); }
        void OnGUI()
        {
            EditorGUILayout.LabelField("GAME SYNC PRO MAX - PROJECT AUDIT",EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Audit is read-only. UI changes and backups are in Tools > Game Sync PRO > Open Manager. Gameplay integration requires actual project APIs.",MessageType.Info);
            using(new EditorGUILayout.HorizontalScope())
            {
                scanScenes=GUILayout.Toggle(scanScenes,"Scenes",GUILayout.Width(90));
                scanPrefabs=GUILayout.Toggle(scanPrefabs,"Prefabs",GUILayout.Width(90));
                scanAssets=GUILayout.Toggle(scanAssets,"Assets",GUILayout.Width(90));
                includePackages=GUILayout.Toggle(includePackages,"Package dependencies",GUILayout.Width(180));
            }
            using(new EditorGUILayout.HorizontalScope())
            {
                if(GUILayout.Button("SCAN ALL (READ ONLY)",GUILayout.Height(32))) Scan();
                if(GUILayout.Button("EXPORT CSV",GUILayout.Height(32))) Export();
                if(GUILayout.Button("OPEN UI SAFE APPLY",GUILayout.Height(32))) GameSyncManagerPRO.Open();
            }
            EditorGUILayout.LabelField("Issues: "+issues.Count+" | Errors: "+issues.Count(x=>x.severity=="ERROR")+" | Warnings: "+issues.Count(x=>x.severity=="WARN"));
            filter=EditorGUILayout.TextField("Filter",filter);
            if(!string.IsNullOrEmpty(lastReport)) EditorGUILayout.LabelField("Report: "+lastReport);
            scroll=EditorGUILayout.BeginScrollView(scroll);
            foreach(var issue in issues)
            {
                if(!string.IsNullOrEmpty(filter) && !(issue.group+issue.asset+issue.hierarchy+issue.description).ToLowerInvariant().Contains(filter.ToLowerInvariant())) continue;
                using(new EditorGUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.LabelField(issue.severity+" / "+issue.group,EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(issue.asset,EditorStyles.miniLabel);
                    if(!string.IsNullOrEmpty(issue.hierarchy)) EditorGUILayout.LabelField(issue.hierarchy,EditorStyles.miniLabel);
                    EditorGUILayout.LabelField(issue.description,EditorStyles.wordWrappedLabel);
                    if(GUILayout.Button("Select asset",GUILayout.Width(110))) { var obj=AssetDatabase.LoadMainAssetAtPath(issue.asset); if(obj) { Selection.activeObject=obj;EditorGUIUtility.PingObject(obj); } }
                }
            }
            EditorGUILayout.EndScrollView();
        }
        void Add(string severity,string group,string asset,string hierarchy,string message) { issues.Add(new Issue(severity,group,asset,hierarchy,message)); }
        void Scan()
        {
            issues.Clear();
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var previous=SceneManager.GetActiveScene().path;
            var paths=AssetDatabase.GetAllAssetPaths().Where(p=>p.StartsWith("Assets/",StringComparison.Ordinal)).ToArray();
            try
            {
                for(int i=0;i<paths.Length;i++)
                {
                    string p=paths[i];
                    if(EditorUtility.DisplayCancelableProgressBar("Game Sync Audit",p,(float)i/Math.Max(1,paths.Length))) break;
                    try
                    {
                        if(scanAssets && !AssetDatabase.IsValidFolder(p)) InspectAsset(p);
                        if(scanPrefabs && p.EndsWith(".prefab",StringComparison.OrdinalIgnoreCase)) InspectPrefab(p);
                        if(scanScenes && p.EndsWith(".unity",StringComparison.OrdinalIgnoreCase)) InspectScene(p);
                    }
                    catch(Exception ex) { Add("ERROR","SCAN",p,"",ex.Message); }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                if(!string.IsNullOrEmpty(previous))
                {
                    try { EditorSceneManager.OpenScene(previous,OpenSceneMode.Single); }
                    catch(Exception ex) { Add("ERROR","RESTORE SCENE",previous,"",ex.Message); }
                }
            }
            Repaint();
        }
        void InspectAsset(string path)
        {
            if(path.EndsWith(".meta",StringComparison.OrdinalIgnoreCase)) return;
            if(AssetDatabase.LoadMainAssetAtPath(path)==null && !path.EndsWith(".cs",StringComparison.OrdinalIgnoreCase))
                Add("WARN","ASSET",path,"","Main asset failed to load (check importer/console).");
            foreach(string dep in AssetDatabase.GetDependencies(path,false))
            {
                if(!dep.StartsWith("Assets/",StringComparison.Ordinal) && !(includePackages && dep.StartsWith("Packages/",StringComparison.Ordinal))) continue;
                if(AssetDatabase.LoadMainAssetAtPath(dep)==null && !AssetDatabase.IsValidFolder(dep) && !dep.EndsWith(".cs",StringComparison.OrdinalIgnoreCase))
                    Add("ERROR","DEPENDENCY",path,"","Unresolved dependency: "+dep);
            }
            if(path.EndsWith(".wav",StringComparison.OrdinalIgnoreCase) || path.EndsWith(".mp3",StringComparison.OrdinalIgnoreCase) || path.EndsWith(".ogg",StringComparison.OrdinalIgnoreCase))
            {
                var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if(!clip) Add("WARN","AUDIO",path,"","AudioClip import failed.");
            }
            if(path.EndsWith(".png",StringComparison.OrdinalIgnoreCase))
            {
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                if(importer!=null && importer.textureType==TextureImporterType.Sprite && importer.alphaSource==TextureImporterAlphaSource.None)
                    Add("INFO","SPRITE",path,"","Sprite alpha source is None; verify transparency if this is UI/VFX art.");
            }
        }
        void InspectPrefab(string path)
        {
            GameObject root=null;
            try { root=PrefabUtility.LoadPrefabContents(path); InspectTree(root,path); }
            finally { if(root) PrefabUtility.UnloadPrefabContents(root); }
        }
        void InspectScene(string path)
        {
            var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
            foreach(var root in scene.GetRootGameObjects()) InspectTree(root,path);
            var canvases=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Canvas>(true)).ToArray();
            if(canvases.Length>0 && !scene.GetRootGameObjects().Any(r=>r.GetComponentInChildren<EventSystem>(true)!=null))
                Add("WARN","UI",path,"","Canvas exists but no EventSystem in this scene.");
        }
        void InspectTree(GameObject root,string asset)
        {
            foreach(var tr in root.GetComponentsInChildren<Transform>(true))
            {
                string hierarchy=Hierarchy(tr);
                if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(tr.gameObject)>0)
                    Add("ERROR","MISSING SCRIPT",asset,hierarchy,"GameObject has missing MonoBehaviour script(s).");
                foreach(var component in tr.GetComponents<Component>())
                {
                    if(!component) continue;
                    try { InspectSerialized(component,asset,hierarchy); }
                    catch(Exception ex) { Add("WARN","SERIALIZATION",asset,hierarchy,ex.Message); }
                }
                var button=tr.GetComponent<Button>();
                if(button)
                {
                    if(!button.targetGraphic) Add("WARN","BUTTON",asset,hierarchy,"No targetGraphic; transitions may not be visible.");
                    if(button.transition==Selectable.Transition.None) Add("INFO","BUTTON",asset,hierarchy,"Button transition disabled (may be intentional).");
                    InspectEvents(button.onClick,asset,hierarchy,"Button.onClick");
                }
                var scaler=tr.GetComponent<CanvasScaler>();
                if(scaler && scaler.uiScaleMode==CanvasScaler.ScaleMode.ConstantPixelSize)
                    Add("INFO","CANVAS",asset,hierarchy,"Constant Pixel Size: consider Scale With Screen Size for responsive UI.");
                var audio=tr.GetComponent<AudioSource>();
                if(audio && !audio.clip && audio.playOnAwake) Add("WARN","AUDIO",asset,hierarchy,"Play On Awake is enabled but AudioClip is empty (could be assigned at runtime).");
                var ps=tr.GetComponent<ParticleSystem>();
                if(ps && !tr.gameObject.activeSelf) Add("INFO","VFX",asset,hierarchy,"ParticleSystem object inactive; verify runtime activation.");
                var renderer=tr.GetComponent<SpriteRenderer>();
                if(renderer && !renderer.sprite) Add("WARN","SPRITE",asset,hierarchy,"SpriteRenderer has no sprite (could be assigned by Animator/script).");
                var animator=tr.GetComponent<Animator>();
                if(animator && !animator.runtimeAnimatorController) Add("WARN","ANIMATION",asset,hierarchy,"Animator has no RuntimeAnimatorController.");
                string n=tr.name.ToLowerInvariant();
                if(n.Contains("shop") || n.Contains("inventory") || n.Contains("save") || n.Contains("currency"))
                {
                    if(tr.GetComponents<MonoBehaviour>().Length==0)
                        Add("INFO","GAMEPLAY",asset,hierarchy,"Named gameplay object has no MonoBehaviour on this object; verify parent/child services and runtime bindings.");
                }
            }
        }
        void InspectSerialized(Component component,string asset,string hierarchy)
        {
            var so=new SerializedObject(component);
            var prop=so.GetIterator();
            bool enter=true;
            while(prop.NextVisible(enter))
            {
                enter=false;
                if(prop.propertyType==SerializedPropertyType.ObjectReference && prop.objectReferenceValue==null && prop.objectReferenceInstanceIDValue!=0)
                    Add("ERROR","BROKEN REFERENCE",asset,hierarchy,component.GetType().Name+"."+prop.propertyPath+" has a missing object reference.");
            }
        }
        void InspectEvents(UnityEvent evt,string asset,string hierarchy,string label)
        {
            for(int i=0;i<evt.GetPersistentEventCount();i++)
            {
                var target=evt.GetPersistentTarget(i);
                var method=evt.GetPersistentMethodName(i);
                if(!target || string.IsNullOrEmpty(method)) Add("ERROR","UNITY EVENT",asset,hierarchy,label+" entry "+i+" has missing target/method.");
                else if(evt.GetPersistentListenerState(i)==UnityEventCallState.Off) Add("INFO","UNITY EVENT",asset,hierarchy,label+" entry "+i+" is disabled.");
            }
            if(evt.GetPersistentEventCount()==0) Add("INFO","BUTTON",asset,hierarchy,label+" has no persistent listeners (may use AddListener at runtime).");
        }
        static string Hierarchy(Transform tr)
        {
            var names=new List<string>();
            while(tr!=null) { names.Add(tr.name); tr=tr.parent; }
            names.Reverse();return string.Join("/",names);
        }
        void Export()
        {
            string path=EditorUtility.SaveFilePanel("Save Audit CSV",Application.dataPath,"GameSyncAudit.csv","csv");
            if(string.IsNullOrEmpty(path)) return;
            var sb=new StringBuilder("Severity,Group,Asset,Hierarchy,Description\n");
            foreach(var x in issues) sb.AppendLine(string.Join(",",new[]{x.severity,x.group,x.asset,x.hierarchy,x.description}.Select(Csv)));
            File.WriteAllText(path,sb.ToString(),new UTF8Encoding(true));lastReport=path;
            EditorUtility.RevealInFinder(path);
        }
        static string Csv(string s) { return "\""+(s??"").Replace("\"","\"\"").Replace("\r"," ").Replace("\n"," ")+"\""; }
    }
}
#endif
