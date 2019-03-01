using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Microsoft.SpatialAlignment
{
    /// <summary>
    /// A helper class to improve logging and status formatting.
    /// </summary>
    static public class Logger
    {
        private enum Level
        {
            Info,
            Warn,
            Error
        };

        static private void Log(Level level, string message, Component ui = null, bool toConsole = true)
        {
            Color color;
            string withPreamble;

            switch (level)
            {
                case Level.Warn:
                    color = Color.yellow;
                    withPreamble = "Warning: " + message;
                    if (toConsole) { Debug.LogWarning(message); }
                    break;
                case Level.Error:
                    color = Color.red;
                    withPreamble = "Error: " + message;
                    if (toConsole) { Debug.LogError(message); }
                    break;
                default:
                    color = Color.white;
                    withPreamble = message;
                    if (toConsole) { Debug.Log(message); }
                    break;
            }

            Text text = ui as Text;
            TextMesh textMesh = ui as TextMesh;
            if ((text == null) && (textMesh == null)) { return; }

            UnityDispatcher.InvokeOnAppThread(() =>
            {
                if (text != null)
                {
                    text.text = withPreamble;
                }
                if (textMesh != null)
                {
                    textMesh.color = color;
                    textMesh.text = message;
                }
            });
        }

        static public void LogError(string message, Component ui = null, bool toConsole = true)
        {
            Log(Level.Error, message, ui, toConsole);
        }

        static public void LogInfo(string message, Component ui = null, bool toConsole = true)
        {
            Log(Level.Info, message, ui, toConsole);
        }

        static public void LogWarn(string message, Component ui = null, bool toConsole = true)
        {
            Log(Level.Warn, message, ui, toConsole);
        }
    }
}