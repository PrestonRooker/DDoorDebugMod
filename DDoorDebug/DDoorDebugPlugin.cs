using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;
using UnityEngine.Rendering;
using static HarmonyLib.AccessTools;
using DDoorDebug.Model;
using DDoorDebug.Extensions;
using static Damageable;
using static DDoorDebug.Model.PluginOptions;
using static DDoorDebug.Model.PluginCache;

namespace DDoorDebug
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class DDoorDebugPlugin : BaseUnityPlugin
    {
        const string NAME = "DDoorDebugPlugin";
        const string VERSION = "0.3.5";
        const string GUID = "org.bepinex.plugins.ddoordebugkz";
        //-
        public static DDoorDebugPlugin instance { get; private set; }
        public DDoorDebugData DData  { get; private set;}
        public PluginOptions Options { get; private set;}
        public PluginCache Cache { get; private set;}
        private readonly MutableString builder = new MutableString(600, true);

        // GUI
        private Matrix4x4 guiMatrix;
        private Vector2 SMscroll;
        private Material GLinesMaterial;
        private Material MeshMaterial;
        private Material BoxMaterial;
        private int sceneIndex = 0;
        private float tickFrameTime;
        private string guiOutputStr = "";
        private readonly RectOffset graphOffset = new RectOffset(0, 0, 0, 0);
        private int statIndex = 0;

        // Reflection access
        public static FieldRef<_ChargeWeapon, float> chargedPower = FieldRefAccess<_ChargeWeapon, float>("chargedPower");
        public static FieldRef<Damageable, float> currentHealth = FieldRefAccess<Damageable, float>("currentHealth");
        public static FieldRef<DamageableCharacter, bool> calledDie = FieldRefAccess<DamageableCharacter, bool>("calledDie");
        public static FieldRef<CameraRotationControl, float> angle = FieldRefAccess<CameraRotationControl, float>("angle");
        public static FieldRef<FovZoom, float> currentBaseFov = FieldRefAccess<FovZoom, float>("currentBaseFov");
        public static FieldRef<PlayerGlobal, PlayerInputControl> input = FieldRefAccess<PlayerGlobal, PlayerInputControl>("input");

        //wijo's stuff
        public static bool NoClip = false; //noclip
        public static float oldSlowDown = 0; //noclip
        public static float oldAcc = 1; //noclip
        public static float oldSpeed = 0; //noclip
        public static PlayerMovementControl movementControl; //noclip
        public static string[] statNames = new string[] { "stat_melee", "stat_dexterity", "stat_haste", "stat_magic" }; //stat menu
        public static float timescale = 1f; //timescale
        public static bool paused = false; //timescale
        public static bool wasSlow = false; //timescale
        public static bool isTurning = false; //cam rotation
        public static bool HasZoomed = false; //zoom reset
        public static float baseZoom = 1f; //zoom reset
        public static bool infMagic = false; //inf magic
        public static bool justReloaded = false; //used for reload file
        public static Dictionary<string, int> spellDic = new Dictionary<string, int>() //for reload file spell fix
        {
            { "arrow", 1 },
            { "fire", 2 },
            { "bombs", 3 },
            { "hookshot", 4 }
        };
        public static bool skipcs = false; //skipping cutscenes
        public static bool inputwaspaused = false; //skipping cutscenes
        public Dictionary<int, Vector3> savePosDic = new Dictionary<int, Vector3>(); // save pos

        //bind menu
        public static List<String>[] features = new List<string>[] // { "name in config file", "default bind", "default modifiers", "allow extra modifiers" (t/f) }
        {
            new List<string>() { "Open binding menu", "Tab", "", "t" },
            new List<string>() { "Info menu", "F1", "", "t" },
            new List<string>() { "Show hp", "F2", "", "t" },
            new List<string>() { "Warp menu", "F3", "", "f" },
            new List<string>() { "Warp to selected", "F3", "c", "t" },
            new List<string>() { "Heal to full", "F4", "", "f" },
            new List<string>() { "Auto heal", "", "", "t" },
            new List<string>() { "Inf magic", "F4", "c", "f" },
            new List<string>() { "Toggle godmode", "F4", "s", "f" },
            new List<string>() { "Boss reset", "F5", "", "t" },
            new List<string>() { "Boss reset with cuts", "F5", "s", "t" },
            new List<string>() { "Give soul", "F6", "", "t" },
            new List<string>() { "Unlock weapons", "F7", "", "t" },
            new List<string>() { "Unlock spells", "F7", "s", "t" },
            new List<string>() { "Save pos", "F8", "", "f" },
            new List<string>() { "Load pos", "F9", "", "f" },
            new List<string>() { "Save gpos", "F8", "s", "t" },
            new List<string>() { "Load gpos", "F9", "s", "t" },
            new List<string>() { "Show colliders", "F10", "", "f" },
            new List<string>() { "Load visible colliders", "F10", "c", "t" },
            new List<string>() { "Freecam", "F11", "", "t" },
            new List<string>() { "Pos history", "P", "", "t" },
            new List<string>() { "Velocity graph", "Backspace", "", "t" },
            new List<string>() { "Timescale down", "Insert", "", "t" },
            new List<string>() { "Timescale up", "PageUp", "", "t" },
            new List<string>() { "Reset timescale", "Home", "", "t" },
            new List<string>() { "Rotate cam right", "Delete", "", "t" },
            new List<string>() { "Rotate cam left", "PageDown", "", "t" },
            new List<string>() { "Reset cam", "End", "", "t" },
            new List<string>() { "Mouse tele", "Mouse0", "c", "t" },
            new List<string>() { "Zoom in", "Minus", "", "t" },
            new List<string>() { "Zoom out", "Equals", "", "t" },
            new List<string>() { "Toggle noclip", "U", "", "t" },
            new List<string>() { "Tele up", "H", "", "t" },
            new List<string>() { "Tele down", "J", "", "t" },
            new List<string>() { "Toggle night", "", "", "t" },
            new List<string>() { "Save file", "S", "c", "t" },
            new List<string>() { "Reload file", "O", "c", "t" },
            new List<string>() { "Get gp", "", "", "t" },
            new List<string>() { "Instant textskip", "", "", "t" },
            new List<string>() { "Toggle Timestop", "", "", "t" },
        };
        
        public static Hashtable featureBinds = new Hashtable(); // { "name in config file", "bind" }
        public static bool bindMenuOpen = false;
        public static List<String> bufferedActions = new List<String>();
        public static String listeningForKey = "";
        public static KeyCode foundKey;

        internal static new ManualLogSource Log; //logging (idk if this is needed whatever I have it in other projects too c:)

        private void Awake()
        {
            Log = base.Logger;

            foreach(var feature in features)
            {
                featureBinds.Add(feature[0], new Bind(Config.Bind(feature[0], "key/button", feature[1]), Config.Bind(feature[0], "modifiers", feature[2]), Config.Bind(feature[0], "allow extra modifiers", feature[3])));
            }

            instance = this;
            Options = new PluginOptions();
            DData = new DDoorDebugData();
            Cache = new PluginCache();
            PopulateDData();
            PrepareGUI();
            var harmony = new Harmony(GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            harmony.PatchAll(typeof(DDoorDebugPlugin));
        }

        private void PrepareGUI()
		{
            Camera.onPostRender += OnPostRenderCallback;
            SetMatrix(Screen.width, Screen.height);
            GLinesMaterial = new Material(Shader.Find("Hidden/Internal-Colored")) { hideFlags = HideFlags.HideAndDontSave };
            GLinesMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            GLinesMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            GLinesMaterial.SetInt("_Cull", (int)CullMode.Off);
            //GLinesMaterial.SetInt("_ZWrite", 1);
            //GLinesMaterial.SetInt("_ZTest", 2);
            var abundle = AssetBundle.LoadFromFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/wfshader.bundle");
            MeshMaterial = new Material(abundle.LoadAsset<Shader>("wireframetransparent")) { hideFlags = HideFlags.HideAndDontSave };
            CopyMesh(GameObject.CreatePrimitive(PrimitiveType.Sphere), ref Cache.sphereMesh);
            //CopyMesh(GameObject.CreatePrimitive(PrimitiveType.Cube), ref Cache.boxMesh);
            abundle.Unload(false);
		}

        private void CopyMesh(GameObject original, ref Mesh target)
        {
            var shared = original.GetComponent<MeshFilter>().sharedMesh;
            target = new Mesh();
            target.SetVertices(shared.vertices);
            target.SetTriangles(shared.triangles, 0);
            target.RecalculateBounds();
            target.RecalculateNormals();
            target.Optimize();
            Destroy(original);
        }

        private void FullSweep(Transform trans)
        {
            var rawColliders = FindObjectsOfType(typeof(Collider));
            if (rawColliders == null || rawColliders.Length < 1) return;
            Cache.ClearColliderCache();
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Cache.mainCam);
            for (int i = 0; i < rawColliders.Length; i++)
            {
                var hitCollider = rawColliders[i] as Collider;
                if (hitCollider && GeometryUtility.TestPlanesAABB(planes , hitCollider.bounds))
                    //&& Globals.instance.solidLayers == (Globals.instance.solidLayers | (1 << hitCollider.gameObject.layer)))
                {
                    if (hitCollider is BoxCollider box)
                        Cache.boxData.Add(box);
                    else if (hitCollider is MeshCollider mesh && !hitCollider.isTrigger && mesh.sharedMesh.isReadable)
                        Cache.meshData.Add(mesh);
                    else if (hitCollider is CapsuleCollider capsule && !hitCollider.isTrigger)
                        Cache.capsuleData.Add(new CapsuleData() { mesh = GenerateCapsule(capsule.height, capsule.radius, capsule), collider = capsule });
                    else if (hitCollider is SphereCollider sphere && !sphere.isTrigger)
                        Cache.sphereData.Add(sphere);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Mesh GenerateCapsule(float height, float radius, CapsuleCollider capsule, int segments = 24) 
	    {
            //calculate exact size sowe dont do anys caling later
            radius = radius * Mathf.Max(Mathf.Abs(capsule.transform.lossyScale.x), Mathf.Abs(capsule.transform.lossyScale.x));
		    height = height * capsule.transform.lossyScale.y;

		    if (segments % 2 != 0)
                segments ++;
		
		    // extra vertex on the seam
		    int points = segments + 1;
		
		    // calculate points around a circle
		    float[] pX = new float[points];
		    float[] pZ = new float[points];
		    float[] pY = new float[points];
		    float[] pR = new float[points];
		
		    float calcH = 0f;
		    float calcV = 0f;
		
		    for ( int i = 0; i < points; i ++ )
		    {
			    pX[i] = Mathf.Sin(calcH * Mathf.Deg2Rad); 
			    pZ[i] = Mathf.Cos(calcH * Mathf.Deg2Rad);
			    pY[i] = Mathf.Cos(calcV * Mathf.Deg2Rad); 
			    pR[i] = Mathf.Sin(calcV * Mathf.Deg2Rad); 
			    calcH += 360f / (float)segments;
			    calcV += 180f / (float)segments;
		    }

		    Vector3[] vertices = new Vector3[points * (points + 1)];
		    int ind = 0;
		
		    // Y-offset is half the height minus the diameter
		    float yOff = (height - (radius * 2f)) * 0.5f;
		    if (yOff < 0)
			    yOff = 0;
		
		    // Top Hemisphere
		    int top = Mathf.CeilToInt((float)points * 0.5f);
		    for (int y = 0; y < top; y++) 
		    {
			    for (int x = 0; x < points; x++) 
			    {
				    vertices[ind] = new Vector3(pX[x] * pR[y], pY[y], pZ[x] * pR[y]) * radius;
				    vertices[ind].y = yOff + vertices[ind].y;
				    ind ++;
			    }
		    }
		
		    // Bottom Hemisphere
		    int btm = Mathf.FloorToInt((float)points * 0.5f);
		
		    for (int y = btm; y < points; y++) 
		    {
			    for (int x = 0; x < points; x++) 
			    {
				    vertices[ind] = new Vector3(pX[x] * pR[y], pY[y], pZ[x] * pR[y]) * radius;
				    vertices[ind].y = -yOff + vertices[ind].y;
				    ind ++;
			    }
		    }
		    // - Triangles -
		    int[] triangles = new int[(segments * (segments + 1) * 2 * 3 )];
		
		    for (int y = 0, t = 0; y < segments + 1; y ++) 
		    {
			    for (int x = 0; x < segments; x ++, t += 6) 
			    {
				    triangles[t + 0] = ((y + 0) * (segments + 1)) + x + 0;
				    triangles[t + 1] = ((y + 1) * (segments + 1)) + x + 0;
				    triangles[t + 2] = ((y + 1) * (segments + 1)) + x + 1;
				    triangles[t + 3] = ((y + 0) * (segments + 1)) + x + 1;
				    triangles[t + 4] = ((y + 0) * (segments + 1)) + x + 0;
				    triangles[t + 5] = ((y + 1) * (segments + 1)) + x + 1;
			    }
		    }
            // We could pool meshes but who cares.	
            Mesh mesh = new Mesh();
		    mesh.vertices = vertices;
		    mesh.triangles = triangles;
		    mesh.RecalculateBounds();
		    mesh.RecalculateNormals();
		    mesh.Optimize();
            return mesh;
	    }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GLDrawBoxCollider(in BoxCollider box)
        {
            Bounds lBounds = new Bounds(box.center, box.size);
            var go = box.gameObject;
            //Ugly? Yes. Do I give a shit? N-no.
            Cache.boxCachePoints[0] = go.transform.TransformPoint(new Vector3(lBounds.min.x, lBounds.min.y, lBounds.min.z));
            Cache.boxCachePoints[1] = go.transform.TransformPoint(new Vector3(lBounds.min.x, lBounds.min.y, lBounds.max.z));
            Cache.boxCachePoints[2] = Cache.boxCachePoints[1];
            Cache.boxCachePoints[3] = go.transform.TransformPoint(new Vector3(lBounds.min.x, lBounds.max.y, lBounds.max.z));
            Cache.boxCachePoints[4] = Cache.boxCachePoints[3];
            Cache.boxCachePoints[5] = go.transform.TransformPoint(new Vector3(lBounds.min.x, lBounds.max.y, lBounds.min.z));
            Cache.boxCachePoints[6] = Cache.boxCachePoints[5];
            Cache.boxCachePoints[7] = Cache.boxCachePoints[0];
            Cache.boxCachePoints[8] = Cache.boxCachePoints[0];
            Cache.boxCachePoints[9] = go.transform.TransformPoint(new Vector3(lBounds.max.x, lBounds.min.y, lBounds.min.z));
            Cache.boxCachePoints[10] = Cache.boxCachePoints[9];
            Cache.boxCachePoints[11] = go.transform.TransformPoint(new Vector3(lBounds.max.x, lBounds.max.y, lBounds.min.z));
            Cache.boxCachePoints[12] = Cache.boxCachePoints[11];
            Cache.boxCachePoints[13] = Cache.boxCachePoints[5];
            Cache.boxCachePoints[14] = Cache.boxCachePoints[3];
            Cache.boxCachePoints[15] = go.transform.TransformPoint(new Vector3(lBounds.max.x, lBounds.max.y, lBounds.max.z));
            Cache.boxCachePoints[16] = Cache.boxCachePoints[15];
            Cache.boxCachePoints[17] = Cache.boxCachePoints[11];
            Cache.boxCachePoints[18] = Cache.boxCachePoints[9];
            Cache.boxCachePoints[19] = go.transform.TransformPoint(new Vector3(lBounds.max.x, lBounds.min.y, lBounds.max.z));
            Cache.boxCachePoints[20] = Cache.boxCachePoints[19];
            Cache.boxCachePoints[21] = Cache.boxCachePoints[15];
            Cache.boxCachePoints[22] = Cache.boxCachePoints[1];
            Cache.boxCachePoints[23] = Cache.boxCachePoints[19];

            for (int j = 0; j < 23; j++)
            {
                GL.Vertex(Cache.boxCachePoints[j]);
                GL.Vertex(Cache.boxCachePoints[++j]);
            }
        }

        private void OnPostRenderCallback(Camera cam)
        {
           if (!CanRender() || cam != Cache.mainCam)
                return;

            // We draw box colliders via GL.Lines because we need quad-like representation
            // and shaders are hard --barbie
            if (Options.collViewMode[(int)ViewMode.Box] == 1 && Cache.boxData.Count > 1)
            {
                GL.PushMatrix();
                GLinesMaterial.SetPass(0);
                GL.MultMatrix(Matrix4x4.identity);
                GL.Begin(GL.LINES);
                for (int i = Cache.boxData.Count - 1; i >= 0; i--)
                {   
                    Color line = Color.red;
                    var box = Cache.boxData[i];
                    if (box == null)
                    {
                        Cache.boxData.RemoveAt(i);
                        continue;
                    }
                    if (box.isTrigger)
                        line = Color.green;
                    if (!box.enabled)
                        line = new Color(0.5f, 0.5f, 0.5f, 0.7f);
                    GL.Color(line);
                    GLDrawBoxCollider(box);
                }
                GL.End();
                GL.PopMatrix();
            }
            // Path trail
            if (Options.posHistGraphEnabled && DData.posHistSamples.Count > 2)
            {
                GL.PushMatrix();
                GLinesMaterial.SetPass(0);
                GL.MultMatrix(Matrix4x4.identity);
                GL.Begin(GL.LINE_STRIP);
                GL.Color(Color.yellow);
                foreach (var pos in DData.posHistSamples)
                {
                    GL.Vertex(pos + new Vector3(0,2.5f,0));
                    GL.Vertex(pos + new Vector3(0,-2f,0));
                    GL.Vertex(pos + new Vector3(0,2.5f,0));
                }
                GL.End();
                GL.PopMatrix();
            }
        }

        //
        // Helpers
        //
        public void SetMatrix(float x, float y) => guiMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(x / 1920f, y / 1080f, 1f));

        // Manual scaling for other resolution off base 1920x1080
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float FitX(float orig) => (orig * Screen.width) / 1920f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float FitY(float orig) => (orig * Screen.height) / 1080f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CanRender() => PlayerGlobal.instance != null && DData != null && DData.curActiveScene != "TitleScreen";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Toggle(ref bool value) { value = !value; return value; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DrawVelGraph()
        {
            if (DData.velSamples.Count < Options.maxGraphSamples)
                return;

            GL.PushMatrix();
            GLinesMaterial.SetPass(0);
            GL.LoadPixelMatrix();
            GL.Begin(GL.QUADS);
            GL.Color(new Color(0f, 0f, 0f, 0.45f)); //black
            var leftX  = FitX(Options.graphPosGL.x);
            var botY   = FitY(Options.graphPosGL.y);
            var rightX = FitX(Options.graphPosGL.x + PluginOptions.graphWidth);
            var topY   = FitY(Options.graphPosGL.y + PluginOptions.graphHeight);
            GL.Vertex3(leftX, botY, 0);
            GL.Vertex3(rightX, botY, 0);
            GL.Vertex3(rightX, topY, 0);
            GL.Vertex3(leftX, topY, 0);
            GL.End();
            GL.Begin(GL.LINES);
            GL.Color(new Color(1f, 1f, 1f, 0.4f)); // grid lines, gray
            Vector3 start = Options.graphPosGL;
            var step = (Options.frameSampleSize * 1000f);
            // Scale Grid
            for (int i = 1; i < 10; i++)
            {
                GL.Vertex(new Vector3(FitX(start.x), FitY(start.y + (i * 10f)), 0f));
                GL.Vertex(new Vector3(FitX(start.x+330f), FitY(start.y + (i * 10f)), 0f));
            }
            // Plot line
            GL.Color(Color.cyan);
            float vel = Mathf.Min(DData.velSamples.Peek(), 100f);
            start = new Vector3(FitX(start.x), FitY(start.y + vel), 0);
            var c = 0;
            foreach (var speed in DData.velSamples)
            {
                vel = Mathf.Min(speed, 100f);
                var end = new Vector3(FitX(Options.graphPosGL.x + (step * (c+1))), FitY(Options.graphPosGL.y + vel), 0);
                for (int j = 0; j < Options.velLineWidth; j++)
                {
                    GL.Vertex(start+new Vector3(0,0+j,0));
                    GL.Vertex(end+new Vector3(0,0+j,0));
                }
                start = end;
                c++;
            }
            GL.End();
            GL.PopMatrix();
        }

        private void PopulateDData()
		{
            // Switch would be faster but we dont want to fix it manually each update if new value added
            DData.dmgTypes = new Dictionary<int, string>(13);
            foreach (DamageType val in Enum.GetValues(typeof(DamageType)))
                DData.dmgTypes.Add((int)val, val.ToString());
            DData.damageables = new List<DamageableRef>(30);
            DData.velSamples = new Queue<float>(Options.maxGraphSamples);
            DData.posHistSamples = new Queue<Vector3>(Options.maxPosHistSamles);
            DData.lastSave = Time.realtimeSinceStartup;
            var sceneNumber = SceneManager.sceneCountInBuildSettings;
            List<string> scenes = new List<string>(20);
            for (int i = 0; i < sceneNumber; i++)
            {
                string scene = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
                if (scene[0] != '_')
                    scenes.Add(scene);
            }
            DData.allScenes = scenes.OrderByDescending(x => x).ToArray();
		}

        private void FixedUpdate() => SamplePositionHistory();

        private void Update()
        {
            ProcessInput();
            TickLogic();
            SampleData();
            DrawLine();
            DrawMeshColliders();
            listenForKeys();
        }

        // We could rewrite it to be generic but who cares
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DrawMeshColliders()
        {
            if (!CanRender())
                return;

            if (Options.collViewMode[(int)ViewMode.Mesh] == 1 && Cache.meshData.Count > 0)
            {
                Cache.matProps.SetColor("_WireColor", Color.magenta);
                for (int i = Cache.meshData.Count - 1; i >= 0; i--)
                {
                    var mesh = Cache.meshData[i];
                    if (!mesh)
                    {
                        Cache.meshData.RemoveAt(i);
                        continue;
                    }
                    if (!mesh.enabled || !mesh.gameObject.activeSelf)
                        continue;
                    var matrix = Matrix4x4.TRS(mesh.transform.position, mesh.transform.rotation, mesh.transform.lossyScale);
                    Graphics.DrawMesh(mesh.sharedMesh, matrix, MeshMaterial, 32, Cache.mainCam, 0, Cache.matProps);
                }
            }

            if (Options.collViewMode[(int)ViewMode.Capsule] == 1 && Cache.capsuleData.Count > 0)
            {
                Cache.matProps.SetColor("_WireColor", Color.yellow);
                for (int i = Cache.capsuleData.Count - 1; i >= 0; i--)
                {
                    var capsule = Cache.capsuleData[i];
                    if (capsule.collider == null)
                    {
                        Cache.capsuleData.RemoveAt(i);
                        continue;
                    }
                    if (!capsule.collider.enabled || !capsule.collider.gameObject.activeSelf)
                        continue;
                    var matrix = Matrix4x4.TRS(capsule.collider.bounds.center, capsule.collider.transform.rotation, Vector3.one);
                    Graphics.DrawMesh(capsule.mesh, matrix, MeshMaterial, 32, Cache.mainCam, 0, Cache.matProps);
                }
            }

            if (Options.collViewMode[(int)ViewMode.Sphere] == 1 && Cache.sphereData.Count > 0)
            {
                Cache.matProps.SetColor("_WireColor", Color.cyan);
                for (int i = Cache.sphereData.Count - 1; i >= 0; i--)
                {
                    var sphere = Cache.sphereData[i];
                    if (sphere == null)
                    {
                        Cache.capsuleData.RemoveAt(i);
                        continue;
                    }
                    if (!sphere.enabled || !sphere.gameObject.activeSelf)
                        continue;
                    var matrix = Matrix4x4.TRS(sphere.bounds.center, sphere.transform.rotation, sphere.transform.lossyScale * sphere.radius * 2);
                    Graphics.DrawMesh(Cache.sphereMesh, matrix, MeshMaterial, 32, Cache.mainCam, 0, Cache.matProps);
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SamplePositionHistory()
        {
            if (PlayerGlobal.instance != null && Options.posHistGraphEnabled)
            {
                DData.lastPosHisSampleTime += Time.fixedDeltaTime;
                var currPos = PlayerGlobal.instance.transform.position;
                if (currPos != DData.lastPosHistSample && DData.lastPosHisSampleTime > Time.fixedDeltaTime + 0.05f)
                {
                    DData.lastPosHistSample = currPos;
                    DData.lastPosHisSampleTime = 0;
                    if (DData.posHistSamples.Count >= Options.maxPosHistSamles)
                        _ = DData.posHistSamples.Dequeue();
                    DData.posHistSamples.Enqueue(currPos);
                }
            }
        }

        public void ClearAllCache()
        {
            Cache.ClearColliderCache();
            DData.damageables.Clear();
            DData.velSamples.Clear();
            DData.posHistSamples.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DrawLine()
        {
            if (!Cache.lineRenderer || !Cache.lineRenderer.enabled) return;
            var playerPos = Cache.lineRenderer.transform.position + new Vector3(0f,2.5f,0f);
            Vector3 end = playerPos - (Cache.lineRenderer.transform.forward * -50f);
            Cache.lineCache[0] = playerPos;
            Cache.lineCache[1] = end;
			Cache.lineRenderer.SetPositions(Cache.lineCache);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TickLogic()
        {
            tickFrameTime += Time.deltaTime;
            if (tickFrameTime < 0.2f) return;
            tickFrameTime = 0;
            if (Options.autoHeal)
            {
                if (!DData.dmgObject)
                {
                    return;
                }
                if (DData.dmgObject.GetCurrentHealth() < DData.dmgObject.maxHealth && DData.dmgObject.GetCurrentHealth() > 0)
                {
                    DData.dmgObject.HealToFull();
                }
            }
            if (infMagic)
            {
                UIArrowChargeBar.instance.GainCharge(8);
            }
        }

        private void listenForKeys()
        {
            if (!Input.anyKeyDown || listeningForKey.Length == 0) { return; }
            foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
            {
                if (k != KeyCode.None && Input.GetKeyDown(k))
                {
                    Log.LogWarning(k.ToString());
                    foundKey = k;
                    return;
                }
            }
        }
            

        private void OnGUI()
        {
            if (!CanRender())
                return;

            Matrix4x4 matrix = GUI.matrix;
            //avoid GIUStyle at all cost - lotsa garbage
            var lFont = GUI.skin.label.fontSize;
            var lStyle = GUI.skin.label.fontStyle;
            var lAlign = GUI.skin.label.alignment;
            var lPadding = GUI.skin.label.padding;
            var bFont = GUI.skin.box.fontSize;
            var bStyle = GUI.skin.box.fontStyle;
            var bAlign = GUI.skin.box.alignment;
            var bPadding = GUI.skin.box.padding;
            var oldWrap = GUI.skin.button.wordWrap;
            GUI.matrix = guiMatrix;

            if (Options.menuEnabled && Event.current.type == EventType.Repaint)
            {
                GUI.skin.box.fontSize = 20;
                GUI.skin.box.fontStyle = FontStyle.Bold;
                GUI.skin.box.alignment = TextAnchor.UpperLeft;
                var box = new Rect(1920f-10f-PluginOptions.guiInfoWidth, 10f, PluginOptions.guiInfoWidth, 550f);
                GUI.Box(box, guiOutputStr);
                if (Options.velGraphEnabled)
                {
                    GUI.skin.label.fontSize = 12;
                    GUI.skin.label.alignment = TextAnchor.UpperRight;
                    GUI.skin.label.padding = graphOffset;
                    GUI.Label(new Rect(1155f, 4f, 30f, 28f), "100");
                    GUI.Label(new Rect(1155f, 53f, 30f, 28f), "50");
                    GUI.Label(new Rect(1155f, 100f, 30f, 28f), "0");
                    DrawVelGraph();
                }
            }

            if (Options.sceneMenuEnabled)
            {
                Cursor.visible = true;
                GUI.skin.button.fontSize = 18;
                GUI.skin.button.fontStyle = FontStyle.Bold;
                var box = new Rect(10f, 10f, 338f, 494f);
                GUI.Box(box, string.Empty);
                var c = DData.allScenes.Length;
                SMscroll = GUI.BeginScrollView(new Rect(box.x + 2f, box.y + 2f, 340f, 458f), SMscroll, new Rect(box.x + 2f, box.y + 2f, 330f, c * 30f), false, true);
                sceneIndex = GUI.SelectionGrid(new Rect(box.x + 3f, box.y + 3f, 330f, c * 30f), sceneIndex, DData.allScenes, 1);
                GUI.EndScrollView();
                if (GUI.Button(new Rect(12f, 470f, 100f, 30f), "<color=yellow>Travel To</color>"))
                {
                    DoorTrigger.currentTargetDoor = "_debug";
                    GameSceneManager.LoadSceneFadeOut(DData.allScenes[sceneIndex], 0.1f, true);
                    Options.sceneMenuEnabled = false;
                    input(PlayerGlobal.instance).PauseInput(Options.sceneMenuEnabled);
                }
            }

            if (bindMenuOpen)
            {
                UI_Control.HideUI();
                Cursor.visible = true;
                GUIMenus.BindMenu.OnGUI();
            }

            if (UIMenuPauseController.instance.IsPaused())
            {
                GUI.skin.button.fontSize = 18;
                GUI.skin.button.fontStyle = FontStyle.Normal;
                var box = new Rect(100f, 500f, 120f, 170f);
                GUI.Box(box, string.Empty);
                statIndex = GUI.SelectionGrid(new Rect(box.x + 3f, box.y + 3f, 90f, 120f), statIndex, new string[] { "Strength", "Dexterity", "Haste", "Magic" }, 1);
                var statName = statNames[statIndex];
                if (GUI.Button(new Rect(box.x + box.width / 2 - 28f, box.yMax - 3f - 35f, 25f, 30f), "<color=blue>+</color>"))
                {
                    if (Inventory.instance.GetItemCount(statName) < 5 || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)) { Inventory.instance.SetItemCount(statName, Inventory.instance.GetItemCount(statName) + 1); }
                    else { Inventory.instance.SetItemCount(statName, 0); }
                    Inventory.instance.AddItem("stat_melee", 0);
                    Inventory.instance.AddItem("stat_dexterity", 0);
                    Inventory.instance.AddItem("stat_haste", 0);
                    Inventory.instance.AddItem("stat_magic", 0);
                }
                if (GUI.Button(new Rect(box.x + box.width / 2 + 3f, box.yMax - 3f - 35f, 25f, 30f), "<color=blue>-</color>"))
                {
                    if (Inventory.instance.GetItemCount(statName) > 0 || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)) { Inventory.instance.SetItemCount(statName, Inventory.instance.GetItemCount(statName) - 1); }       
                    else { Inventory.instance.SetItemCount(statName, 5); }
                    Inventory.instance.AddItem("stat_melee", 0);            
                    Inventory.instance.AddItem("stat_dexterity", 0);            
                    Inventory.instance.AddItem("stat_haste", 0);            
                    Inventory.instance.AddItem("stat_magic", 0);            
                }
                GUI.Label(new Rect(box.xMax - 15f, box.y + 3f, 10f, 30f), Inventory.instance.GetItemCount(statNames[0]).ToString());
                GUI.Label(new Rect(box.xMax - 15f, box.y + 33f, 10f, 30f), Inventory.instance.GetItemCount(statNames[1]).ToString());
                GUI.Label(new Rect(box.xMax - 15f, box.y + 66f, 10f, 30f), Inventory.instance.GetItemCount(statNames[2]).ToString());
                GUI.Label(new Rect(box.xMax - 15f, box.y + 99f, 10f, 30f), Inventory.instance.GetItemCount(statNames[3]).ToString());
            }

            GUI.matrix = matrix;
            if (Options.hpEnabled && Event.current.type == EventType.Repaint && Cache.mainCam != null && DData.damageables.Count > 0)
            {
                for (int i = DData.damageables.Count - 1; i >= 0; i--)
                {
                    var curRef = DData.damageables[i];
                    var current = curRef.instance;
                    if (!current)
                    {
                        DData.damageables[i].stringHealth = null;
                        DData.damageables[i] = null;
                        DData.damageables.RemoveAt(i);
                        continue;
                    }
                    var curhealth = currentHealth(current);
                    if (curhealth > 989f || curhealth < 0.09f || !current.gameObject.activeInHierarchy || !current.gameObject.activeSelf)
                        continue;
                    Vector2 vector = Cache.mainCam.WorldToScreenPoint(current.transform.position);
                    if (vector.x > 0f && vector.x < (float)Screen.width && vector.y > 0f && vector.y < (float)Screen.height)
                    {
                        if (Mathf.Abs(Mathf.Abs(curRef.trackedHealth) - Mathf.Abs(curhealth)) > 0.05f)
                        {
                            curRef.trackedHealth = curhealth;
                            curRef.stringHealth = curhealth.ToString("N1");
                        }
                        GUI.skin.box.fontSize = 20;
                        GUI.skin.box.fontStyle = FontStyle.Bold;
                        GUI.skin.box.alignment = TextAnchor.MiddleCenter;
                        float y = (float)Screen.height - vector.y;
                        GUI.Box(new Rect(vector.x, y, 60f, 22f), curRef.stringHealth);
                    }
                }
            }
             GUI.skin.label.fontSize = lFont;
             GUI.skin.label.fontStyle = lStyle;
             GUI.skin.label.alignment = lAlign;
             GUI.skin.label.padding = lPadding;
             GUI.skin.box.fontSize = bFont;
             GUI.skin.box.fontStyle = bStyle;
             GUI.skin.box.alignment = bAlign;
             GUI.skin.box.padding = bPadding;
             GUI.skin.button.wordWrap = oldWrap;
        }

        private void SampleData()
        {
            if (CanRender())
            {
                builder.Append("[DEBUG INFO]  <size=16>(").Append(VERSION).Append(")</size>");
                if (Options.menuEnabled)
                {
                    var meleeMult = Inventory.GetMeleeDamageModifier();
                    var magicMod = Inventory.GetMagicDamageModifier();
                    var speed = Inventory.GetSpeedModifier();
                    builder.Append("\nScene: ").Append(DData.curActiveScene);
                    builder.Append("\nMeleeMult: ").Append(meleeMult, 2);
                    builder.Append("\nMeleeRange: ").Append(Inventory.GetMeleeRangeModifier(), 2);
                    builder.Append("\nMagicMult: ").Append(magicMod, 2);
                    builder.Append("\nDexterityMult: ").Append(Inventory.GetDexterityModifier(), 2);
                    builder.Append("\nRollSpeedMult: ").Append(Inventory.GetRollSpeedModifier(), 2);
                    builder.Append("\nSpeedMult: ").Append(speed, 2);

                    if (DData.movCtrlObject != null)
                        builder.Append("\nMaxSpeed: ").Append(DData.movCtrlObject.maxSpeed * PlayerGlobal.instance.speedMultiplier * speed, 2);
                    builder.Append("\n-");

                    if (DData.wpnRefs != null)
                    {
                        SampleWeapon(builder, DData.wpnRefs.lightAttack, meleeMult, "\nLight: ");
                        SampleWeapon(builder, DData.wpnRefs.heavyAttack, meleeMult, "\nHeavy: ");
                        SampleWeapon(builder, DData.wpnRefs.rollAttack, meleeMult, "\nRoll: ");
                        SampleWeapon(builder, DData.wpnRefs.hookshotAttack, meleeMult, "\nHook: ");
                    }
                    if (DData.magicRefs != null)
                    {
                        var inst = DData.magicRefs.GetArrowInstance();
                        var dmg = 0f;
                        var type = DData.magicRefs.GetType().Name;
                        if (inst)
                        {
                            dmg = inst is BombArrow ? 3f : inst.damage;
                            type = DData.dmgTypes[(int)inst.damageType];
                        }
                        builder.Append("\nMagic: ").Append(magicMod * dmg, 2).Append(" (").Append(dmg, 2).Append("*")
                                                .Append(magicMod).Append(")").Append(" [").Append(type).Append("]");
                    }
                    builder.Append("\nPlungingDmg: ").Append(PlayerGlobal.instance.GetPlungingDamage(), 1);
                    builder.Append("\n-");
                    builder.Append("\nLast Damage: \n").Append("[ <color=yellow>").Append(DData.lastDamage.dmg, 2).Append("</color> | ")
                           .Append(DData.lastDamage.poiseDmg, 2).Append(" | ").Append(DData.dmgTypes[(int)DData.lastDamage.type]).Append(" ]");

                    var diff = Time.realtimeSinceStartup - DData.lastSave;
                    string color = diff < 5f ? "\n<color=lime>" : "\n<color=white>";
                    builder.Append(color).Append("Last save: ").Append(diff, 0, 5, ' ').Append("s ago</color>");

                    TimeSpan span = TimeSpan.FromSeconds(GameTimeTracker.instance.GetTime());
                    builder.Append("\nGameTime: ").Append(span.Hours, 3, ' ').Append(":").Append(span.Minutes, 2, '0').Append(":").Append(span.Seconds, 2, '0');

                    var pos = PlayerGlobal.instance.transform.localPosition; // slight diif in Y with global .position
                    builder.Append("\nPos: ").Append("x: ").Append(pos.x, 2, 2, ' ').Append(" y: ").Append(pos.y, 2, 2, ' ').Append(" z: ").Append(pos.z, 2, 2, ' ');

                    if (DData.plrRBody != null)
                    {
                        var vel = DData.plrRBody.velocity.magnitude;
                        SampleVelocity(vel);
                        builder.Append("\nVelocity: ").Append(vel, 2).Append("  Peak: ").Append(DData.lastVelocity, 2);
                    }
                }
                guiOutputStr = builder.Finalize();
            }
        }
        
        public static bool CheckIfPressed(String name)
        {
            if (bufferedActions.Contains(name)) { bufferedActions.Remove(name); return true; }
            var raw = featureBinds[name];
            if (!(raw.GetType() == typeof(Bind)))
            {
                return false;
            }
            var b = (Bind)raw;
            if (!b.allowExtraModifiers) { return CheckIfPressedNoExtras(b); }
            var result = Input.GetKeyDown(b.keycode);
            if (b.modifiers != "")
            {
                if (b.modifiers.Contains('s') && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) { result = false; }
                if (b.modifiers.Contains('c') && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) { result = false; }
                if (b.modifiers.Contains('a') && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) { result = false; }
            }
            return result;
        }

        public static bool CheckIfPressedNoExtras(Bind b)
        {
            return Input.GetKeyDown(b.keycode) && !(b.modifiers.Contains('s') ^ (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) && !(b.modifiers.Contains('c') ^ (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) && !(b.modifiers.Contains('a') ^ (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)));
        }

        public static bool CheckIfHeld(String name)
        {
            if (bufferedActions.Contains(name)) { bufferedActions.Remove(name); return true; }
            var raw = featureBinds[name];
            if (!(raw.GetType() == typeof(Bind)))
            {
                return false;
            }
            var b = (Bind)raw;
            var result = Input.GetKey(b.keycode);
            if (b.modifiers != "")
            {
                if (b.modifiers.Contains('s') && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) { result = false; }
                if (b.modifiers.Contains('c') && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) { result = false; }
                if (b.modifiers.Contains('a') && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) { result = false; }
            }
            return result;
        }

        private bool CheckIfModifierHeld(String name)
        {
            var raw = featureBinds[name];
            if (!(raw.GetType() == typeof(Bind)))
            {
                return false;
            }
            var b = (Bind)raw;
            return (!b.modifiers.Contains("s") || (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) && (!b.modifiers.Contains("c") || (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) && (!b.modifiers.Contains("a") || (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)));
        }

        private void ProcessInput()
        {
            if (!CanRender()) return;

            if (CheckIfPressed("Open binding menu"))
            {
                input(PlayerGlobal.instance).PauseInput(Toggle(ref bindMenuOpen));
                Options.sceneMenuEnabled = false;
                if (!bindMenuOpen) { UI_Control.ShowUI(); }
            }

            if (CheckIfPressed("Info menu"))
            {
                Toggle(ref Options.menuEnabled);
                DData.lastVelocity = 0;
            }

            if (CheckIfPressed("Show hp"))
            {
                DData.damageables.Clear();
                if (Toggle(ref Options.hpEnabled))
                {
                    var foundDmgbls = FindObjectsOfType<DamageableCharacter>();
                    for (int i = 0; i < foundDmgbls.Length; i++)
                        AddDamageable(foundDmgbls[i]);
                }
            }
            if (CheckIfPressed("Warp menu"))
            {
                input(PlayerGlobal.instance).PauseInput(Toggle(ref Options.sceneMenuEnabled));
                bindMenuOpen = false;
            }
            if (CheckIfPressed("Heal to full"))
            {
                DData.dmgObject.HealToFull();
            }

            if (CheckIfPressed("Auto heal"))
            {
                Toggle(ref Options.autoHeal);
            }

            if (CheckIfPressed("Boss reset"))
            {
                foreach (var boss_str in DData.bossKeys)
                    GameSave.GetSaveData().SetKeyState(boss_str, false, false);
                GameSave.currentSave.Save();
            }
            if (CheckIfPressed("Boss reset with cuts"))
            {
                foreach (var boss_str in DData.bossesIntroKeys)
                    GameSave.GetSaveData().SetKeyState(boss_str, false, false);
                GameSave.currentSave.Save();
            }
            if (CheckIfPressed("Give soul"))
            {
                Inventory.instance.AddItem("currency", 100, false);
            }
            if (CheckIfPressed("Unlock weapons"))
            {
                Inventory.instance.AddItem("daggers", 1, false);
                Inventory.instance.AddItem("hammer", 1, false);
                Inventory.instance.AddItem("sword_heavy", 1, false);
                Inventory.instance.AddItem("umbrella", 1, false);
            }
            if (CheckIfPressed("Unlock spells"))
            {
                WeaponSwitcher.instance.UnlockBombs();
                WeaponSwitcher.instance.UnlockFire();
                WeaponSwitcher.instance.UnlockHooskhot();
            }
            if (CheckIfPressed("Save pos"))
            {
                savePosDic[DData.curActiveScene.GetHashCode()] = PlayerGlobal.instance.transform.position;
            }
            if (CheckIfPressed("Load pos"))
            {
                if (savePosDic.ContainsKey(DData.curActiveScene.GetHashCode())) 
                {
                    PlayerGlobal.instance.SetPosition(savePosDic[DData.curActiveScene.GetHashCode()], false, false);
                }
            }

            if (CheckIfPressed("Load gpos"))
            {
				if (DData.lastCheckPoint.pos != null)
				{
                	PlayerGlobal.instance.SetPosition(DData.lastCheckPoint.pos, false, false);
				}
            }
			if (CheckIfPressed("Save gpos"))
			{
				DData.lastCheckPoint.pos = PlayerGlobal.instance.transform.position;
			}

            if (CheckIfPressed("Show colliders"))
            {
                Options.collViewMode[Options.cvmPos] = (byte)(1 - Options.collViewMode[Options.cvmPos]);
                Options.cvmPos = ++Options.cvmPos % Options.collViewMode.Length;
            }
            if (CheckIfPressed("Load visible colliders"))
            {
                FullSweep(PlayerGlobal.instance.transform);
            }

            if (CheckIfPressed("Freecam") && Cache.cineBrain != null)
            {
                Cache.cineBrain.enabled = !Cache.cineBrain.enabled;
                Options.freeCamEnabled = !Cache.cineBrain.enabled;
            }

            if (CheckIfPressed("Pos history"))
            {
                Toggle(ref Options.posHistGraphEnabled);
                DData.posHistSamples.Clear();
            }

            if (CheckIfPressed("Velocity graph"))
                Toggle(ref Options.velGraphEnabled);

            if (CheckIfPressed("Timescale down"))
            {
                timescale = Mathf.Clamp(timescale - 0.25f, 0f, 5f);
                Time.timeScale = timescale;
            }
            if (CheckIfPressed("Timescale up"))
            {
                timescale = Mathf.Clamp(timescale + 0.25f, 0f, 5f);
                Time.timeScale = timescale;
            }
            if (CheckIfPressed("Reset timescale"))
            {
                timescale = 1f;
                Time.timeScale = timescale;
            }

            if (!wasSlow && Time.timeScale <= 0.24f && !UIMenuPauseController.instance.IsPaused() && !GameSceneManager.instance.IsLoading())
            {
                wasSlow = true;
            }
            if (wasSlow && Time.timeScale > 0.24)
            {
                wasSlow = false;
                Time.timeScale = timescale;
            }

            if (!paused && UIMenuPauseController.instance.IsPaused())
            {
                paused = true;
            }
            if (paused && !UIMenuPauseController.instance.IsPaused())
            {
                paused = false;
                if (!wasSlow) { Time.timeScale = timescale; }
            }
            if (!UIMenuPauseController.instance.IsPaused() && Time.timeScale != timescale && !wasSlow)
            {
                timescale = Time.timeScale;
            }        

            Buttons.PauseInput(CheckIfModifierHeld("Mouse tele"));
            if (CheckIfPressed("Mouse tele") && PlayerGlobal.instance != null && !PlayerGlobal.instance.InputPaused())
                SpawnAtCursor();

            if (CheckIfHeld("Rotate cam right") && CameraRotationControl.instance)
            {
                var currAngle = angle(CameraRotationControl.instance) + 1.5f;
                if (currAngle > 360) { currAngle -= 360; }
                CameraRotationControl.instance.Rotate(currAngle, 1000);
                isTurning = true;
            }
            
            if (!CheckIfHeld("Rotate cam right") && CameraRotationControl.instance && isTurning)
            {
                isTurning = false;
                var currAngle = angle(CameraRotationControl.instance);
                CameraRotationControl.instance.Rotate(currAngle, 3);
            }


            if (CheckIfHeld("Rotate cam left") && CameraRotationControl.instance)
            {
                var currAngle = angle(CameraRotationControl.instance) - 1.5f;
                if (currAngle < 0) { currAngle += 360; }
                CameraRotationControl.instance.Rotate(currAngle, 1000);
                isTurning = true;
            }
            if (!CheckIfHeld("Rotate cam left") && CameraRotationControl.instance && isTurning)
            {
                isTurning = false;
                var currAngle = angle(CameraRotationControl.instance);
                CameraRotationControl.instance.Rotate(currAngle, 3);
            }

            if (CheckIfPressed("Reset cam") && CameraRotationControl.instance && !isTurning)
            {
                CameraRotationControl.instance.Rotate(0, 12);
                if (HasZoomed)
                {
                    FovZoom.instance.SetCurrentBaseZoom(baseZoom);
                    HasZoomed = false;
                }
            }

            if (CheckIfPressed("Instant textskip"))
            {
                skipcs = true;
            }
			else if (!CheckIfHeld("Instant textskip"))
            {
				skipcs = false;
            }

            if ((skipcs && !inputwaspaused) || inputwaspaused)
            {
                wasSlow = true;
                Time.timeScale = 20f;
                foreach (NPCCharacter i in Resources.FindObjectsOfTypeAll<NPCCharacter>())
                {
                    if (!i.IsFinished())
                    {
                        i.NextLine();
                    }
                }
                if (PlayerGlobal.instance.InputPaused())
                { 
                    inputwaspaused = true; 
                }
                if (inputwaspaused && !PlayerGlobal.instance.InputPaused())
                {
                    skipcs = false;
                    inputwaspaused = false;
                    Time.timeScale = timescale;
                }
            }

            if (CheckIfPressed("Toggle Timestop"))
            {
                if (PlayerGlobal.instance.IsInputPausedCutscene() || PlayerGlobal.instance.IsInputPausedTalk())
                {
                    PlayerGlobal.instance.UnPauseInput_Cutscene();
                }
                else
                {
                    PlayerGlobal.instance.PauseInput_Talk();
                    PlayerGlobal.instance.UnPauseInput();
                }
            }

            if (CheckIfPressed("Warp to selected"))
            {
                DoorTrigger.currentTargetDoor = "_debug";
                GameSceneManager.LoadSceneFadeOut(DData.allScenes[sceneIndex], 0.1f, true);
                Options.sceneMenuEnabled = false;
                input(PlayerGlobal.instance).PauseInput(Options.sceneMenuEnabled);
            }

            if (Options.freeCamEnabled && Cache.mainCam != null)
            {
                Options.freeLookConf.rotationX += Input.GetAxis("Mouse X") * Options.freeLookConf.cameraSensitivity * Time.deltaTime;
                Options.freeLookConf.rotationY += Input.GetAxis("Mouse Y") * Options.freeLookConf.cameraSensitivity * Time.deltaTime;
                Options.freeLookConf.rotationY = Mathf.Clamp(Options.freeLookConf.rotationY, -90, 90);

                float vAxis = 0f;
                float hAxis = 0f;
                float factor = 1;

                /*if (Input.GetKey(KeyCode.F))
                    hAxis = -1f;
                else if (Input.GetKey(KeyCode.H))
                    hAxis = 1f;
                if (Input.GetKey(KeyCode.T))
                    vAxis = 1f;
                else if (Input.GetKey(KeyCode.G))
                    vAxis = -1f;*/

                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    factor = Options.freeLookConf.fastMoveFactor;
                else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    factor = Options.freeLookConf.slowMoveFactor;

                Cache.mainCam.transform.position += Cache.mainCam.transform.forward * (Options.freeLookConf.normalMoveSpeed * factor) * vAxis * Time.deltaTime;
                Cache.mainCam.transform.position += Cache.mainCam.transform.right * (Options.freeLookConf.normalMoveSpeed * factor) * hAxis * Time.deltaTime;

                var currEulerAngles = Cache.mainCam.transform.localRotation;
                /*if (Input.GetKey(KeyCode.Home))
                    currEulerAngles.y += 2f * Time.deltaTime;
                if (Input.GetKey(KeyCode.End))
                    currEulerAngles.y -= 2f * Time.deltaTime; ;
                if (Input.GetKey(KeyCode.Delete))
                    currEulerAngles.x -= 2f * Time.deltaTime;
                if (Input.GetKey(KeyCode.PageDown))
                    currEulerAngles.x += 2f * Time.deltaTime;
                if (Input.GetKey(KeyCode.Insert))
                    currEulerAngles.z -= 2f * Time.deltaTime;
                if (Input.GetKey(KeyCode.PageUp))
                    currEulerAngles.z += 2f * Time.deltaTime;*/
                if (Options.freeCamMouse)
                {
                    Cache.mainCam.transform.localRotation = Quaternion.AngleAxis(Options.freeLookConf.rotationX, Vector3.up);
                    Cache.mainCam.transform.localRotation *= Quaternion.AngleAxis(Options.freeLookConf.rotationY, Vector3.left);
                }
                else
                {
                    Cache.mainCam.transform.localRotation = currEulerAngles;
                }
                if (CheckIfPressed("Zoom in")) { Cache.mainCam.fieldOfView -= 0.5f * Time.deltaTime; }
                if (CheckIfPressed("Zoom out")) { Cache.mainCam.fieldOfView += 0.5f * Time.deltaTime; }
                if (Input.GetKey(KeyCode.R)) { Cache.mainCam.transform.position += Cache.mainCam.transform.up * (Options.freeLookConf.climbSpeed * factor) * Time.deltaTime; }
                if (Input.GetKey(KeyCode.Y)) { Cache.mainCam.transform.position -= Cache.mainCam.transform.up * (Options.freeLookConf.climbSpeed * factor) * Time.deltaTime; }
            }

            if (CheckIfPressed("Zoom in") && !Options.freeCamEnabled && FovZoom.instance)
            {
                if (!HasZoomed)
                {
                    baseZoom = currentBaseFov(FovZoom.instance);
                    HasZoomed = true;
                }
                FovZoom.instance.SetCurrentBaseZoom(currentBaseFov(FovZoom.instance) - 2f);
            }

            if (CheckIfPressed("Zoom out") && !Options.freeCamEnabled && FovZoom.instance)
            {
                if (!HasZoomed)
                {
                    baseZoom = currentBaseFov(FovZoom.instance);
                    HasZoomed = true;
                }
                FovZoom.instance.SetCurrentBaseZoom(currentBaseFov(FovZoom.instance) + 2f);
            }

            if (CheckIfPressed("Toggle noclip"))
            {
                movementControl = FindObjectOfType<PlayerMovementControl>();
                if (NoClip)
                {
                    movementControl.SetGravity(2);
                    foreach (SphereCollider i in FindObjectOfType<PlayerGlobal>().GetComponents<SphereCollider>())
                    {
                        i.enabled = true;
                    }
                    movementControl.slowDownMultiplier = oldSlowDown;
                    movementControl.acceleration = oldAcc;
                    movementControl.maxSpeed = oldSpeed;

                    NoClip = false;
                }
                else
                {
                    movementControl.SetGravity(0);
                    foreach (SphereCollider i in FindObjectOfType<PlayerGlobal>().GetComponents<SphereCollider>())
                    {
                        i.enabled = false;
                    }
                    oldSlowDown = movementControl.slowDownMultiplier;
                    oldAcc = movementControl.acceleration;
                    oldSpeed = movementControl.maxSpeed;
                    movementControl.acceleration = 100;
                    movementControl.maxSpeed = 30;

                    NoClip = true;
                }

            }
            if (CheckIfPressed("Tele up"))
            {
                FindObjectOfType<PlayerGlobal>().gameObject.transform.position += Vector3.up * 5;
                CameraMovementControl.instance.SetPosition(CameraMovementControl.instance.GetFocusPos() + Vector3.up * 5);
            }
            if (CheckIfPressed("Tele down"))
            {
                FindObjectOfType<PlayerGlobal>().gameObject.transform.position += Vector3.down * 5;
                CameraMovementControl.instance.SetPosition(CameraMovementControl.instance.GetFocusPos() + Vector3.down * 5);
            }

            if (Input.anyKey && NoClip) { movementControl.slowDownMultiplier = 1; }
            if (!Input.anyKey && NoClip) { movementControl.slowDownMultiplier = 0; }

            if (CheckIfPressed("Toggle night")) { LightNight.nightTime = !LightNight.nightTime; }
            if (CheckIfPressed("Inf magic")) { infMagic = !infMagic; }
            if (CheckIfPressed("Toggle godmode")) { PlayerGlobal.instance.gameObject.GetComponent<DamageablePlayer>().ToggleGodMode(); }

            if (CheckIfPressed("Reload file"))
            {
                GameSave.currentSave.Load();
                ScreenFade.storedFadeColor = Color.white;
                ScreenFade.instance.SetColor(Color.white, false);
                ScreenFade.instance.FadeOut(0.2f, true, null);
                ScreenFade.instance.LockFade();
                GameSceneManager.DontSaveNext();
                GameSceneManager.LoadSceneFadeOut(GameSave.GetSaveData().GetSpawnScene(), 0.2f, true);
                GameSceneManager.ReloadSaveOnLoad();
                //fix spell
                string id = (string)AccessTools.Field(typeof(WeaponSwitcher), "currentWeaponId").GetValue(WeaponSwitcher.instance);
                AccessTools.Field(typeof(WeaponSwitcher), "selected").SetValue(WeaponSwitcher.instance, spellDic[id]);
                justReloaded = true;
            }
            if (CheckIfPressed("Save file"))
            {
                if (!(GameSave.GetSaveData().GetSpawnScene() == SceneManager.GetActiveScene().name))
                { 
                    GameSave.GetSaveData().SetSpawnPoint(SceneManager.GetActiveScene().name, null);
                }      
                GameSave.GetSaveData().Save();     
            }

            if (CheckIfPressed("Get gp"))
            {
                var c = PlayerGlobal.instance.gameObject.GetComponent<InputLock>();
                if (c != null)
                {
                    c.enabled = false;
                    c.enabled = true;
                }
            }
        }

        public void AddDamageable(DamageableCharacter dmg)
        {
            if (!Options.hpEnabled) return;
            var curHP = currentHealth(dmg);
            if (curHP > 0.05 && !calledDie(dmg) && dmg.name.IndexOf("ragdoll", StringComparison.OrdinalIgnoreCase) < 0)
                DData.damageables.Add(new DamageableRef(dmg, curHP, curHP.ToString("N1")));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SampleVelocity(float speed)
        {
            if (speed > DData.lastVelocity)
                DData.lastVelocity = speed;
            DData.lastVelSampleTime += Time.deltaTime;
            if (DData.lastVelSampleTime > Options.frameSampleSize)
            {
                DData.lastVelSampleTime = 0;
                if (DData.velSamples.Count >= Options.maxGraphSamples)
                    _ = DData.velSamples.Dequeue();
                DData.velSamples.Enqueue(speed);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SampleWeapon(MutableString str, _Weapon wpn, float modifier, string name)
        {
            if (wpn == null) return;
            var finalDmg = wpn.baseDamage * modifier;
            var dmgType = DData.dmgTypes[(int)wpn.damageType];
            if (wpn is _ChargeWeapon chargWpn && chargedPower(chargWpn) > 0.999999)
            {
                finalDmg *= 1.5f;
                modifier *= 1.5f;
            }
	        str.Append(name).Append(finalDmg,2).Append(" (").Append(wpn.baseDamage, 2).Append("*").Append(modifier)
                   .Append(")").Append(" [").Append(dmgType).Append("]");
        }

        private void SpawnAtCursor()
        {
            RaycastHit hit;
            Ray ray = Cache.mainCam.ScreenPointToRay(Input.mousePosition);
            if (GameRoom.currentRoom == null)
            {
                if (Physics.Raycast(ray, out hit, 5000f, Globals.instance.solidLayers))
                    PlayerGlobal.instance.SetPosition(hit.point, false, false);
            }
            else
            {
                var c = Physics.RaycastNonAlloc(ray, Cache.hitsCache, 5000f, Globals.instance.solidLayers);
                Vector3 pos = Vector3.zero;
		        bool found = false;
		        float maxDistance = 999999f;
		        for (int i = 0; i < c; i++)
		        {
			        if (Cache.hitsCache[i].collider != null && GameRoom.currentRoom.PointInsideRoom(Cache.hitsCache[i].point))
			        {
				        found = true;
				        if (Cache.hitsCache[i].distance < maxDistance)
				        {
					        pos = Cache.hitsCache[i].point;
					        maxDistance = Cache.hitsCache[i].distance;
				        }
			        }
		        }
		        if (found)
		        {
			        GameRoom.currentRoom.debugForceExitRoom();
			        PlayerGlobal.instance.SetPosition(pos, false, false);
		        }
            }
        }

        private void OnEnable() => SceneManager.activeSceneChanged += OnSceneActivated;

        private void OnDisable() => SceneManager.activeSceneChanged -= OnSceneActivated;

        private void OnSceneActivated(Scene from, Scene to)
        {
            if (DData != null)
            {
                DData.lastActiveScene = from.name;
                DData.curActiveScene = to.name;
                ClearAllCache();
                if (to.name == "TitleScreen")
                {
                    Cache.mainCam = null;
                    Cache.cineBrain = null;
                    Cache.lineRenderer = null;
                    Cache.virtCam = null;
                }
            }
        }
    }
}
