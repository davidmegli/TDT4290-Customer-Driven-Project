using UnityEngine;

public class WallCollisions : MonoBehaviour
{
    public AudioClip collisionSound; // Assign in inspector
    private AudioSource audioSource;

    void Start()
    {
        // Ensure the wall has an AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Optional: filter by tags (e.g., only player/hands cause sound)
        if (other.CompareTag("PlayerHand") || other.CompareTag("Player"))
        {
            PlayCollisionSound();
            Debug.Log("Wall collided with: " + other.gameObject.name);
        }
    }

    private void PlayCollisionSound()
    {
        if (collisionSound != null)
        {
            audioSource.PlayOneShot(collisionSound);
        }
        else
        {
            Debug.LogWarning("No collision sound assigned to WallCollision script!");
        }
    }
}
