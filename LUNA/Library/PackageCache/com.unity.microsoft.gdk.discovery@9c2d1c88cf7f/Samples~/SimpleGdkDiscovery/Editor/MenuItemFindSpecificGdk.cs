using System;
using System.Linq;
using System.Text;
using Unity.Microsoft.GDK.Discovery;
using UnityEditor;
using UnityEngine;

namespace Unity.Microsoft.GDK.Discovery.Sample
{
    public partial class GdkDiscoverySampleMenu : EditorWindow
    {
        private const string k_GdkEditionQuery = "251000";

        [MenuItem("GDK/Discovery Samples/Find Specific Edition")]
        public static void FindSpecificGdk()
        {
            string editionInput = SimpleInputDialog.Show("GDK Edition", "Enter GDK Edition number:", k_GdkEditionQuery);
            if (string.IsNullOrEmpty(editionInput) || !int.TryParse(editionInput, out int edition))
            {
                Debug.LogError("Invalid GDK Edition number entered.");
                return;
            }

            var specificGdk = GdkEnumerator.DiscoveredGdks.FirstOrDefault(gdk => gdk.Edition == edition);

            var result = (specificGdk.Edition != 0)
                ? GenerateFoundGdkMessage(specificGdk, edition)
                : GenerateNotFoundMessage(edition);

            Debug.Log(result);
        }

        private static string GenerateFoundGdkMessage(GdkData gdk, int edition)
        {
            StringBuilder sb =  new StringBuilder();
            
            sb.AppendLine($"Found GDK Edition {edition} at: {gdk.Path}:")
                .AppendLine($"\tFriendly Name: {GdkVersion.Format(gdk.Edition)}")
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

            return sb.ToString();
        }

        private static string GenerateNotFoundMessage(int edition)
        {
            return $"GDK Edition {edition} not found.";
        }
    }

    /// <summary>
    /// A simple input dialog window in the Unity Editor.
    /// </summary>
    public class SimpleInputDialog : EditorWindow
    {
        /// <summary>
        /// Shows a modal input dialog and returns the user's input.
        /// </summary>
        /// <param name="title">The title of the dialog window.</param>
        /// <param name="message">The message to display in the dialog.</param>
        /// <param name="defaultText">The default text to display in the input field.</param>
        /// <returns>The text entered by the user.</returns>
        public static string Show(string title, string message, string defaultText)
        {
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException(nameof(title), "Title cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message), "Message cannot be null or empty.");
            }

            SimpleInputDialog window = CreateInstance<SimpleInputDialog>();

            window.titleContent = new GUIContent(title);
            window.minSize = new Vector2(400, 100);
            window.m_Message = message;
            window.m_InputText = defaultText ?? string.Empty;

            window.ShowModalUtility();

            return window.m_Result;
        }

        private string m_Result;
        private string m_Message;
        private string m_InputText;
        private bool m_Cancelled = true;

        private void OnGUI()
        {
            EditorGUILayout.LabelField(m_Message);
            m_InputText = EditorGUILayout.TextField("", m_InputText);

            if (GUILayout.Button("OK"))
            {
                m_Result = m_InputText;
                m_Cancelled = false;
                Close();
            }
        }
        private void OnDestroy()
        {
            if (m_Cancelled)
            {
                m_Result = null;
            }
        }
    }
}