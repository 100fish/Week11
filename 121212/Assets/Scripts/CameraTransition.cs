using UnityEngine;

public class CameraTransition : MonoBehaviour
{
    [SerializeField] private float sceneTransitionTime;
    [SerializeField] private float sceneTransitionTimer;

    [SerializeField] private Material transitionMaterial;
    [SerializeField] private MeshRenderer backMeshRenderer;
    [SerializeField] private MeshRenderer frontMeshRenderer;
}
