public class PlayerMovement : Monobehavior
{
    public float speed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1f;

    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }
}