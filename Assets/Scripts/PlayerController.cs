using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using UnityEngine.UIElements;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private Vector3 targetPos;
    [SerializeField] private bool isMoving = false;

    // Rigidbody of the player.
    private Rigidbody rb;
    private Transform tr;

    // Variable to keep track of collected "PickUp" objects.
    private int count;

    // Movement along X and Y axes.
    private float movementX;
    private float movementY;

    private AudioSource audioSource;

    private Animator anim;

    // Speed at which the player moves.
    public float speed = 0;

    // UI text component to display count of "PickUp" objects collected.
    public TextMeshProUGUI countText;

    // UI object to display winning text.
    public GameObject winTextObject;

    public GameObject explosionFX;
    public GameObject winFX;
    public GameObject pickupFX;
    public GameObject trailFX;
    // Start is called before the first frame update.
    void Start()
    {
        // Get and store the Rigidbody component attached to the player.
        rb = GetComponent<Rigidbody>();
        tr= GetComponent<Transform>();

        // Initialize count to zero.
        count = 0;

        // Update the count display.
        SetCountText();

        // Initially set the win text to be inactive.
        winTextObject.SetActive(false);
        audioSource = GetComponent<AudioSource>();

        anim = GetComponentInChildren<Animator>();
        //Set the value of speed_f
        if (anim)
        {
            anim.SetInteger("AnimationID", 1);
        }
    }

    private void Update()
    {
        //if (Input.GetMouseButton(0)) // Check if left mouse button is held down
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            //Debug.Log("Mouse Clicked");
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            //Debug.Log(ray);
            Debug.DrawRay(ray.origin, ray.direction * 50, Color.yellow);

            RaycastHit hit; // Define variable to hold raycast hit information

            
            // Check if raycast hits an object
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    targetPos = hit.point; // Set target position
                    isMoving = true; // Start player movement
                }
            }

        }            
        else
        {
                isMoving = false; // Stop player movement
        }

     }

    // This function is called when a move input is detected.
    void OnMove(InputValue movementValue)
    {
        // Convert the input value into a Vector2 for movement.
        Vector2 movementVector = movementValue.Get<Vector2>();

        // Store the X and Y components of the movement.
        movementX = movementVector.x;
        movementY = movementVector.y;
        trailFX.SetActive(true);

        if (anim)
        {
            anim.SetInteger("AnimationID", 2);
        }

    }

    // FixedUpdate is called once per fixed frame-rate frame.
    private void FixedUpdate()
    {

        // Create a 3D movement vector using the X and Y inputs.
        Vector3 movement = new Vector3(movementX, 0.0f, movementY);
        
        // Apply force to the Rigidbody to move the player.
        rb.AddForce(movement * speed);


        if (movementX >= 0 && Math.Abs(movementX) >= Math.Abs(movementY))//direction.y)
            tr.Rotate(0, 0, -1);
        else
        if (movementX < 0 && Math.Abs(movementX) >= Math.Abs(movementY))
            tr.Rotate(0, 0, 1);
        else
        if (movementX >= 0 && Math.Abs(movementX) < Math.Abs(movementY))
            tr.Rotate(0, 0, 1);
        else
        if (movementX < 0 && Math.Abs(movementX) < Math.Abs(movementY))
            tr.Rotate(0, 0, -1);

        Instantiate(trailFX, transform.position, Quaternion.identity);
        trailFX.SetActive(false);


        if (isMoving)
        {
            Vector3 direction = (targetPos - rb.position).normalized;
            trailFX.SetActive(true);
            // Move the player towards the target position
            rb.AddForce(direction * speed);

            if (direction.x >= 0 && Math.Abs(direction.x)>=Math.Abs(direction.y))//direction.y)
                tr.Rotate(0, 0, direction.z);
            else
                if (direction.x < 0 && Math.Abs(direction.x) >= Math.Abs(direction.y))
                    tr.Rotate(0, 0, -direction.z);
            else
                if (direction.x >= 0 && Math.Abs(direction.x) < Math.Abs(direction.y))
                tr.Rotate(0, 0, -direction.z);
            else
                if (direction.x < 0 && Math.Abs(direction.x) < Math.Abs(direction.y))
                tr.Rotate(0, 0, direction.z);

            
            Instantiate(trailFX, transform.position, Quaternion.identity);
            trailFX.SetActive(false);

        }
    
        // Stop moving the player if it is close to the target position
        if (Vector3.Distance(rb.position, targetPos) < 0.5f)
        {
            isMoving = false;
        }

        


    }


    void OnTriggerEnter(Collider other)
    {
        // Check if the object the player collided with has the "PickUp" tag.
        if (other.gameObject.CompareTag("Pickup"))
        {
            // Deactivate the collided object (making it disappear).
            other.gameObject.SetActive(false);

            var currentPickupFX = Instantiate(pickupFX, other.transform.position, Quaternion.identity);
            Destroy(currentPickupFX, 3);


            // Increment the count of "PickUp" objects collected.
            count = count + 1;

            if (count < 12) { audioSource.Play(); }

            // Update the count display.
            SetCountText();
        }
    }

    // Function to update the displayed count of "PickUp" objects collected.
    void SetCountText()
    {
        // Update the count text with the current count.
        countText.text = "Count: " + count.ToString();

        // Check if the count has reached or exceeded the win condition.
        if (count >= 12)
        {
            // Display the win text.
            winTextObject.SetActive(true);
            winTextObject.GetComponent<TextMeshProUGUI>().text = "You Win!";
            Instantiate(winFX, transform.position, Quaternion.identity);

            countText.GetComponent<AudioSource>().Play();

          

            // Destroy the enemy GameObject.
            Destroy(GameObject.FindGameObjectWithTag("Enemy"));
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            AudioSource audioSourceEnemy = collision.gameObject.GetComponent<AudioSource>();

            Animator animator = collision.gameObject.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                // Detenemos la animación relacionada con la velocidad
                animator.SetFloat("speed_f", 0);
            }
            

            audioSourceEnemy.Play();

            
            // Destroy the current object
            Destroy(gameObject);
            Instantiate(explosionFX, transform.position, Quaternion.identity);

            

            // Update the winText to display "You Lose!"
            winTextObject.gameObject.SetActive(true);
            winTextObject.GetComponent<TextMeshProUGUI>().text = "You Lose!";
            


        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            AudioSource audioSourceWall = collision.gameObject.GetComponent<AudioSource>();

            audioSourceWall.Play();


        }

    }


}