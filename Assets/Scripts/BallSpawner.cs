using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BallSpawner : MonoBehaviour
{
    public int ballsNumber = 10;

    public GameObject ball;
    public GameObject emptyBall;
    public GameObject newBall;

    private GameObject empty;
    private Color[] ballsColors;
    private TextMeshPro text;
    private int currentNumber = 1;

    private void Start()
    {
        Colors();
        Create();
        CreateEmpty();
    }

    private void Create()
    {
        if (currentNumber <= ballsNumber)
        {
            newBall = Instantiate(ball);
            newBall.GetComponent<SpriteRenderer>().color = ballsColors[currentNumber - 1];
        } 
    }

    private void CreateEmpty()
    {
        if (currentNumber <= ballsNumber)
        {
            empty = Instantiate(emptyBall);
            empty.GetComponent<SpriteRenderer>().color = ballsColors[currentNumber];
            text = empty.GetComponentInChildren<TextMeshPro>();
            text.text = (ballsNumber - currentNumber).ToString();
        }
    }

    private void Colors()
    {
        ballsColors = new Color[ballsNumber];
        Color[] colors = { Color.yellow, Color.blue, Color.red, Color.green };

        for (int i = 0; i < ballsColors.Length; i++)
        {
            Color color = colors[Random.Range(0, colors.Length)];
            ballsColors[i] = color;
        }
    }
    
    public void NewCreate()
    {
        if (currentNumber < ballsNumber)
        {
            currentNumber++;
            Create();

            if (currentNumber < ballsNumber)
            {
                empty.GetComponent<SpriteRenderer>().color = ballsColors[currentNumber];
                text.text = (ballsNumber - currentNumber).ToString();
            }
            else
            {
                Destroy(empty);
            }
        }
        else
        {
            GameObject manager = GameObject.Find("Manager");
            manager.GetComponent<Field>().LossResult();
        }
    }

}
