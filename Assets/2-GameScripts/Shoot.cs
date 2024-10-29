using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 2.5f;
    [SerializeField] AudioClip shootSFX;

    new Rigidbody2D rigidbody2D;
    PlayerMovement playerMovement;
    float directionSpeed;

    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        playerMovement = FindObjectOfType<PlayerMovement>();
        directionSpeed = playerMovement.transform.localScale.x * speed;

        AudioSource.PlayClipAtPoint(shootSFX, Camera.main.transform.position);
    }

    void Update()
    {
        rigidbody2D.velocity = new Vector2(directionSpeed, 0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Enemy")
        {
            Destroy(other.gameObject);
        }
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        Destroy(gameObject);
    }
}
