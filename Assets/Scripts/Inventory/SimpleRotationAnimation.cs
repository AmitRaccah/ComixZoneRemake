using UnityEngine;

[AddComponentMenu("IAC/Simple Game/Simple Rotation")]
public class SimpleRotationAnimation : MonoBehaviour
{public enum Axis { X, Y, Z }; // Enum for rotation axis
    public Axis rotationAxis = Axis.Y; // Default rotation axis
    public float rotationSpeed = 30f; // Base speed of rotation in degrees per second
    public float hoverHeight = 0.1f; // Base height of hover movement
    public float hoverSpeed = 1f; // Base speed of hover movement
    public float rotationSpeedOffset = 10f; // Maximum random offset for rotation speed
    public float hoverHeightOffset = 0.05f; // Maximum random offset for hover height

    private Vector3 startPosition; // Initial position of the object
    private float rotationSpeedActual; // Actual rotation speed with random offset
    private float hoverHeightActual; // Actual hover height with random offset

    void Start()
    {
        startPosition = transform.position;

        // Calculate actual rotation speed with random offset
        rotationSpeedActual = rotationSpeed + Random.Range(-rotationSpeedOffset, rotationSpeedOffset);

        // Calculate actual hover height with random offset
        hoverHeightActual = hoverHeight + Random.Range(-hoverHeightOffset, hoverHeightOffset);
    }

    void Update()
    {
        Vector3 axisVector = Vector3.zero;

        // Set the axis vector based on the selected enum value
        switch (rotationAxis)
        {
            case Axis.X:
                axisVector = Vector3.right;
                break;
            case Axis.Y:
                axisVector = Vector3.up;
                break;
            case Axis.Z:
                axisVector = Vector3.forward;
                break;
        }

        // Rotate the object around the selected axis at the actual rotation speed
        transform.Rotate(axisVector, rotationSpeedActual * Time.deltaTime);

        // Calculate the vertical movement with actual hover height
        float hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverHeightActual;
        Vector3 newPosition = startPosition + new Vector3(0f, hoverOffset, 0f);

        // Apply the vertical movement
        transform.position = newPosition;

    }
}
