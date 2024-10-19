using UnityEngine;

public class BladeRotator : MonoBehaviour
{
    // Array to hold the blades (or any other GameObjects)
    public GameObject[] blades = new GameObject[4];

    // Rotation speed for the blades
    public float rotationSpeed = 100f;

    [SerializeField] private AudioSource _droneSound;

    private void OnEnable()
    {
        if(_droneSound != null )
            _droneSound.Play();
    }
    private void OnDisable()
    {
        if (_droneSound != null)
            _droneSound.Stop();
    }

    void Update()
    {
        // Rotate each blade around its own axis
        foreach (GameObject blade in blades)
        {
            if (blade != null)
            {
                blade.transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
