using UnityEngine;

public class CameraTransition : MonoBehaviour
{
    [SerializeField] private float sceneTransitionTime;
    [SerializeField] private float sceneTransitionTimer;

    [SerializeField] private Material transitionMaterial;
    [SerializeField] private CustomRenderTexture camART;
    [SerializeField] private RenderTexture camBRT;
    [SerializeField] private MeshRenderer backMeshRenderer;
    [SerializeField] private MeshRenderer frontMeshRenderer;

    //DELETE
    [SerializeField] private Camera camAA;
    [SerializeField] private Camera camBB;


    TransitionCause transitionCause;

    [SerializeField] private Color[] colors;

    public enum TransitionCause
    {
        None,
        Enemy,
        Environment,
        Progression,
    }
    // Assign your Shader Graph material or the Shader asset here
    public Material targetMaterial;

    Shader shader;

    void Start()
    {
        if (targetMaterial == null) return;

        shader = targetMaterial.shader;
        int propertyCount = shader.GetPropertyCount();

        Debug.Log($"<b>Shader:</b> {shader.name} has {propertyCount} properties.");

        for (int i = 0; i < propertyCount; i++)
        {
            // Get the internal reference name (e.g., "_BaseColor")
            string propertyName = shader.GetPropertyName(i);

            // Get the property type enum (Float, Vector, Color, TexEnv, etc.)
            UnityEngine.Rendering.ShaderPropertyType type = shader.GetPropertyType(i);

            Debug.Log($"Property Name: <b>{propertyName}</b> | Type Name: <b>{type}</b>");
        }



        sceneTransitionTimer = sceneTransitionTime;
        StartTransition(camAA, camBB, TransitionCause.Progression);
    }

    public void StartTransition(Camera camA, Camera camB, TransitionCause cause)
    {
        camA.targetTexture = camART;
        camB.targetTexture = camBRT;

        transitionCause = cause;

        Debug.Log("BeforeSettingStart");
        //SetGlobalPropertyDynamic(shader, 0, colors[(int)cause]);

        Shader.SetGlobalColor("_MyFireColour", colors[(int)cause]);
        //transitionMaterial.SetColor("_FireColour", colors[(int)transitionCause]);
        camART.Update();
    }

    private void Update()
    {
        if (transitionCause == TransitionCause.None)
            return;

        camART.Update();

        if (sceneTransitionTimer > 0)
        {
            sceneTransitionTimer -= Time.deltaTime;


            Debug.Log("Lerp: " + Mathf.InverseLerp(0, sceneTransitionTime, sceneTransitionTimer));



            Debug.Log("BeforeSettingLERPLERPLERp");
            //SetGlobalPropertyDynamic(shader, 1, Mathf.InverseLerp(0, sceneTransitionTime, sceneTransitionTimer));

            Shader.SetGlobalFloat("_MyFireProgress", 1 - Mathf.InverseLerp(0, sceneTransitionTime, sceneTransitionTimer));
            //transitionMaterial.SetGlobalFloat("_FireProgress", Mathf.InverseLerp(0,sceneTransitionTime,sceneTransitionTimer));
            camART.Update();
        }
        else
        {
            Debug.Log("Transition Ended");
            sceneTransitionTimer = sceneTransitionTime;
            transitionCause = TransitionCause.None;
        }

    }
}
/*
public class CRTMaterialController : MonoBehaviour
{
    [Header("Setup References")]
    [SerializeField] private CustomRenderTexture targetCRT;
    [SerializeField] private Material sourceShaderGraphMaterial;

    // This will hold our unique runtime instance of the ShaderGraph material
    private Material runtimeMaterialInstance;

    void Start()
    {
        if (targetCRT == null || sourceShaderGraphMaterial == null)
        {
            //Debug.LogError("Please assign the Custom RenderTexture and Source Material in the inspector.");
            return;
        }

        // 1. Create a unique runtime instance so you don't overwrite your project asset files
        runtimeMaterialInstance = new Material(sourceShaderGraphMaterial);

        // 2. Force the Custom RenderTexture to use this specific runtime instance
        targetCRT.material = runtimeMaterialInstance;

        // 3. Initialize/Reset the Custom RenderTexture zone
        targetCRT.Initialize();
    }

    // Call this method whenever you need to update a property
    public void UpdateShaderGraphProperty(string propertyName, float newValue)
    {
        if (runtimeMaterialInstance != null)
        {
            // 1. Change the value on the source material
            runtimeMaterialInstance.SetFloat(propertyName, newValue);

            // 2. Tell the Custom RenderTexture it needs to redraw itself using the new value
            targetCRT.Update();
        }
    }
}*/