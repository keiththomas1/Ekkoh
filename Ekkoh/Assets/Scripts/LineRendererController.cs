using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererController : MonoBehaviour
{
    [SerializeField]
    private LineRenderer LineRenderer;
    [SerializeField]
    private Material SelectedMaterial;
    [SerializeField]
    private Material UnselectedMaterial;

    public void Setup(Vector3[] positions)
    {
        this.LineRenderer.positionCount = positions.Length;
        this.LineRenderer.SetPositions(positions);
        this.SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        this.LineRenderer.startWidth = selected ? 0.025f : 0.012f;
        this.LineRenderer.endWidth = selected ? 0.025f : 0.012f;
        this.LineRenderer.material = selected ? this.SelectedMaterial : this.UnselectedMaterial;
    }
}
