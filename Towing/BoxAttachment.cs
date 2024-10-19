using UnityEngine;

public class BoxAttachment : MonoBehaviour
{
    [SerializeField] private GameObject car;  // The car GameObject
    [SerializeField] private GameObject box;  // The box GameObject
    [SerializeField] private Transform pullingPoint; // The point on the box where the line should attach
    [SerializeField] private Rigidbody connectedRigidbody; // Rigidbody to which the box will be attached
    [SerializeField] private KeyCode detachKey = KeyCode.Q; // Key to detach the box

    private ConfigurableJoint configurableJoint;
    private LineRenderer lineRenderer;

    // Adjust these parameters to influence how weight affects the car
    [SerializeField] private float baseSpringForce = 10f; // Base strength of the spring
    [SerializeField] private float weightImpactFactor = 2f; // How much the box's weight impacts the spring force
    [SerializeField] private float initialRopeLength = 5f; // Initial length of the rope

    // Assign the rope texture material here
    [SerializeField] private Material ropeMaterial;

    public bool isOn;
    private bool wasOn;

    void Start()
    {
        SetupLineRenderer(); // Ensure LineRenderer is set up on start
    }

    void Update()
    {
        if (isOn)
        {
            wasOn = true;

            // Check if the detachment should happen
            if (Input.GetKeyDown(detachKey) && configurableJoint != null)
            {
                DetachBox();
            }

            // Check if the box is still valid
            if (box == null || !box.activeInHierarchy)
            {
                DetachBox();
                return;
            }

            // Update the visual line position
            if (configurableJoint != null)
            {
                UpdateLineRendererPositions();
            }
        }
        else if (wasOn)
        {
            wasOn = false;
            DetachBox();
        }
    }

    void AttachBox()
    {
        if (connectedRigidbody == null || pullingPoint == null || box == null)
        {
            Debug.LogError("Connected Rigidbody, pulling point, or box is not assigned.");
            return;
        }

        // Remove any existing ConfigurableJoint on the box before adding a new one
        ConfigurableJoint existingJoint = box.GetComponent<ConfigurableJoint>();
        if (existingJoint != null)
        {
            Destroy(existingJoint);
        }

        // Add a ConfigurableJoint component to the box and connect it to the specified Rigidbody
        configurableJoint = box.AddComponent<ConfigurableJoint>();
        configurableJoint.connectedBody = connectedRigidbody;

        // Set the anchor to the pulling point
        configurableJoint.anchor = box.transform.InverseTransformPoint(pullingPoint.position);

        // Configure the joint to behave like a spring
        JointDrive jointDrive = new JointDrive
        {
            positionSpring = baseSpringForce + box.GetComponent<Rigidbody>().mass * weightImpactFactor,
            positionDamper = 2f,
            maximumForce = Mathf.Infinity
        };
        configurableJoint.xDrive = jointDrive;
        configurableJoint.yDrive = jointDrive;
        configurableJoint.zDrive = jointDrive;

        // Set initial rope length
        configurableJoint.linearLimit = new SoftJointLimit { limit = initialRopeLength };

        // Ensure LineRenderer is updated
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            UpdateLineRendererPositions();
        }
    }

    public void DetachBox()
    {
        // Destroy the ConfigurableJoint to detach the box
        if (configurableJoint != null)
        {
            Destroy(configurableJoint);
            configurableJoint = null;
        }

        // Disable the LineRenderer and clear its positions
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);
        }

        // Clear references to the box and pulling point
        box = null;
        pullingPoint = null;
    }

    void SetupLineRenderer()
    {
        if (box != null)
        {
            // Add or get the LineRenderer component on the box
            lineRenderer = box.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = box.AddComponent<LineRenderer>();
                Debug.Log("LineRenderer added to the box.");
            }
            else
            {
                Debug.Log("LineRenderer already exists on the box.");
            }

            // Configure the LineRenderer properties
            lineRenderer.startWidth = 0.1f; // Adjust width as needed
            lineRenderer.endWidth = 0.1f; // Adjust width as needed
            lineRenderer.material = ropeMaterial; // Apply the rope material
            lineRenderer.positionCount = 2; // Set the number of positions
            lineRenderer.enabled = false; // Initially disabled
        }
    }

    void UpdateLineRendererPositions()
    {
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, car.transform.position);
            lineRenderer.SetPosition(1, pullingPoint != null ? pullingPoint.position : box.transform.position);
        }
        else
        {
            Debug.LogError("LineRenderer is not set up.");
        }
    }

    [SerializeField] private float _attachmentAngleThreshold = -0.5f;

    private void OnCollisionEnter(Collision collision)
    {
        if (isOn)
        {
            Towable towableObject = null;
            if (collision.gameObject.TryGetComponent<Towable>(out towableObject) && configurableJoint == null)
            {
                Vector3 toCollision = collision.contacts[0].point - car.transform.position;
                toCollision.Normalize();

                // Check if the collision is behind the car
                if (Vector3.Dot(-car.transform.right, toCollision) < _attachmentAngleThreshold) // Adjust threshold as needed
                {
                    if (towableObject._connectionPoint != null)
                    {
                        pullingPoint = towableObject._connectionPoint;
                        box = towableObject.gameObject;
                        SetupLineRenderer(); // Ensure LineRenderer is set up for new box
                    }
                    else
                    {
                        pullingPoint = null; // Default to the box itself if no pulling point is assigned
                        box = towableObject.gameObject;
                        SetupLineRenderer(); // Ensure LineRenderer is set up for new box
                    }

                    AttachBox();
                    Debug.Log("Box attached and LineRenderer updated.");
                }
                else
                {
                    Debug.Log("Not angled properly for attachment.");
                }
            }
        }
    }

    // Method to change the length of the rope
    public void SetRopeLength(float length)
    {
        if (configurableJoint != null)
        {
            configurableJoint.linearLimit = new SoftJointLimit { limit = length };
        }
    }
}
