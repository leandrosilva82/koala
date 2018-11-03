using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(CollisionController2D))]
public class Player : CharacterBehaviour {

    [Header("Animations")]
    public SpriteRenderer spriteRenderer;

    [Header("Collision Config")]
    public LayerMask enemiesLayers;
    public int maxNumberEnemyCollider = 1;

    [Header("Lives Config")]
    public int lives = 3;
    public bool useZeroLife = false;

    private CollisionController2D collisionController2D;
    private ContactFilter2D enemyContactFilter;
    private Collider2D[] arrayEnemiesCollider;
    private GameController gameController;

    private bool rightDirection = true;

    

    private void Start()
    {
        //métodos necessários que este script herda
        VariablesInit();
        MechanicsInit();
        
        CustomVariablesInit(); //inicializa as variáveis pertencentes a este script somente
    }

    void Update()
	{

        UpdateDefault();

        if (!alive)
            return;

        //se encostou no teto ou no chão, resetar gravidade
        if (controller.collisions.above || controller.collisions.below)
		{
            ResetVerticalMovement();
		}

        //pega o input horizontal e vertical
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.x < 0f) //andando para esquerda
            moveHorizontal = -1;
        else if (input.x > 0f) //andando para direita
            moveHorizontal = 1;
        else //parado
            moveHorizontal = 0;

        //move a personagem de acordo com o input
        Move(input);

        //verifica se houve pulo e está no chão
        if (Input.GetButtonDown("Jump") && controller.collisions.below)
            Jump();

        if (Input.GetButtonDown("Fire1"))
        {
            
        }

        //encostou na Hitbox de um inimigo: heroi levou dano
        if (CheckCollisionWithEnemy("Hitbox"))
        {
            Die();
            LoseLife();
            gameController.HUDUpdate();
        }
        else //encostou na hurtbox do inimigo: inimigo levou dano
        {
            Collider2D colliderEnemy = CheckCollisionWithEnemy("Hurtbox");
            if (colliderEnemy != null)
            {
                Jump(false);
                Enemy tempEnemy = colliderEnemy.GetComponentInParent<Enemy>();
                tempEnemy.Die();
                tempEnemy.EnableColliders(false);
                AddScore(tempEnemy.defeatScore);

                gameController.HUDUpdate();
            }
        }

        //aplica gravidade
        Gravity();

        UpdateAnimations(); //atualiza animações
        UpdateDirection(); //atualiza direção que a personagem está virada

        UpdateMovement(); //atualiza a movimentação da personagem (horizontal e vertical)

        
    }

    //inicializa as variáveis pertencentes somente a este script
    public void CustomVariablesInit()
    {
        arrayEnemiesCollider = new Collider2D[maxNumberEnemyCollider];
        enemyContactFilter = new ContactFilter2D();
        enemyContactFilter.SetLayerMask(enemiesLayers);
        collisionController2D = GetComponent<CollisionController2D>();

        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
    }

    public void UpdateAnimations()
    {
        if (!alive)
            return;

        //atualiza animação de andar
        characterAC.SetBool("Walk", moveHorizontal != 0);

        //personagem não está em um declive e não está no chão
        if (!controller.collisions.climbingSlope && !controller.collisions.below)
        {
            characterAC.SetBool("Walk", false); //remove animação de andar

            if (velocity.y > 0f) //está indo para cima
            {
                characterAC.SetBool("JumpUp", true);
                characterAC.SetBool("JumpDown", false);
            }
            else //está indo para baixo
            {
                characterAC.SetBool("JumpDown", true);
                characterAC.SetBool("JumpUp", false);
            }
        }
        else
        {
            characterAC.SetBool("JumpUp", false);
            characterAC.SetBool("JumpDown", false);
        }
    }
    public void UpdateDirection()
    {
        if (velocity.x > 0f) //virado para direita
            rightDirection = true;
        else if (velocity.x < 0f) //virado para esquerda
            rightDirection = false;

        spriteRenderer.flipX = !rightDirection; //atualiza direção
    }

    //checa e retorna se houve alguma colisão com algum inimigo
    public Collider2D CheckCollisionWithEnemy(string tag)
    {
        int result = Physics2D.OverlapCollider(collisionController2D.characterCollider, enemyContactFilter, arrayEnemiesCollider);
        if (result > 0)
        {
            foreach (var itemCollider in arrayEnemiesCollider)
            {
                if (itemCollider == null)
                    break;

                if (itemCollider.CompareTag(tag))
                {
                    return itemCollider;
                }
            }
        }
        return null;
    }

    public void LoseLife()
    {
        lives -= 1;
    }

    public override IEnumerator Restart()
    {
        yield return new WaitForSeconds(waitAfterSound);
        if (CheckGameOver())
        {
            GameOver();
            yield break; //encerra coroutine
        }

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

    public bool CheckGameOver()
    {
        if (!useZeroLife && lives == 0)
        {
            return true;
        }
        else if (useZeroLife && lives < 0)
        {
            return true;
        }
        return false;
    }

    public void GameOver()
    {
        print("Fim de jogo.");
    }
}
