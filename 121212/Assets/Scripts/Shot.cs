using UnityEngine;

public class Shot : MonoBehaviour
{
    [SerializeField] private float shootFadeOutSpeed = 2;
    LineRenderer lineRenderer;
    bool shot = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[2] { Vector3.zero, Vector3.zero });
        Set_Shot(false);
    }

    public void Shoot(Vector3 _start_pos, Vector3 _end_pos, Color32 _color)
    {
        Set_Shot(true);

        lineRenderer.SetPositions(new Vector3[2] { _start_pos, _end_pos });

        lineRenderer.startColor = _color;
        lineRenderer.endColor = _color;
    }

    public void Update()
    {
        if (!shot) return;

        lineRenderer.startColor = new Color(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, lineRenderer.startColor.a - Time.deltaTime * shootFadeOutSpeed);
        lineRenderer.endColor = new Color(lineRenderer.endColor.r, lineRenderer.endColor.g, lineRenderer.endColor.b, lineRenderer.endColor.a - Time.deltaTime * shootFadeOutSpeed);

        if (lineRenderer.endColor.a <= 0) Set_Shot(false);
    }

    public void Set_Shot(bool _active)
    {
        shot = _active;
        gameObject.SetActive(_active);
    }
}
