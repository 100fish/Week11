using UnityEngine;

public class MathH : MonoBehaviour
{
    public static float Vector2_To_Angle(Vector2 _v)
    {
        float _angle = Mathf.Atan2(_v.y, _v.x) * Mathf.Rad2Deg;
        return _angle;
    }

    public static Vector2 Angle_To_Vector2(float _angle)
    {
        Vector2 _v = (Vector2)(Quaternion.Euler(0, 0, _angle) * Vector2.right);
        return _v;
    }

    public static float Distance_Between_Two_Angles(float angle_1, float angle_2)
    {
        float _angle_1 = angle_1;
        float _angle_2 = angle_2;

        float _angle_distance = Mathf.DeltaAngle(_angle_1, _angle_2);
        return _angle_distance;
    }

    public static float Angle_Between_Two_Points(Vector2 _origin, Vector2 _destination)
    {
        float _angle = Mathf.Atan2(_destination.y - _origin.y, _destination.x - _origin.x) * 180 / Mathf.PI;
        return _angle;
    }

    public static Vector3 Direction_Between_Two_Vectors(Vector3 _origin, Vector3 _destination)
    {
        Vector3 _dir = (_destination - _origin).normalized;
        return _dir;
    }

    public static bool Is_This_Integer(float _float)
    {
        return Mathf.Approximately(_float, Mathf.RoundToInt(_float));
    }

    public static float Quantize(float _float, float _max, int _stepCount)
    {
        return (Mathf.Floor((_float / _max) * _stepCount) / _stepCount) * _max;
    }

    public static float Average_Two_Angles(float _angle_1, float _angle_2)
    {
        float _x = Mathf.Cos(_angle_1 * Mathf.Deg2Rad) + Mathf.Cos(_angle_2 * Mathf.Deg2Rad);
        float _y = Mathf.Sin(_angle_1 * Mathf.Deg2Rad) + Mathf.Sin(_angle_2 * Mathf.Deg2Rad);
        float _angle = Mathf.Rad2Deg * Mathf.Atan2(_y, _x);
        return _angle;
    }
}