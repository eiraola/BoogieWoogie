using UnityEngine;
public static class ExtensionMethods
{

    public static Vector2 ToVector2(this Vector3 originalVector)
    {
        return Vector2.up * originalVector.y + Vector2.right * originalVector.x;
    }
    public static Vector3 ToVector3(this Vector2 originalVector)
    {
        return Vector3.up * originalVector.y + Vector3.right * originalVector.x;
    }
}