using UnityEngine;

public class RenderObject
{
    Quaternion Rotation = Quaternion.identity;
    private GameObject OriginalGameObject;
    private Vector3 Position;
    public RenderObject(GameObject original, Vector3 position)
    {
        this.OriginalGameObject = original;
        this.Position = position;
    }

    public Quaternion GetRotation()
    {
        return Rotation;
    }
    public Vector3 GetPosition()
    {
        return Position;
    }
    public GameObject GetGameObject()
    {
        return OriginalGameObject;
    }
}
