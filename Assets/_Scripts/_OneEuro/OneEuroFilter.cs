using UnityEngine;

public class OneEuroFilter
{
    public float minCutoff;
    public float beta;
    public float dCutoff;

    private Vector3 prevPosition;
    private Vector3 prevFilteredPosition;
    private float prevFilteredPositionDerivative;
    private float dt;

    public OneEuroFilter(float minCutoff, float beta, float dCutoff, float initialDt)
    {
        this.minCutoff = minCutoff;
        this.beta = beta;
        this.dCutoff = dCutoff;
        dt = initialDt;
    }

    private float LowPassFilter(float value, float prevFilteredValue, float cutoff)
    {
        float tau = 1.0f / (2 * Mathf.PI * cutoff);
        float alpha = dt / (tau + dt);
        return alpha * value + (1 - alpha) * prevFilteredValue;
    }

    private Vector3 LowPassFilter(Vector3 value, Vector3 prevFilteredValue, float cutoff)
    {
        float tau = 1.0f / (2 * Mathf.PI * cutoff);
        float alpha = dt / (tau + dt);
        return Vector3.Lerp(prevFilteredValue, value, alpha);
    }

    public Vector3 FilterPosition(Vector3 position)
    {
        // Compute the derivative of the position
        Vector3 positionDerivative = (position - prevPosition) / dt;
        prevFilteredPositionDerivative = LowPassFilter(positionDerivative.magnitude, prevFilteredPositionDerivative, dCutoff);

        // Adapt the cutoff frequency
        float cutoff = minCutoff + beta * Mathf.Abs(prevFilteredPositionDerivative);

        // Apply the low-pass filter
        Vector3 filteredPosition = LowPassFilter(position, prevFilteredPosition, cutoff);

        // Update previous values
        prevPosition = position;
        prevFilteredPosition = filteredPosition;

        return filteredPosition;
    }

    public void UpdateDeltaTime(float deltaTime)
    {
        dt = deltaTime;
    }
}