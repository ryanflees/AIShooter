// using UnityEngine;
// using UnityEditor;
// using KinematicCharacterController;
//
// namespace CR
// {
//     /// <summary>
//     /// Editor tool for quickly setting up FPS character and runtime managers in the scene
//     /// </summary>
//     public class FPSSetupTool : EditorWindow
//     {
//         private bool _createMetaGame = true;
//         private bool _createRuntimeInputManager = true;
//         private bool _createFPSPlayer = true;
//
//         private GlobalConfig _globalConfig;
//
//         [MenuItem("Tools/CR/FPS Setup Tool")]
//         public static void ShowWindow()
//         {
//             FPSSetupTool window = GetWindow<FPSSetupTool>("FPS Setup Tool");
//             window.minSize = new Vector2(400, 500);
//             window.Show();
//         }
//
//         private void OnGUI()
//         {
//             GUILayout.Label("FPS Character Setup Tool", EditorStyles.boldLabel);
//             GUILayout.Space(10);
//
//             EditorGUILayout.HelpBox(
//                 "This tool will create the necessary runtime managers and FPS character in your scene.\n\n" +
//                 "Runtime Managers (Persistent - DontDestroyOnLoad):\n" +
//                 "- MetaGame: Global game manager\n" +
//                 "- RuntimeInputManager: Centralized input management\n\n" +
//                 "Scene Objects:\n" +
//                 "- FPS Player: Complete first-person character with camera and movement",
//                 MessageType.Info);
//
//             GUILayout.Space(10);
//
//             // Runtime Managers Section
//             GUILayout.Label("Runtime Managers (Persistent)", EditorStyles.boldLabel);
//             EditorGUI.indentLevel++;
//
//             _createMetaGame = EditorGUILayout.Toggle("Create MetaGame", _createMetaGame);
//             if (_createMetaGame)
//             {
//                 EditorGUI.indentLevel++;
//                 _globalConfig = (GlobalConfig)EditorGUILayout.ObjectField("Global Config", _globalConfig, typeof(GlobalConfig), false);
//                 if (_globalConfig == null)
//                 {
//                     EditorGUILayout.HelpBox("GlobalConfig is required for MetaGame. Please assign one.", MessageType.Warning);
//                 }
//                 EditorGUI.indentLevel--;
//             }
//
//             _createRuntimeInputManager = EditorGUILayout.Toggle("Create RuntimeInputManager", _createRuntimeInputManager);
//
//             EditorGUI.indentLevel--;
//             GUILayout.Space(10);
//
//             // Scene Objects Section
//             GUILayout.Label("Scene Objects", EditorStyles.boldLabel);
//             EditorGUI.indentLevel++;
//             _createFPSPlayer = EditorGUILayout.Toggle("Create FPS Player", _createFPSPlayer);
//             EditorGUI.indentLevel--;
//
//             GUILayout.Space(20);
//
//             // Create Button
//             GUI.enabled = CanCreate();
//             if (GUILayout.Button("Create Setup", GUILayout.Height(40)))
//             {
//                 CreateSetup();
//             }
//             GUI.enabled = true;
//
//             GUILayout.Space(10);
//
//             // Quick Actions
//             GUILayout.Label("Quick Actions", EditorStyles.boldLabel);
//
//             if (GUILayout.Button("Create Only Runtime Managers"))
//             {
//                 CreateRuntimeManagers();
//             }
//
//             if (GUILayout.Button("Create Only FPS Player"))
//             {
//                 CreateFPSPlayer();
//             }
//
//             GUILayout.Space(10);
//
//             // Cleanup Section
//             GUILayout.Label("Cleanup", EditorStyles.boldLabel);
//             if (GUILayout.Button("Remove All Runtime Managers"))
//             {
//                 if (EditorUtility.DisplayDialog("Remove Runtime Managers",
//                     "Are you sure you want to remove all runtime managers from the scene?",
//                     "Yes", "Cancel"))
//                 {
//                     RemoveRuntimeManagers();
//                 }
//             }
//         }
//
//         private bool CanCreate()
//         {
//             if (_createMetaGame && _globalConfig == null)
//             {
//                 return false;
//             }
//             return _createMetaGame || _createRuntimeInputManager || _createFPSPlayer;
//         }
//
//         private void CreateSetup()
//         {
//             if (_createMetaGame || _createRuntimeInputManager)
//             {
//                 CreateRuntimeManagers();
//             }
//
//             if (_createFPSPlayer)
//             {
//                 CreateFPSPlayer();
//             }
//
//             EditorUtility.DisplayDialog("Setup Complete",
//                 "FPS setup has been created successfully!\n\n" +
//                 "Next steps:\n" +
//                 "1. Adjust character movement speeds in FPSCharacterController\n" +
//                 "2. Adjust camera sensitivity in FPSCameraController\n" +
//                 "3. Add ground colliders to your scene\n" +
//                 "4. Press Play to test!",
//                 "OK");
//         }
//
//         private void CreateRuntimeManagers()
//         {
//             Undo.IncrementCurrentGroup();
//             Undo.SetCurrentGroupName("Create Runtime Managers");
//             int undoGroup = Undo.GetCurrentGroup();
//
//             // Create MetaGame
//             if (_createMetaGame)
//             {
//                 MetaGame existingMetaGame = FindObjectOfType<MetaGame>();
//                 if (existingMetaGame == null)
//                 {
//                     GameObject metaGameObj = new GameObject("MetaGame");
//                     MetaGame metaGame = metaGameObj.AddComponent<MetaGame>();
//                     metaGame.m_GlobalConfig = _globalConfig;
//                     Undo.RegisterCreatedObjectUndo(metaGameObj, "Create MetaGame");
//                     Debug.Log("Created MetaGame (Persistent)");
//                 }
//                 else
//                 {
//                     Debug.LogWarning("MetaGame already exists in scene");
//                 }
//             }
//
//             // Create RuntimeInputManager
//             if (_createRuntimeInputManager)
//             {
//                 RuntimeInputManager existingInputManager = FindObjectOfType<RuntimeInputManager>();
//                 if (existingInputManager == null)
//                 {
//                     GameObject inputManagerObj = new GameObject("RuntimeInputManager");
//                     inputManagerObj.AddComponent<RuntimeInputManager>();
//                     Undo.RegisterCreatedObjectUndo(inputManagerObj, "Create RuntimeInputManager");
//                     Debug.Log("Created RuntimeInputManager (Persistent)");
//                 }
//                 else
//                 {
//                     Debug.LogWarning("RuntimeInputManager already exists in scene");
//                 }
//             }
//
//             Undo.CollapseUndoOperations(undoGroup);
//         }
//
//         private void CreateFPSPlayer()
//         {
//             Undo.IncrementCurrentGroup();
//             Undo.SetCurrentGroupName("Create FPS Player");
//             int undoGroup = Undo.GetCurrentGroup();
//
//             // Create root object
//             GameObject playerRoot = new GameObject("FPSPlayer");
//             Undo.RegisterCreatedObjectUndo(playerRoot, "Create FPS Player");
//             playerRoot.transform.position = Vector3.zero;
//             playerRoot.layer = LayerMask.NameToLayer("Default");
//
//             // Add KinematicCharacterMotor
//             KinematicCharacterMotor motor = playerRoot.AddComponent<KinematicCharacterMotor>();
//
//             // Use reflection to set private fields
//             System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
//             System.Type motorType = typeof(KinematicCharacterMotor);
//
//             motorType.GetField("CapsuleRadius", flags)?.SetValue(motor, 0.5f);
//             motorType.GetField("CapsuleHeight", flags)?.SetValue(motor, 2f);
//             motorType.GetField("CapsuleYOffset", flags)?.SetValue(motor, 1f);
//             motorType.GetField("GroundDetectionExtraDistance", flags)?.SetValue(motor, 0.1f);
//             motorType.GetField("MaxStableSlopeAngle", flags)?.SetValue(motor, 60f);
//             motorType.GetField("MaxStepHeight", flags)?.SetValue(motor, 0.5f);
//
//             // Set public property
//             motor.StepHandling = StepHandlingMethod.Standard;
//
//             // Add FPSCharacterController
//             FPSCharacterController character = playerRoot.AddComponent<FPSCharacterController>();
//             character.Motor = motor;
//
//             // Create CameraRoot
//             GameObject cameraRoot = new GameObject("CameraRoot");
//             cameraRoot.transform.SetParent(playerRoot.transform);
//             cameraRoot.transform.localPosition = new Vector3(0f, 1.6f, 0f);
//             Undo.RegisterCreatedObjectUndo(cameraRoot, "Create Camera Root");
//
//             // Create CameraYaw
//             GameObject cameraYaw = new GameObject("CameraYaw");
//             cameraYaw.transform.SetParent(cameraRoot.transform);
//             cameraYaw.transform.localPosition = Vector3.zero;
//             cameraYaw.transform.localRotation = Quaternion.identity;
//             Undo.RegisterCreatedObjectUndo(cameraYaw, "Create Camera Yaw");
//
//             // Add FPSCameraController to CameraYaw
//             FPSCameraController cameraController = cameraYaw.AddComponent<FPSCameraController>();
//
//             // Create CameraPitch
//             GameObject cameraPitch = new GameObject("CameraPitch");
//             cameraPitch.transform.SetParent(cameraYaw.transform);
//             cameraPitch.transform.localPosition = Vector3.zero;
//             cameraPitch.transform.localRotation = Quaternion.identity;
//             Undo.RegisterCreatedObjectUndo(cameraPitch, "Create Camera Pitch");
//
//             // Create or find Main Camera
//             Camera mainCamera = Camera.main;
//             GameObject cameraObj;
//
//             if (mainCamera == null)
//             {
//                 cameraObj = new GameObject("Main Camera");
//                 mainCamera = cameraObj.AddComponent<Camera>();
//                 cameraObj.tag = "MainCamera";
//                 cameraObj.AddComponent<AudioListener>();
//                 Undo.RegisterCreatedObjectUndo(cameraObj, "Create Main Camera");
//             }
//             else
//             {
//                 cameraObj = mainCamera.gameObject;
//                 Undo.SetTransformParent(cameraObj.transform, cameraPitch.transform, "Reparent Main Camera");
//             }
//
//             cameraObj.transform.SetParent(cameraPitch.transform);
//             cameraObj.transform.localPosition = Vector3.zero;
//             cameraObj.transform.localRotation = Quaternion.identity;
//
//             // Configure FPSCameraController
//             cameraController.Camera = mainCamera;
//             cameraController.CameraYawTransform = cameraYaw.transform;
//             cameraController.CameraPitchTransform = cameraPitch.transform;
//
//             // Configure FPSCharacterController
//             character.CameraController = cameraController;
//             character.CameraFollowPoint = cameraRoot.transform;
//
//             // Add FPSPlayerController
//             FPSPlayerController playerController = playerRoot.AddComponent<FPSPlayerController>();
//             playerController.Character = character;
//             playerController.CameraController = cameraController;
//
//             // Create MeshRoot (optional, for future character mesh)
//             GameObject meshRoot = new GameObject("MeshRoot");
//             meshRoot.transform.SetParent(playerRoot.transform);
//             meshRoot.transform.localPosition = Vector3.zero;
//             meshRoot.transform.localRotation = Quaternion.identity;
//             Undo.RegisterCreatedObjectUndo(meshRoot, "Create Mesh Root");
//             character.MeshRoot = meshRoot.transform;
//
//             // Select the created player
//             Selection.activeGameObject = playerRoot;
//
//             Undo.CollapseUndoOperations(undoGroup);
//
//             Debug.Log("Created FPS Player with complete hierarchy");
//         }
//
//         private void RemoveRuntimeManagers()
//         {
//             Undo.IncrementCurrentGroup();
//             Undo.SetCurrentGroupName("Remove Runtime Managers");
//
//             MetaGame metaGame = FindObjectOfType<MetaGame>();
//             if (metaGame != null)
//             {
//                 Undo.DestroyObjectImmediate(metaGame.gameObject);
//                 Debug.Log("Removed MetaGame");
//             }
//
//             RuntimeInputManager inputManager = FindObjectOfType<RuntimeInputManager>();
//             if (inputManager != null)
//             {
//                 Undo.DestroyObjectImmediate(inputManager.gameObject);
//                 Debug.Log("Removed RuntimeInputManager");
//             }
//         }
//     }
// }
