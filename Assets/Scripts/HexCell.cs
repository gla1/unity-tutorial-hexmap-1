using UnityEngine;

public  class HexCell : MonoBehaviour
{
    int elevation = int.MinValue;
    public HexCoordinates coordinates;
    public RectTransform uiRect;
    public HexGridChunk chunk;
    private bool hasIncomingRiver;
    private bool hasOutgoingRiver;
    private HexDirection incomingRiver;
    private HexDirection outgoingRiver;
    public bool HasIncomingRiver
    {
        get {
            return hasIncomingRiver;
        }
    }

    public bool HasOutgoingRiver
    {
        get {
            return hasOutgoingRiver;
        }
    }

    public HexDirection IncomingRiver
    {
        get {
            return incomingRiver;
        }
    }
    public HexDirection OutgoingRiver {
        get {
            return outgoingRiver;
        }
    }

    public bool HasRiver
    {
        get {
            return hasIncomingRiver || hasOutgoingRiver;
        }
    }

    public bool HasRiverBeginOrEnd
    {
        get {
            return hasIncomingRiver != hasOutgoingRiver;
        }
    }
    
    public Vector3 Position
    {
        get
        {
            return transform.localPosition;
        }
    }
    
    public bool HasRiverThroughEdge (HexDirection direction) {
        return
            hasIncomingRiver && incomingRiver == direction ||
            hasOutgoingRiver && outgoingRiver == direction;
    }

    public void RemoveOutgoingRiver()
    {
        if (!hasOutgoingRiver)
        {
            return;
        }
        hasOutgoingRiver = false;
        RefreshSelfOnly();
        
        HexCell neighbor = GetNeighbor(outgoingRiver);
        // TODO support river from edge by checking if neighbor is null
        neighbor.hasIncomingRiver = false;
        neighbor.RefreshSelfOnly();
    }
    
    public void RemoveIncomingRiver()
    {
        if (!hasIncomingRiver)
        {
            return;
        }
        hasIncomingRiver = false;
        RefreshSelfOnly();
        
        HexCell neighbor = GetNeighbor(incomingRiver);
        // TODO support river from edge by checking if neighbor is null
        neighbor.hasOutgoingRiver = false;
        neighbor.RefreshSelfOnly();
    }
    
    public void RemoveRiver()
    {
        RemoveOutgoingRiver();
        RemoveIncomingRiver();
    }

    public void SetOutgoingRiver(HexDirection direction)
    {
        if (hasOutgoingRiver && outgoingRiver == direction)
        {
            return;
        }
        HexCell neighbor = GetNeighbor(direction);
        if (!neighbor || elevation < neighbor.elevation)
        {
            return;
        }
        RemoveOutgoingRiver();
        if (hasIncomingRiver && incomingRiver == direction)
        {
            RemoveIncomingRiver();
        }
        hasOutgoingRiver = true;
        outgoingRiver = direction;
        RefreshSelfOnly();
        neighbor.RemoveIncomingRiver();
        neighbor.hasIncomingRiver = true;
        neighbor.incomingRiver = direction.Opposite();
        neighbor.RefreshSelfOnly();
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
            
            if (
                hasOutgoingRiver && elevation < GetNeighbor(outgoingRiver).elevation
            )
            {
                RemoveOutgoingRiver();
            }
            if (
                hasIncomingRiver && elevation > GetNeighbor(incomingRiver).elevation
            )
            {
                RemoveIncomingRiver();
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
    
    void RefreshSelfOnly()
    {
        chunk.Refresh();
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

