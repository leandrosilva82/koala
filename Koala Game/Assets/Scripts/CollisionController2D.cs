using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(BoxCollider2D))]
public class CollisionController2D : MonoBehaviour {

    //Constantes
	const float skinWidth = 0.015f;

    [Header("Rays Config")]
    public int horizontalRayCount = 4; //quantos rayCasts terão horizontalmente
	public int verticalRayCount = 4; //quantos raycasts terão verticalmente
	public float horizontalRaySpacing; //espaçamento horizontal entre os raycasts
	public float verticalRaySpacing; //espaçamento vertical entre os raycasts

    [Header("Collision")]
    public Collider2D characterCollider;
    public LayerMask collisionMask; //quais objetos este objeto colidirá
	public CollisionInfo collisions; //informações de colisão

	private float maxClimbAngle = 80f;
    private float maxDescendAngle = 75f;

	RaycastOrigins raycastOrigins; //de onde o raycast sairá

	//struct para guardar as coordenadas de onde os raycast sairão do objeto
	struct RaycastOrigins
	{
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}

	//struct para guardar informações da colisão
	public struct CollisionInfo
	{
		public bool above, below;
		public bool left, right;
		public bool climbingSlope;
        public bool descendingSlope;

		public float slopeAngle, slopeAngleOld;

        public Vector3 velocityOld;

		public void Reset()
		{
			above = below = left = right = false;
			climbingSlope = false;
            descendingSlope = false;

            slopeAngleOld = slopeAngle;
			slopeAngle = 0f;
		}
	}

	void Start()
	{
		try
		{
			if (characterCollider == null)
				throw new UnityException("É necessário ter o componente \"BoxCollider2D\".");

			CalculateRaySpacing (); //calcula espaçamento entre os raycasts
		}
		catch (System.Exception ex)
		{
			Debug.Log (ex.Message);
			return;
		}
	}

	//movimenta o objeto
	public void Move(Vector3 velocity)
	{
		UpdateRaycastOrigins ();
		collisions.Reset ();

        collisions.velocityOld = velocity;

        if (velocity.y < 0) //está caindo
        {
            DescendSlope(ref velocity); //ação de descer ladeira
        }

		if (velocity.x != 0) //está se movendo horizontalmente
		{
			HorizontalCollisions (ref velocity);
		}
		if (velocity.y != 0) //está se movendo verticalmente
		{
			VerticalCollisions (ref velocity);
		}

		transform.Translate (velocity); //aplica velocidade de movimento
	}

	//aplica as colisões horizontais
	void HorizontalCollisions(ref Vector3 velocity)
	{
		float directionX = Mathf.Sign (velocity.x); //direção da velocidade horizontal: -1 -> left; 1 -> right
		float rayLength = Mathf.Abs(velocity.x) + skinWidth; //define o tamanho do raycast

		//verifica as colisões para cada raycast definido
		for (int i = 0; i < horizontalRayCount; i++)
		{
			//define qual será a origem vertical do raycast
			Vector2 rayOrigin = (directionX == -1)? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);

			//efetua um raycast e retorna se encostou em algo na layer collisionMask
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			//desenha todos os raycasts
			//Vector2.right é utilizado para fazer a conta com bottomLeft utilizando um Vector2
			Debug.DrawRay (rayOrigin, Vector2.right * directionX * rayLength, Color.red);

			//colidiu com algo
			if (hit)
			{
				float slopeAngle = Vector2.Angle (hit.normal, Vector2.up); //pega o ângulo da direção perpendicular ao chão (normal) e da que sempre aponta para cima (up)
				if (i == 0 && slopeAngle <= maxClimbAngle) //está no primeiro raycast e pode subir pelo ângulo
				{
                    if (collisions.descendingSlope)
                    {
                        collisions.descendingSlope = false;
                        velocity = collisions.velocityOld;
                    }
					//evita com que o objeto fique flutuando um pouco acima do declive, devido ao raycast
					float distanceToSlopeStart = 0;
					if (slopeAngle != collisions.slopeAngleOld) //ângulo atual é diferente to ângulo do declive anterior
					{
						distanceToSlopeStart = hit.distance - skinWidth; //distância entre o objeto e o declive
						//subtrai da velocidade X a distância com o declive
						velocity.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope (ref velocity, slopeAngle); //faz o objeto continuar subindo
					//após subir, adiciona novamente na velocidade X a distância com o declive
					velocity.x += distanceToSlopeStart * directionX;
				}

				//não fazer o cálculo de velocidade x ao encostar em parede caso esteja em um declive
				if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
				{
					velocity.x = (hit.distance - skinWidth) * directionX;
					rayLength = hit.distance;

					//está subindo em um declive
					if (collisions.climbingSlope)
					{
						//faz a velocidade y ficar de acordo com o ângulo tangente da colisão e a velocidade x
						velocity.y = Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs (velocity.x);
					}

					collisions.left = directionX == -1; //colidiu na esquerda
					collisions.right = directionX == 1; //colidiu na direita
				}
			}
		}
	}

