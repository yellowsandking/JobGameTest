using UnityEngine;

/// <summary>
/// Player wing presentation data. Kept separate from the view so gameplay code can drive it without touching Unity objects.
/// </summary>
public class WingModel
{
    public int WingID { get; set; }
    public bool IsVisible { get; set; } = true;
    public Vector3 LocalPosition { get; set; } = new Vector3(0f, 1.2f, -0.25f);
    public Quaternion LocalRotation { get; set; } = Quaternion.identity;
    public Vector3 LocalScale { get; set; } = Vector3.one;

    public void Reset()
    {
        WingID = 0;
        IsVisible = true;
        LocalPosition = new Vector3(0f, 1.2f, -0.25f);
        LocalRotation = Quaternion.identity;
        LocalScale = Vector3.one;
    }
}
