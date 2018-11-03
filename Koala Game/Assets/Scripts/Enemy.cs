using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : CharacterBehaviour
{
    [Header("AI Config")]
    public Transform initMoveLimit;
    public Transform endMoveLimit;
    public float horizontalSpeed = 5f, verticalSpeed = 0f;

    [Header("Gravity Config")]
    public bool hasGravity = true;

    [Header("Collisions Config")]
    public Collider2D hurtBoxCollider; //collider onde o inimigo leva dano
    public Collider2D hitBoxCollider; //collider onde o inimigo inflinge dano

    private CollisionController2D collisionController2D;
    
    private bool foundInit = false, foundEnd = false;

    private void Start()
    {
        VariablesInit();
        MechanicsInit();

        collisionController2D = GetComponent<CollisionController2D>();
        
    }

    private void Update()
    {
        UpdateDefault();

        if (!alive)
            return;

        if (hasGravity)
            Gravity();
        /*else
            velocity.y = 0f;*/

        CheckMoveLimit();

        Vector2 currentVelocity = new Vector2(horizontalSpeed, verticalSpeed);

        Move(currentVelocity);

        UpdateMovement(); //atualiza a movimentação da personagem (horizontal e vertical)

        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(collisionController2D.collisionMask);
        int numberColliders = 10;
        Collider2D[] arrayColliders = new Collider2D[numberColliders];
        
        //checa se levou dano
        /*int result = Physics2D.OverlapCollider(hurtBoxCollider, contactFilter, arrayColliders);
        if (result > 0)
        {
            //a cada colisão encontrada
            foreach (var itemCollider in arrayColliders)
            {
                if (itemCollider == null) //pula a última colisão encontrada, pois esta será sempre null
                    return;

                //se colidiu com o heroi
                if (itemCollider.CompareTag("Player") || itemCollider.CompareTag("PlayerCollision"))
                {
                    Die();
                    
                }
            }
        }*/

        /*if (Input.GetButtonDown("Fire1"))
        {
            Die();
        }*/
    }

    public void CheckMoveLimit()
    {
        if (horizontalSpeed != 0f)
        {
            if (transform.position.x <= initMoveLimit.position.x && foundInit == false)
            {
                SetFoundInit();
                ReverseDirectionX();
            }
            else if (transform.position.x >= endMoveLimit.position.x && foundEnd == false)
            {
                SetFoundEnd();
                ReverseDirectionX();
            }
        }
        else if (verticalSpeed != 0f)
        {
            if (transform.position.y >= initMoveLimit.position.y && foundInit == false)
            {
                SetFoundInit();
                ReverseDirectionY();
            }
            else if (transform.position.y <= endMoveLimit.position.y && foundEnd == false)
            {
                SetFoundEnd();
                ReverseDirectionY();
            }
        }
    }

    public void EnableColliders(bool value)
    {
        hitBoxCollider.enabled = value;
        hurtBoxCollider.enabled = value;
    }

    public void ReverseDirectionX()
    {
        horizontalSpeed *= -1;
    }
    public void ReverseDirectionY()
    {
        verticalSpeed *= -1;
    }
    public void SetFoundInit()
    {
        foundInit = true;
        foundEnd = false;
    }
    public void SetFoundEnd()
    {
        foundEnd = true;
        foundInit = false;
    }
}