	void VerticalCollisions(ref Vector3 velocity)
	{
		float directionY = Mathf.Sign (velocity.y); //direção da velocidade vertical: -1 -> up; 1 -> down
		float rayLength = Mathf.Abs(velocity.y) + skinWidth; //define o tamanho do raycast

		//verifica as colisões para cada raycast definido
		for (int i = 0; i < verticalRayCount; i++)
		{
			//define qual será a origem vertical do raycast
			Vector2 rayOrigin = (directionY == -1)? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);

			//efetua um raycast e retorna se encostou em algo na layer collisionMask
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

			//desenha todos os raycasts
			//Vector2.right é utilizado para fazer a conta com bottomLeft utilizando um Vector2
			Debug.DrawRay (rayOrigin, Vector2.up * directionY * rayLength, Color.red);

			//colidiu com algo
			if (hit)
			{
				velocity.y = (hit.distance - skinWidth) * directionY;
				//define o tamanho do raycast para o da distância -> necessário para os raycast seguintes não serem influenciados pelo anterior
				rayLength = hit.distance;

				//evita o objeto de entrar verticalmente no objeto em que colidiu enquanto se estiver em um declive
				if (collisions.climbingSlope)
				{
					velocity.x = velocity.y / Mathf.Tan (collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign (velocity.x);
				}

				collisions.below = directionY == -1; //colidiu abaixo
				collisions.above = directionY == 1; //colidiu acima
			}

			if (collisions.climbingSlope)
			{
				float directionX = Mathf.Sign(velocity.x);
				rayLength = Mathf.Abs(velocity.x) + skinWidth;
				rayOrigin = ((directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight) + Vector2.up * velocity.y;
				hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

				if (hit) {
					float slopeAngle = Vector2.Angle(hit.normal,Vector2.up);
					if (slopeAngle != collisions.slopeAngle) {
						velocity.x = (hit.distance - skinWidth) * directionX;
						collisions.slopeAngle = slopeAngle;
					}
				}
			}
		}
	}

	void ClimbSlope(ref Vector3 velocity, float slopeAngle)
	{
		float moveDistance = Mathf.Abs (velocity.x); //pega a velocidade X absoluta
		//aplica a mesma velocidade X em cima do ângulo seno
		float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

		//só aplica a velocidade para subir o declive se a posição y é menor/igual que a nova velocidade y
		//ou seja, após a nova velocidade ser aplicada, só sera aplicada novamente quando encostar no declive
		if (velocity.y <= climbVelocityY)
		{
			//aplica as velocidades necessárias para fazer com que o objeto suba o declive
			velocity.y = climbVelocityY;
			velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);

			collisions.below = true; //indica que está no chão
			collisions.climbingSlope = true;
			collisions.slopeAngle = slopeAngle;
		}
	}

    void DescendSlope(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
            {
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descendVelocityY;

                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;
                    }
                }
            }
        }
    }

	//atualiza as coordenadas de origem do raycast de acordo com a posição atual do objeto
	void UpdateRaycastOrigins()
	{
		Bounds bounds = characterCollider.bounds; //pega os limites/beiradas da colisão
		bounds.Expand (skinWidth * -2); //move as beiradas para dentro de acordo com a largura de skinWidth

		//define as coordenadas de origem
		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
	}

	//calcula o espaço entre os raycasts
	void CalculateRaySpacing()
	{
		Bounds bounds = characterCollider.bounds; //pega os limites/beiradas da colisão
		bounds.Expand (skinWidth * -2); //move as beiradas para dentro de acordo com a largura de skinWidth

		//força as quantidades de raycasts a serem sempre maior ou igual a 2
		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp (verticalRayCount, 2, int.MaxValue);

		//calcula o espaçamento entre os raycasts
		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}
}
