using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] public float startingHealth;
    public float currentHealth { get; private set; }
    private Animator anim;

    [Header("iFrames")]
    [SerializeField] private float iFramesDuration;
    [SerializeField] private int numberOfFlashes;
    private SpriteRenderer spriteRend;

    public bool dead;

    [SerializeField] private float howLongUntilRespawn;
    private float timerUntilRespawn;

    private enum MovementState { dead }

    [SerializeField] private AudioSource deadSoundEffect;
    [SerializeField] private AudioSource playerDamageSoundEffect;


    private void Awake()
    {
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
        spriteRend = GetComponent<SpriteRenderer>();
        timerUntilRespawn = howLongUntilRespawn;
    }

    private void Update()
    {
        if (dead)
        {
            timerUntilRespawn -= Time.deltaTime;

            if(timerUntilRespawn < 0 && gameObject.tag == "Player")
            {
                SceneManager.LoadScene("Room1");
                currentHealth = startingHealth;
                GetComponent<PlayerMovement>().enabled = true;
                anim.SetTrigger("alive");
                timerUntilRespawn = howLongUntilRespawn;
            }
        }
    }

    public void TakeDamage(float _damage) 
    {
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);

        if (currentHealth > 0)
        {
            if(!gameObject.GetComponent<Animator>().GetBool("IdleBlock"))
            {
                anim.SetTrigger("hurt");
            }
            else
            {
                gameObject.GetComponent<Animator>().SetBool("IdleBlock", false);
            }

            if(gameObject.tag == "Player")
            {
                StartCoroutine(Invulnerability());
                playerDamageSoundEffect.Play();
            }
        }
        else
        {
            if(dead == false)
            {
                anim.SetTrigger("die");
            }

            if(GetComponent<PlayerMovement>() != null)
                GetComponent<PlayerMovement>().enabled = false;

            if(GetComponentInParent<EnemyPatrol>() != null)
                GetComponentInParent<EnemyPatrol>().enabled = false;
            
            if(GetComponent<MeleeSkeleton>() != null)
            {
                Destroy(GetComponent<MeleeSkeleton>().attackArea);
                GetComponent<MeleeSkeleton>().enabled = false;
            }

            if (gameObject.tag == "Player" && dead == false)
            {
                deadSoundEffect.Play();
            }

            dead = true;
        }
    }


    private void OnLevelWasLoaded(int level)
    {
        if(gameObject.tag == "Player" && dead)
        {
            gameObject.transform.position = GameObject.FindWithTag("EntrancePos").transform.position;
            dead = false;
        }
    }

    public void AddHealth(float _value)
    {
        currentHealth = Mathf.Clamp(currentHealth + _value, 0, startingHealth);
    }

    private IEnumerator Invulnerability()
    {
        Physics2D.IgnoreLayerCollision(10, 11, true);
        for(int i = 0; i < numberOfFlashes; i++)
        {
            spriteRend.color = new Color(1, 1, 1, 0.7f);
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
            spriteRend.color = Color.white;
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
        }
        Physics2D.IgnoreLayerCollision(10, 11, false);
    }
}
