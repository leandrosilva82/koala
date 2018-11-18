using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(CollisionController2D))]
[RequireComponent(typeof(AudioController))]
[RequireComponent(typeof(AudioSource))]

public class CharacterBehaviour : MonoBehaviour
{
    [Header("Jump")]
    public float jumpHeight = 4f; //limite da altura do pulo
    public float timeToJumpApex = 0.4f; //velocidade em que se alcançará o limite do pulo
    public float accelerationTimeAirbone = 0.2f; //tempo de aceleração horizontal no alto
    public AudioClip soundJump;

    [Header("Move")]
    public float moveSpeed = 6; //velocidade de movimento
    public float accelerationTimeGrounded = 0.1f; //tempo de aceleração no chão

    [Header("Attack")]
    public GameObject weapon;

    [Header("Restart")]
    public int waitAfterSound = 1;
    public bool restartPositionAfter = false;
    public AudioClip soundDie;

    [Header("Sound")]
    public AudioController audioController; 

    [Header("Animations")]
    public Animator characterAC;

    [Header("Score Config")]
    public int score = 0;
    public int scoreOneUp = 1000;
    public int defeatScore = 100;

    protected float velocityXSmoothing, velocityYSmoothing;
    protected float jumpVelocity;
    protected float gravity;
    protected int moveHorizontal = 0; //indica se a personagem está andando. 1: direita; -1: esquerda; 0: parado
    protected Vector2 initialPosition;
    protected Vector3 velocity; //velocidade deste objeto
    protected CollisionController2D controller; //responsável por controlar o personagem
    protected bool alive = true, isRestarting = false;

    void Start()
    {
        VariablesInit();
        MechanicsInit();
    }

    private void Update()
    {
        UpdateDefault();
    }

    protected void UpdateDefault()
    {
        //continua o código somente se o heroi estiver vivo
        if (!alive)
        {
            if (!isRestarting && CheckCanRestart())
            {
                isRestarting = true;
                StartCoroutine(Restart());
            }
            return;
        }
    }

    public void VariablesInit()
    {
        try
        {

            initialPosition = transform.position;

            controller = GetComponent<CollisionController2D>();
            if (controller == null)
                throw new UnityException("É necessário ter o script \"Controller2D\".");

            if (audioController == null)
                throw new UnityException("É necessário ter o componente \"Audio Controller\".");
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
            return;
        }

    }
    public void MechanicsInit()
    {
        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2); //define gravidade
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex; //define velocidade de pulo
    }

    public void ResetVerticalMovement()
    {
        velocity.y = 0f;
    }
    public void ResetHorizontalMovement()
    {
        velocity.x = 0f;
    }
    public void Jump(bool playAudio = true, float jumpPower = 0f)
    {
        velocity.y = jumpPower == 0f? jumpVelocity : jumpPower;
        if (playAudio)
            audioController.PlayAudio(soundJump);
    }
    public void Move(Vector2 movement)
    {
        //atualiza a velocidade da personagem de acordo com o input + velocidade de movimento estabelecida
        float targetVelocityX = movement.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(
            velocity.x,
            targetVelocityX,
            ref velocityXSmoothing,
            controller.collisions.below ? accelerationTimeGrounded : accelerationTimeAirbone
        );

        if (movement.y > 0f)
        {
            Debug.Log("Move Weapon!");
            if(weapon.transform.rotation.z < 90)
                weapon.transform.Rotate(0,0, 500f * Time.deltaTime);
        }
    }

    public void Gravity()
    {
        velocity.y += gravity * Time.deltaTime; //efeito da gravidade neste objeto
    }

    public void UpdateMovement()
    {
        controller.Move(velocity * Time.deltaTime); //movimenta este objeto
    }

    public void Die()
    {
        alive = false;
        audioController.PlayAudio(soundDie);
        ResetHorizontalMovement();
        ResetVerticalMovement();
        characterAC.SetBool("Die", true);
    }

    public bool CheckCanRestart()
    {
        if (!audioController.IsPlaying())
            return true;

        return false;
    }

    public virtual IEnumerator Restart()
    {
        yield return new WaitForSeconds(waitAfterSound);
        if (!restartPositionAfter)
        {
            Destroy(gameObject);
        }
        else
        {
            transform.position = initialPosition;
            alive = true;
            isRestarting = false;
            characterAC.SetBool("Die", false);
        }
    }

    #region SCORE

    public virtual void AddScore(int value)
    {
        if (score + value < 0)
            SetScore(0);
        else
            SetScore(score + value);
    }

    #endregion

    #region GETTERS/SETTERS

    public virtual int GetScore()
    {
        return score;
    }
    public virtual void SetScore(int value)
    {
        score = value;
    }
    public virtual int GetDefeatScore()
    {
        return defeatScore;
    }
    public virtual void SetDefeatScore(int value)
    {
        defeatScore = value;
    }

    #endregion
}