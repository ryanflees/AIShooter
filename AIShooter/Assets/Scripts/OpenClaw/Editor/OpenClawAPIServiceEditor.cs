using UnityEditor;
using UnityEngine;

namespace CR.OpenClaw.Editor
{
    /// <summary>
    /// Editor tools for OpenClaw API Service
    /// </summary>
    public static class OpenClawAPIServiceEditor
    {
        [MenuItem("GameObject/OpenClaw/Add API Service", false, 10)]
        public static void AddOpenClawAPIService()
        {
            // Check if service already exists
            var existingService = Object.FindObjectOfType<OpenClawAPIService>();
            if (existingService != null)
            {
                EditorUtility.DisplayDialog("OpenClaw API Service", 
                    "OpenClaw API Service already exists in the scene!", 
                    "OK");
                Selection.activeObject = existingService.gameObject;
                return;
            }
            
            // Create new GameObject
            GameObject serviceObj = new GameObject("OpenClawAPIService");
            
            // Add OpenClawAPIService component
            var service = serviceObj.AddComponent<OpenClawAPIService>();
            
            // Configure default settings
            service.port = 8080;
            service.enableServer = true;
            service.debugLogging = true;
            
            // Select the new object
            Selection.activeGameObject = serviceObj;
            
            Debug.Log("[OpenClawAPI] API Service added to scene. Server will start on play mode.");
        }
        
        [MenuItem("Tools/OpenClaw/Test API Connection")]
        public static void TestAPIConnection()
        {
            var service = Object.FindObjectOfType<OpenClawAPIService>();
            
            if (service == null)
            {
                EditorUtility.DisplayDialog("OpenClaw API Test", 
                    "No OpenClaw API Service found in scene. Please add it first.", 
                    "OK");
                return;
            }
            
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("OpenClaw API Test", 
                    "Please enter Play Mode to test the API connection.", 
                    "OK");
                return;
            }
            
            if (!service.IsServerRunning)
            {
                EditorUtility.DisplayDialog("OpenClaw API Test", 
                    "API server is not running. Enable it in the component settings.", 
                    "OK");
                return;
            }
            
            // Show test instructions
            string message = $@"OpenClaw API Server is running!

Server URL: {service.ServerUrl}

Test endpoints:
1. Health check: {service.ServerUrl}/api/health
2. Player status: {service.ServerUrl}/api/player/status
3. Player position: {service.ServerUrl}/api/player/position

You can test with curl:
curl {service.ServerUrl}/api/health
curl {service.ServerUrl}/api/player/status

Or use a web browser:
{service.ServerUrl}/api/health";

            EditorUtility.DisplayDialog("OpenClaw API Test", message, "OK");
        }
        
        [MenuItem("Tools/OpenClaw/Create OpenClaw Skill")]
        public static void CreateOpenClawSkill()
        {
            string skillContent = @"# OpenClaw Unity FPS Skill

This skill allows OpenClaw to interact with a Unity FPS game via HTTP API.

## Setup

1. Add the `OpenClawAPIService` component to your scene
2. Configure the port (default: 8080)
3. Enter Play Mode in Unity
4. The API server will start automatically

## API Endpoints

- `GET /api/health` - Health check
- `GET /api/player/status` - Get player status
- `GET /api/player/position` - Get player position
- `POST /api/player/move` - Move player (x, y, z)
- `POST /api/player/look` - Look/aim (yaw, pitch)
- `POST /api/player/jump` - Jump
- `POST /api/player/crouch` - Crouch/stand
- `POST /api/player/sprint` - Sprint/walk

## Example OpenClaw Commands

```bash
# Check if Unity game is running
curl http://localhost:8080/api/health

# Get player status
curl http://localhost:8080/api/player/status

# Move player forward
curl -X POST http://localhost:8080/api/player/move \
  -H ""Content-Type: application/json"" \
  -d '{\""x\"": 0, \""y\"": 0, \""z\"": 1}'

# Make player jump
curl -X POST http://localhost:8080/api/player/jump
```

## Integration with OpenClaw

Create a skill file that uses these API endpoints to control the game.";

            // Create skill file in workspace
            string workspacePath = Application.dataPath + "/../workspace/openclaw-unity-skill.md";
            System.IO.File.WriteAllText(workspacePath, skillContent);
            
            EditorUtility.DisplayDialog("OpenClaw Skill Created", 
                $"Skill documentation created at:\n{workspacePath}", 
                "OK");
                
            // Open the file
            System.Diagnostics.Process.Start(workspacePath);
        }
    }
}