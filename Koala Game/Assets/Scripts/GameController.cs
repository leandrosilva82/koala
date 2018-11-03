using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("HUD")]
    [Tooltip("String that is showed before the lives' number")]


    private string livesPrefix, scorePrefix; //string mostrada antes do número de vidas
    private Player player;
    private Text hudLivesText, hudScoreText;

    // Use this for initialization
    void Start()
    {
        Init();
        HUDUpdate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        hudLivesText = GameObject.FindGameObjectWithTag("UI-HUDLivesText").GetComponent<Text>();
        livesPrefix = hudLivesText.text;

        hudScoreText = GameObject.FindGameObjectWithTag("UI-HUDScoreText").GetComponent<Text>();
        scorePrefix = hudScoreText.text;
    }

    public void HUDUpdate()
    {
        HUDUpdateLives();
        HUDUpdateScore();
    }
    public void HUDUpdateLives()
    {
        int lives = player.lives >= 0? player.lives : 0; //pega a quantidade de vids do jogador. Se menor que 0, então exibir 0
        hudLivesText.text = livesPrefix + " " + lives;
    }
    public void HUDUpdateScore()
    {
        int score = player.GetScore();
        hudScoreText.text = scorePrefix + " " + score;
    }
}
