using System.Linq;
using System.Text;
using Unity.Microsoft.GDK.Discovery;
using UnityEditor;
using UnityEngine;

namespace Unity.Microsoft.GDK.Discovery.Sample
{
    public partial class GdkDiscoverySampleMenu : EditorWindow
    {
        [MenuItem("GDK/Discovery Samples/Query Installed GDKs")]
        public static void QueryInstalledGdks()
        {
            var gdks = GdkEnumerator.DiscoveredGdks;

            var sb = new StringBuilder();
            sb.AppendLine("Installed GDKs Results...")
                .AppendLine($"{gdks.Count()} GDK Installation(s) discovered.");

            if (gdks.Any())
            {
                foreach (var gdk in gdks)
                {
                    AppendGdkInfoToStringBuilder(sb, gdk);
                }
            }
            else
            {
                sb.AppendLine("No GDKs discovered.");
            }

            Debug.Log(sb.ToString());
        }

        private static void AppendGdkInfoToStringBuilder(StringBuilder sb, GdkData gdk)
        {
            sb.AppendLine($"GDK Edition {gdk.Edition}:")
                .AppendLine($"\tFriendly Name: {GdkVersion.Format(gdk.Edition)}")
                .AppendLine($"\tPath: {gdk.Path}")
                .AppendLine($"\tComponents:");

            if (gdk.ComponentPaths.Any())
            {
                foreach (var component in gdk.ComponentPaths)
                {
                    sb.AppendLine($"\t{component.Key}: {component.Value}");
                }
            }
            else
            {
                sb.AppendLine("\tNo components found.");
            }

            sb.AppendLine();
        }
    }
}