using UnityEngine;

public  class HexCell : MonoBehaviour
{
    int elevation = int.MinValue;
    public HexCoordinates coordinates;
    public RectTransform uiRect;
    public HexGridChunk chunk;

    public Vector3 Position
    {
        get
        {
            return transform.localPosition;
        }
    }
    [SerializeField]
    HexCell[] neighbors;

    public Color Color {
        get {
            return color;
        }
        set {
            if (color == value) {
                return;
            }
            color = value;
            Refresh();
        }
    }

    Color color;

    public int Elevation
    {
        get { return elevation; }
        set
        {
            elevation = value;
            Vector3 position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;
            position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
            transform.localPosition = position;
            
            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = elevation * -position.y;
            uiRect.localPosition = uiPosition;
            if (elevation == value)
            {
                return;
            }
            Refresh();
        }
    }

    void Refresh()
    {
        if (chunk)
        {
            chunk.Refresh();
            for (int i = 0; i < neighbors.Length; i++)
            {
                HexCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.chunk != chunk)
                {
                    neighbor.chunk.Refresh();
                }
            }
        }
    }
    
    public HexCell GetNeighbor (HexDirection direction) {
        return neighbors[(int)direction];
    }

    public void SetNeighbor (HexDirection direction, HexCell cell) {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }
    
    public HexEdgeType GetEdgeType (HexDirection direction) {
        return HexMetrics.GetEdgeType(elevation, neighbors[(int)direction].elevation);
    }
    
    public HexEdgeType GetEdgeType (HexCell otherCell) {
        return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
    }
}

