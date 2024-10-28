// Cristian Pop - https://boxophobic.com/

using UnityEngine;
using Boxophobic.StyledGUI;

namespace TheVisualEngineLite
{
    [HelpURL("https://docs.google.com/document/d/145JOVlJ1tE-WODW45YoJ6Ixg23mFc56EnB_8Tbwloz8/edit#heading=h.czf8ud5bmaq2")]
    [ExecuteInEditMode]
    [AddComponentMenu("BOXOPHOBIC/The Visual Engine/TVE Manager Lite")]
    public class TVELiteManager : StyledMonoBehaviour
    {
        [StyledBanner(0.75f, 0.75f, 0.75f, "The Visual Engine Lite")]
        public bool styledBanner;

        [StyledMessage("Info", "When The Visual Engine Core manager is enabled in the scene, the Lite shaders will use the core settings instead of the current settings!", 0, 5)]
        public bool managerMessage = false;

        [StyledCategory("Quick Settings")]
        public bool windCat;

        [Tooltip("Controls the global wind power.")]
        [StyledRangeOptions("Motion Control", 0, 1, new string[] { "None", "Windy", "Strong" })]
        public float motionControl = 0.5f;

        [StyledCategory("Main Settings", 5, 10)]
        public bool sceneCat;

        [Tooltip("Sets the main light used as the sun in the scene.")]
        public Light mainLight;
        [Tooltip("Sets the main direction from a gameobject.")]
        public GameObject mainWind;

        [StyledCategory("Global Settings")]
        public bool defaultCat;

        public TVEGlobalCoatData globalCoatData = new();
        public TVEGlobalAtmoData globalAtmoData = new();
        public TVEGlobalGlowData globalGlowData = new();

        [StyledSpace(10)]
        public bool styledSpace0;

        void OnEnable()
        {
            Shader.SetGlobalFloat("TVE_IsEnabled", 1);
        }


        void OnDisable()
        {
            Shader.SetGlobalFloat("TVE_IsEnabled", 0);
        }

        void OnDestroy()
        {
            Shader.SetGlobalFloat("TVE_IsEnabled", 0);
        }

        void Start()
        {
            gameObject.name = "The Visual Engine Lite";

            if (mainLight == null)
            {
                SetGlobalLightingMainLight();
            }
        }

        void Update()
        {
            if (mainWind == null)
            {
                mainWind = gameObject;
            }

            gameObject.transform.eulerAngles = new Vector3(0, mainWind.transform.eulerAngles.y, 0);

            int isCoreManagerActive = Shader.GetGlobalInt("TVE_ManagerActive");

            if (isCoreManagerActive == 0)
            {
                SetGlobalShaderProperties();

                managerMessage = false;
            }
            else
            {
                managerMessage = true;
            }
        }

        void SetGlobalShaderProperties()
        {
            var windDirection = mainWind.transform.forward;

            if (mainLight != null)
            {
                var mainLightColor = mainLight.color.linear;
                var mainLightValue = new Color(mainLight.intensity, mainLight.intensity, mainLight.intensity).linear;
                var mainLightParams = new Color(mainLightColor.r, mainLightColor.g, mainLightColor.b, mainLightValue.r);

                Shader.SetGlobalVector("TVE_MainLightParams", mainLightParams);
                Shader.SetGlobalVector("TVE_MainLightDirection", Vector4.Normalize(-mainLight.transform.forward));
            }
            else
            {
                var mainLightParams = new Vector4(1, 1, 1, 1);

                Shader.SetGlobalVector("TVE_MainLightParams", mainLightParams);
                Shader.SetGlobalVector("TVE_MainLightDirection", new Vector4(0, 1, 0, 0));
            }

            Shader.SetGlobalVector("TVE_CoatParams", new Vector4(0, globalCoatData.detailIntensity, globalCoatData.layerIntensity, 0));
            Shader.SetGlobalVector("TVE_AtmoParams", new Vector4(0, 0, globalAtmoData.overlayIntensity, 0));

            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            {
                var color = globalGlowData.emissiveColor.linear;
                var alpha = globalGlowData.subsurfaceIntensity;
                Shader.SetGlobalVector("TVE_GlowParams", new Vector4(color.r, color.g, color.b, alpha));
            }
            else
            {
                var color = globalGlowData.emissiveColor;
                var alpha = globalGlowData.subsurfaceIntensity;
                Shader.SetGlobalVector("TVE_GlowParams", new Vector4(color.r, color.g, color.b, alpha));
            }

            var motionDirection = mainWind.transform.forward;

            var materialManagerActive = Shader.GetGlobalFloat("TVE_MaterialManagerActive");
            var motionControlEditor = Shader.GetGlobalFloat("TVE_MotionControlEditor");

            if (materialManagerActive == 0)
            {
                Shader.SetGlobalFloat("TVE_MotionIntensityValue", motionControl);
                Shader.SetGlobalVector("TVE_MotionParams", new Vector4(motionDirection.x * 0.5f + 0.5f, motionDirection.z * 0.5f + 0.5f, motionControl, 0.0f));
            }
            else
            {
                Shader.SetGlobalFloat("TVE_MotionIntensityValue", motionControlEditor);
                Shader.SetGlobalVector("TVE_MotionParams", new Vector4(motionDirection.x * 0.5f + 0.5f, motionDirection.z * 0.5f + 0.5f, motionControlEditor, 0.0f));
            }

            Shader.SetGlobalVector("TVE_WindParams", new Vector4(0.5f, 0.5f, 0.0f, 0.0f));
        }

        void SetGlobalLightingMainLight()
        {
#if UNITY_2023_1_OR_NEWER
            var allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
#else
            var allLights = FindObjectsOfType<Light>();
#endif

            var intensity = 0.0f;

            for (int i = 0; i < allLights.Length; i++)
            {
                if (allLights[i].type == LightType.Directional)
                {
                    if (allLights[i].intensity > intensity)
                    {
                        mainLight = allLights[i];
                    }
                }
            }
        }
    }
}
