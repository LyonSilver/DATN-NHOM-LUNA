#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace GameSyncPRO
{
    public sealed class GameSyncManagerPRO : EditorWindow
    {
        [Serializable] private class Finding
        {
            public string severity, category, path, objectName, detail;
            public Finding(string s,string c,string p,string o,string d) { severity=s; category=c; path=p; objectName=o; detail=d; }
        }
        private readonly List<Finding> findings = new List<Finding>();
        private Vector2 scroll;
        private bool scanScenes=true, scanPrefabs=true, scanAssets=true;
        private bool normalizeCanvas=true, normalizeButtons=false;
        private Vector2 referenceResolution=new Vector2(1920,1080);
        private ColorBlock buttonPalette=ColorBlock.defaultColorBlock;
        private string lastBackup="";
        private bool scanned;
        private const string BackupFolder="GameSyncBackups";
        private static readonly string[] GameTerms={"Shop","Inventory","Currency","Save","GameManager"};

        [MenuItem("Tools/Game Sync PRO/Open Manager")]
        public static void Open() { GetWindow<GameSyncManagerPRO>("Game Sync PRO").minSize=new Vector2(700,520); }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("GAME SYNC MANAGER PRO", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Scan is read-only. Apply changes only after a backup. No GUID, script or transform edits.",MessageType.Info);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scan Scope",EditorStyles.boldLabel);
            scanScenes=EditorGUILayout.ToggleLeft("Scenes",scanScenes);
            scanPrefabs=EditorGUILayout.ToggleLeft("Prefabs",scanPrefabs);
            scanAssets=EditorGUILayout.ToggleLeft("Asset dependencies / scripts / audio / textures",scanAssets);
            using(new EditorGUILayout.HorizontalScope())
            {
                if(GUILayout.Button("SCAN PROJECT",GUILayout.Height(30))) ScanProject();
                if(GUILayout.Button("EXPORT CSV",GUILayout.Height(30))) ExportCSV();
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Safe UI Normalization",EditorStyles.boldLabel);
            normalizeCanvas=EditorGUILayout.ToggleLeft("Canvas Scaler: Scale With Screen Size",normalizeCanvas);
            referenceResolution=EditorGUILayout.Vector2Field("Reference Resolution",referenceResolution);
            normalizeButtons=EditorGUILayout.ToggleLeft("Button ColorBlock (opt-in: may change visual theme)",normalizeButtons);
            if(normalizeButtons)
            {
                buttonPalette.normalColor=EditorGUILayout.ColorField("Normal",buttonPalette.normalColor);
                buttonPalette.highlightedColor=EditorGUILayout.ColorField("Highlighted",buttonPalette.highlightedColor);
                buttonPalette.pressedColor=EditorGUILayout.ColorField("Pressed",buttonPalette.pressedColor);
                buttonPalette.selectedColor=EditorGUILayout.ColorField("Selected",buttonPalette.selectedColor);
                buttonPalette.disabledColor=EditorGUILayout.ColorField("Disabled",buttonPalette.disabledColor);
            }
            using(new EditorGUI.DisabledScope(!scanned || (!normalizeCanvas && !normalizeButtons)))
            {
                if(GUILayout.Button("BACKUP + APPLY SAFE UI CHANGES",GUILayout.Height(32))) ApplySafeChanges();
            }
            if(!string.IsNullOrEmpty(lastBackup)) EditorGUILayout.SelectableLabel("Backup: "+lastBackup, GUILayout.Height(32));
            EditorGUILayout.LabelField("Findings: "+findings.Count,EditorStyles.boldLabel);
            scroll=EditorGUILayout.BeginScrollView(scroll);
            foreach(var f in findings)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(f.severity+" | "+f.category+" | "+f.objectName,EditorStyles.boldLabel);
                EditorGUILayout.SelectableLabel(f.path,GUILayout.Height(18));
                EditorGUILayout.LabelField(f.detail,EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }

        private void Add(string s,string c,string p,string o,string d) { findings.Add(new Finding(s,c,p,o,d)); }
        private void ScanProject()
        {
            findings.Clear(); scanned=false;
            try
            {
                var paths=AssetDatabase.GetAllAssetPaths().Where(p=>p.StartsWith("Assets/",StringComparison.OrdinalIgnoreCase)).ToArray();
                if(scanAssets) ScanAssetPaths(paths);
                if(scanPrefabs)
                    foreach(var p in paths.Where(p=>p.EndsWith(".prefab",StringComparison.OrdinalIgnoreCase))) ScanPrefab(p);
                if(scanScenes)
                    foreach(var p in paths.Where(p=>p.EndsWith(".unity",StringComparison.OrdinalIgnoreCase))) ScanScene(p);
                Add("INFO","Scan","Assets","Complete","Scanned "+paths.Length+" asset paths. No project files modified.");
                scanned=true;
            }
            catch(Exception e) { Add("ERROR","Scanner","Assets","Scan interrupted",e.ToString()); }
            finally { EditorUtility.ClearProgressBar(); Repaint(); }
        }
        private void ScanAssetPaths(string[] paths)
        {
            var scripts=paths.Where(p=>p.EndsWith(".cs",StringComparison.OrdinalIgnoreCase)).ToArray();
            foreach(var term in GameTerms)
                if(!scripts.Any(p=>Path.GetFileNameWithoutExtension(p).IndexOf(term,StringComparison.OrdinalIgnoreCase)>=0))
                    Add("INFO","Gameplay","Assets","Possible module: "+term,"No script filename contains this term. This is a heuristic, not proof of missing functionality.");
            int i=0;
            foreach(var p in paths)
            {
                if(++i%100==0) EditorUtility.DisplayProgressBar("Scanning asset dependencies",p,(float)i/Math.Max(1,paths.Length));
                if(p.EndsWith(".meta",StringComparison.OrdinalIgnoreCase) || AssetDatabase.IsValidFolder(p)) continue;
                try
                {
                    foreach(var dep in AssetDatabase.GetDependencies(p,false))
                        if(dep.StartsWith("Assets/",StringComparison.OrdinalIgnoreCase) && AssetDatabase.LoadMainAssetAtPath(dep)==null && !AssetDatabase.IsValidFolder(dep))
                            Add("WARN","Dependency",p,Path.GetFileName(p),"Unresolved dependency: "+dep);
                }
                catch(Exception e) { Add("WARN","Dependency",p,Path.GetFileName(p),e.Message); }
            }
        }
        private void ScanPrefab(string path)
        {
            GameObject root=null;
            try { root=PrefabUtility.LoadPrefabContents(path); InspectHierarchy(root,path); }
            catch(Exception e) { Add("ERROR","Prefab",path,Path.GetFileName(path),e.Message); }
            finally { if(root!=null) PrefabUtility.UnloadPrefabContents(root); }
        }
        private void ScanScene(string path)
        {
            // Inspect a scene additively, then close without saving or modifying existing scene setup.
            Scene scene=default(Scene);
            bool opened=false;
            try
            {
                var existing=SceneManager.GetSceneByPath(path);
                if(existing.IsValid() && existing.isLoaded) scene=existing;
                else { scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive); opened=true; }
                foreach(var root in scene.GetRootGameObjects()) InspectHierarchy(root,path);
            }
            catch(Exception e) { Add("ERROR","Scene",path,Path.GetFileName(path),e.Message); }
            finally { if(opened && scene.IsValid()) EditorSceneManager.CloseScene(scene,true); }
        }
        private void InspectHierarchy(GameObject root,string path)
        {
            foreach(var t in root.GetComponentsInChildren<Transform>(true))
            {
                var go=t.gameObject;
                if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go)>0)
                    Add("ERROR","Missing Script",path,GetHierarchyPath(t),"Missing MonoBehaviour component. Manual repair required.");
                var canvas=go.GetComponent<Canvas>();
                if(canvas!=null && canvas.renderMode!=RenderMode.WorldSpace)
                {
                    var scaler=go.GetComponent<CanvasScaler>();
                    if(scaler==null) Add("WARN","UI",path,GetHierarchyPath(t),"CanvasScaler missing.");
                    else if(scaler.uiScaleMode!=CanvasScaler.ScaleMode.ScaleWithScreenSize)
                        Add("INFO","UI",path,GetHierarchyPath(t),"CanvasScaler is not ScaleWithScreenSize.");
                }
                var button=go.GetComponent<Button>();
                if(button!=null)
                {
                    if(button.targetGraphic==null) Add("WARN","Button",path,GetHierarchyPath(t),"Button has no targetGraphic.");
                    if(button.onClick.GetPersistentEventCount()==0)
                        Add("INFO","Button",path,GetHierarchyPath(t),"No persistent OnClick listeners. Runtime listeners may still exist.");
                }
                var source=go.GetComponent<AudioSource>();
                if(source!=null && source.playOnAwake && source.clip==null)
                    Add("INFO","Audio",path,GetHierarchyPath(t),"AudioSource plays on awake but has no AudioClip.");
                var particle=go.GetComponent<ParticleSystem>();
                if(particle!=null && particle.GetComponent<ParticleSystemRenderer>()==null)
                    Add("WARN","VFX",path,GetHierarchyPath(t),"ParticleSystemRenderer missing.");
                var sr=go.GetComponent<SpriteRenderer>();
                if(sr!=null && sr.sprite==null)
                    Add("INFO","Sprite",path,GetHierarchyPath(t),"SpriteRenderer has no Sprite (may be assigned at runtime).");
            }
        }
        private static string GetHierarchyPath(Transform t)
        {
            string result=t.name;
            while(t.parent!=null) { t=t.parent; result=t.name+"/"+result; }
            return result;
        }
        private void ApplySafeChanges()
        {
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if(!EditorUtility.DisplayDialog("Game Sync PRO","Create a backup, then modify CanvasScaler and optionally Button colors in selected scenes/prefabs?","Backup and Apply","Cancel")) return;
            var paths=AssetDatabase.GetAllAssetPaths().Where(p=>p.StartsWith("Assets/") && ((scanScenes && p.EndsWith(".unity")) || (scanPrefabs && p.EndsWith(".prefab")))).ToArray();
            if(paths.Length==0) { EditorUtility.DisplayDialog("No files","Select Scenes and/or Prefabs before applying.","OK"); return; }
            var root=Directory.GetParent(Application.dataPath).FullName;
            lastBackup=Path.Combine(root,BackupFolder,DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            try
            {
                // Backup all targeted scene/prefab files and their .meta files before any edit.
                foreach(var path in paths)
                {
                    var src=Path.Combine(root,path);
                    if(!File.Exists(src)) continue;
                    var dest=Path.Combine(lastBackup,path);
                    Directory.CreateDirectory(Path.GetDirectoryName(dest));
                    File.Copy(src,dest,true);
                    if(File.Exists(src+".meta")) File.Copy(src+".meta",dest+".meta",true);
                }
            }
            catch(Exception e) { EditorUtility.DisplayDialog("Backup failed",e.Message,"OK"); return; }
            int changed=0,failed=0;
            try
            {
                for(int i=0;i<paths.Length;i++)
                {
                    var p=paths[i];
                    if(EditorUtility.DisplayCancelableProgressBar("Applying safe UI sync",p,(float)i/paths.Length)) break;
                    try
                    {
                        if(p.EndsWith(".prefab",StringComparison.OrdinalIgnoreCase))
                        {
                            GameObject obj=null;
                            try { obj=PrefabUtility.LoadPrefabContents(p); if(ApplyHierarchy(obj)) { PrefabUtility.SaveAsPrefabAsset(obj,p); changed++; } }
                            finally { if(obj!=null) PrefabUtility.UnloadPrefabContents(obj); }
                        }
                        else
                        {
                            Scene scene=default(Scene); bool opened=false;
                            try
                            {
                                var existing=SceneManager.GetSceneByPath(p);
                                if(existing.IsValid() && existing.isLoaded) scene=existing;
                                else { scene=EditorSceneManager.OpenScene(p,OpenSceneMode.Additive); opened=true; }
                                bool dirty=false;
                                foreach(var go in scene.GetRootGameObjects()) dirty|=ApplyHierarchy(go);
                                if(dirty) { EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); changed++; }
                            }
                            finally { if(opened && scene.IsValid()) EditorSceneManager.CloseScene(scene,true); }
                        }
                    }
                    catch(Exception e) { failed++; Add("ERROR","Apply",p,Path.GetFileName(p),e.Message); }
                }
                AssetDatabase.SaveAssets();
            }
            finally { EditorUtility.ClearProgressBar(); }
            scanned=false;
            EditorUtility.DisplayDialog("Game Sync PRO","Modified files: "+changed+"\nFailed: "+failed+"\nBackup: "+lastBackup+"\nRe-scan and test gameplay before committing.","OK");
        }
        private bool ApplyHierarchy(GameObject root)
        {
            bool changed=false;
            foreach(var t in root.GetComponentsInChildren<Transform>(true))
            {
                var go=t.gameObject;
                if(normalizeCanvas)
                {
                    var canvas=go.GetComponent<Canvas>();
                    if(canvas!=null && canvas.renderMode!=RenderMode.WorldSpace)
                    {
                        var scaler=go.GetComponent<CanvasScaler>();
                        if(scaler==null) { scaler=go.AddComponent<CanvasScaler>(); changed=true; }
                        var resolution=new Vector2(Mathf.Max(1,referenceResolution.x),Mathf.Max(1,referenceResolution.y));
                        if(scaler.uiScaleMode!=CanvasScaler.ScaleMode.ScaleWithScreenSize || scaler.referenceResolution!=resolution)
                        {
                            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
                            scaler.referenceResolution=resolution;
                            changed=true;
                        }
                    }
                }
                if(normalizeButtons)
                {
                    var button=go.GetComponent<Button>();
                    if(button!=null && button.transition==Selectable.Transition.ColorTint)
                    {
                        var cb=button.colors;
                        cb.normalColor=buttonPalette.normalColor;
                        cb.highlightedColor=buttonPalette.highlightedColor;
                        cb.pressedColor=buttonPalette.pressedColor;
                        cb.selectedColor=buttonPalette.selectedColor;
                        cb.disabledColor=buttonPalette.disabledColor;
                        if(!cb.Equals(button.colors)) { button.colors=cb; changed=true; }
                    }
                }
            }
            return changed;
        }
        private void ExportCSV()
        {
            if(findings.Count==0) { EditorUtility.DisplayDialog("No report","Run Scan Project first.","OK"); return; }
            var file=EditorUtility.SaveFilePanel("Export Game Sync Report",Application.dataPath,"GameSyncReport.csv","csv");
            if(string.IsNullOrEmpty(file)) return;
            var sb=new StringBuilder("Severity,Category,AssetPath,Object,Detail\r\n");
            foreach(var f in findings) sb.AppendLine(string.Join(",",new[]{f.severity,f.category,f.path,f.objectName,f.detail}.Select(Csv)));
            File.WriteAllText(file,"\uFEFF"+sb.ToString(),Encoding.UTF8);
            EditorUtility.RevealInFinder(file);
        }
        private static string Csv(string value) { return "\""+(value??"").Replace("\"","\"\"").Replace("\r"," ").Replace("\n"," ")+"\""; }
    }
}
#endif
