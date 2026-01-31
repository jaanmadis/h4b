using UnityEngine;

public static class Constants
{
    public const float GRAVITY_STRENGTH = 10f;
    public const float LANDING_TARGET_DISTANCE = 100f;
    public const float LANDING_TARGET_VELOCITY = 10f;
    public const float PRE_COASTING_DELAY = 5f;
    public const float POST_LANDING_DELAY = 10f;
    public static readonly Vector3 ASTEROID_CENTER_X = Vector3.zero;
    public static readonly Vector3 ASTEROID_CENTER_LANDING = new(0, -95000, 0);
}
