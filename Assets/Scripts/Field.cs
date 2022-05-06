using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Field : MonoBehaviour
{
    //префабы обектов и счет
    public GameObject templateBubble;
    public GameObject scoreUI;
    public GameObject resultUI;

    private TextMeshProUGUI text;
    private string forScore = "Score: ";
    private int score;
    private int winPoints;

    //словари для нахождения одинаковых и повисших в воздухе шариков
    private Dictionary<string, Color> bubblesField = new Dictionary<string, Color>();
    private Dictionary<string, GameObject> gameObjectField = new Dictionary<string, GameObject>();   
    private Dictionary<string, Color> tempField = new Dictionary<string, Color>();
    private Dictionary<string, GameObject> tempGameObjectField = new Dictionary<string, GameObject>();

    //вспомогательные списки
    private List<string> strongRow = new List<string>();
    private List<GameObject> tempBubbles = new List<GameObject>();
    private List<GameObject> bubblesList = new List<GameObject>();
    private List<List<Color>> bubblesColor = new List<List<Color>>();

    //стартовые координаты для построения поля, диаметр и дистанция между шариками
    private float borderLineX = -1.85f;
    private float borderLineY = 4.55f;
    private float bubbleDiameter = 0.4f;
    private float distanceBetween = 0.01f;

    private bool gameOver;
    private bool check1 = true;
    private bool check2 = true;
    private float seconds = 0.1f;

    private void Start()
    {
        Read();
        GenerateBubbles();
    }

    //создаем поле шариков
    private void GenerateBubbles()
    {
        float offsetX = (bubbleDiameter + distanceBetween) / 2;
        float offsetY = 0.36f;
        float currentLineX;
        float currentLineY = borderLineY - (bubblesColor.Count - 1) * offsetY;

        List<GameObject> previousBubblesTemp = new List<GameObject>();

        text = scoreUI.GetComponent<TextMeshProUGUI>();
        text.text = forScore + score.ToString();

        for (int i = 0; i < bubblesColor.Count; i++)
        {
            //указываем есть ли смещение для ряда (10 или 11 шариков в ряду)
            if (bubblesColor[i].Count == 11)
            {
                currentLineX = borderLineX - offsetX;
            }
            else
            {
                currentLineX = borderLineX;
            }

            List<GameObject> bubblesTemp = new List<GameObject>();

            for (int j = 0; j < bubblesColor[i].Count; j++)
            {
                GameObject newBubble;
                Vector2 position = new Vector2(currentLineX, currentLineY);

                if (bubblesColor[i][j] != Color.clear)
                {
                    newBubble = Instantiate(templateBubble);
                    newBubble.GetComponent<Bubble>().SetOptions(bubblesColor[i][j], position);
                    newBubble.GetComponent<Bubble>().AllAnchors(position, offsetX, offsetY);

                    //при генерации поля шарик будет содержать информацию о цвете соседних с ним шариков
                    //проверка предыдущего шарика в одном ряду
                    if (j > 0)
                    {
                        if (bubblesColor[i][j] == bubblesColor[i][j - 1])
                        {
                            newBubble.GetComponent<Bubble>().Neighbours(bubblesTemp[j - 1]);
                            bubblesTemp[j - 1].GetComponent<Bubble>().Neighbours(newBubble);
                        }
                        //информация о соседнем шарике любого цвета
                        if (bubblesColor[i][j - 1] != Color.clear)
                        {
                            newBubble.GetComponent<Bubble>().BusyNeighbors(bubblesTemp[j - 1]);
                            bubblesTemp[j - 1].GetComponent<Bubble>().BusyNeighbors(newBubble);
                        }
                    }

                    //проверка нижнего ряда
                    if (i > 0)
                    {
                        if (bubblesColor[i].Count == 11)
                        {
                            if (j < 10)
                            {
                                if (bubblesColor[i][j] == bubblesColor[i - 1][j])
                                {
                                    newBubble.GetComponent<Bubble>().Neighbours(previousBubblesTemp[j]);
                                    previousBubblesTemp[j].GetComponent<Bubble>().Neighbours(newBubble);
                                }
                                //информация о соседнем шарике любого цвета
                                if (bubblesColor[i - 1][j] != Color.clear)
                                {
                                    newBubble.GetComponent<Bubble>().BusyNeighbors(previousBubblesTemp[j]);
                                    previousBubblesTemp[j].GetComponent<Bubble>().BusyNeighbors(newBubble);
                                }  
                            }
                            if (j > 0)
                            {
                                if (bubblesColor[i][j] == bubblesColor[i - 1][j - 1])
                                {
                                    newBubble.GetComponent<Bubble>().Neighbours(previousBubblesTemp[j - 1]);
                                    previousBubblesTemp[j - 1].GetComponent<Bubble>().Neighbours(newBubble);
                                }
                                //информация о соседнем шарике любого цвета
                                if (bubblesColor[i - 1][j - 1] != Color.clear)
                                {
                                    newBubble.GetComponent<Bubble>().BusyNeighbors(previousBubblesTemp[j - 1]);
                                    previousBubblesTemp[j - 1].GetComponent<Bubble>().BusyNeighbors(newBubble);
                                }
                            }
                        }
                        else
                        {
                            if (bubblesColor[i][j] == bubblesColor[i - 1][j])
                            {
                                newBubble.GetComponent<Bubble>().Neighbours(previousBubblesTemp[j]);
                                previousBubblesTemp[j].GetComponent<Bubble>().Neighbours(newBubble);
                            }
                            if (bubblesColor[i][j] == bubblesColor[i - 1][j + 1])
                            {
                                newBubble.GetComponent<Bubble>().Neighbours(previousBubblesTemp[j + 1]);
                                previousBubblesTemp[j + 1].GetComponent<Bubble>().Neighbours(newBubble);
                            }
                            //информация о соседнем шарике любого цвета
                            if (bubblesColor[i - 1][j] != Color.clear)
                            {
                                newBubble.GetComponent<Bubble>().BusyNeighbors(previousBubblesTemp[j]);
                                previousBubblesTemp[j].GetComponent<Bubble>().BusyNeighbors(newBubble);
                            }
                            if (bubblesColor[i - 1][j + 1] != Color.clear)
                            {
                                newBubble.GetComponent<Bubble>().BusyNeighbors(previousBubblesTemp[j + 1]);
                                previousBubblesTemp[j + 1].GetComponent<Bubble>().BusyNeighbors(newBubble);
                            }
                        }
                    }
                }
                else
                {
                    newBubble = new GameObject();
                }
                bubblesTemp.Add(newBubble);
                bubblesList.Add(newBubble);

                //формируем ключ для словарей из координат X и Y с одним знаком после запятой
                int value = 1;
                double positionX = Math.Round(currentLineX, value);
                double positionY = Math.Round(currentLineY, value);
                string positionKey = positionX.ToString() + positionY.ToString();
                bubblesField.Add(positionKey, bubblesColor[i][j]);
                gameObjectField.Add(positionKey, newBubble);

                //список ключей верхнего ряда для поиска зависших в воздухе шариков и контроля счета
                if (i == bubblesColor.Count - 1)
                {
                    strongRow.Add(positionKey);
                }
                currentLineX = currentLineX + bubbleDiameter + distanceBetween;
            }
            
            previousBubblesTemp.Clear();
            previousBubblesTemp = bubblesTemp;
            currentLineY = currentLineY + offsetY;
        }

        //количество шариков верхнего ряда которые должны остаться для победы
        winPoints = (int)Math.Round(strongRow.Count / 100f * 30f);
    }

    //прибавление очков и контроль выигрыша
    public void PlusPoints()
    {
        if (winPoints < strongRow.Count)
        {
            score++;
            text.text = forScore + score.ToString();
        }
        else
        {
            if (gameOver == false)
            {
                gameOver = true;
                WinResult();
            }  
        }
    }

    //окно выигрыша
    private void WinResult()
    {
        GameObject winResult = Instantiate(resultUI);
        GameObject FoundCanvas = GameObject.Find("Canvas");
        winResult.transform.SetParent(FoundCanvas.transform, false);
        winResult.GetComponentInChildren<TextMeshProUGUI>().text = "Выигрыш! <br>" + forScore + score;
        winResult.GetComponentInChildren<Button>().onClick.AddListener(ToMenu);
        //запрещаем передвигать мяч после появления окна
        GameObject spawner = GameObject.Find("Spawner");
        spawner.GetComponent<BallSpawner>().newBall.GetComponent<CircleCollider2D>().enabled = false;

        //оставшиеся шарики падают
        for (int i = 0; i < bubblesList.Count; i++)
        {
            if (bubblesList[i].GetComponent<SpringJoint2D>())
            {
                bubblesList[i].GetComponent<SpringJoint2D>().enabled = false;
            }
        }
    }

    //окно проигрыша
    public void LossResult()
    {
        GameObject winResult = Instantiate(resultUI);
        GameObject FoundCanvas = GameObject.Find("Canvas");
        winResult.transform.SetParent(FoundCanvas.transform, false);
        winResult.GetComponentInChildren<TextMeshProUGUI>().text = "Проигрыш! <br>Закончились мячи";
        winResult.GetComponentInChildren<Button>().onClick.AddListener(ToMenu);
        //запрещаем передвигать мяч после появления окна
        GameObject spawner = GameObject.Find("Spawner");
        spawner.GetComponent<BallSpawner>().newBall.GetComponent<CircleCollider2D>().enabled = false;  
    }

    private void ToMenu()
    {
        GetComponent<UIManager>().BackToMenu();
    }

    //удаляем замещенный шарик из списков
    public void Remove(string positionKey)
    {
        GameObject deletedObject = gameObjectField[positionKey];

        if (bubblesField.ContainsKey(positionKey))
        {
            bubblesField.Remove(positionKey);
            gameObjectField.Remove(positionKey);
        }
        bubblesList.Remove(deletedObject);
    }

    //добавляем прилипший или заместивший другой шарик мяч к спискам остальных шариков
    public void Add(string positionKey, Color bubbleColor, GameObject gameObject)
    {
        if (bubblesField.ContainsKey(positionKey))
        {
            bubblesField.Remove(positionKey);
            bubblesField.Add(positionKey, bubbleColor);

            GameObject deletedObject = gameObjectField[positionKey];
            bubblesList.Remove(deletedObject);
            bubblesList.Add(gameObject);

            gameObjectField.Remove(positionKey);
            gameObjectField.Add(positionKey, gameObject);
        }
        else
        {
            bubblesField.Add(positionKey, bubbleColor);
            bubblesList.Add(gameObject);
            gameObjectField.Add(positionKey, gameObject);
        }  
    }

    //формируем список для текущей проверки одинаковых шариков
    public void BurstList(string positionKey, Color bubbleColor, GameObject gameObject)
    {
        if (!tempField.ContainsKey(positionKey))
        {
            tempField.Add(positionKey, bubbleColor);
            tempBubbles.Add(gameObject);
        }
    }

    //после того как все шарики от исходного пересчитали одинаковых соседей
    public void BurstWhenAllCollect()
    {
        if (check1 == true)
        {
            check1 = false;
            StartCoroutine(WaitTime());
        }
    }

    //удаляем шарики из списков и лопаем их
    public IEnumerator WaitTime()
    {
        yield return new WaitForSeconds(seconds);

        for (int i = 0; i < tempBubbles.Count; i++)
        {
            string key = tempBubbles[i].GetComponent<Bubble>().GetKey();
            Color color = Color.clear;
            GameObject emptyBubble = new GameObject();

            if (bubblesField.ContainsKey(key))
            {
                if (bubblesField[key] != color)
                {
                    bubblesField.Remove(key);
                    bubblesField.Add(key, color);

                    GameObject deletedObject = gameObjectField[key];
                    for (int j = 0; j < bubblesList.Count; j++)
                    {
                        if (bubblesList[j] == deletedObject)
                        {
                            bubblesList[j].GetComponent<Bubble>().CheckForFall();
                            bubblesList.Remove(deletedObject);
                        }
                    }
                    gameObjectField.Remove(key);
                    gameObjectField.Add(key, emptyBubble);
                }
                strongRow.Remove(key);
            }
            Destroy(tempBubbles[i]);
        }
        tempField.Clear();
        tempBubbles.Clear();
        tempGameObjectField.Clear();
        tempGameObjectField = new Dictionary<string, GameObject>(gameObjectField);
        StartCoroutine(CheckFallDown());
        check1 = true;
    }

    //проверяем есть ли зависшие в воздухе шарики
    IEnumerator CheckFallDown()
    {
        yield return new WaitForSeconds(seconds);

        if (check2 == true)
        {
            check2 = false;

            for (int i = 0; i < strongRow.Count; i++)
            {
                if (tempGameObjectField.ContainsKey(strongRow[i]))
                {
                    if (tempGameObjectField[strongRow[i]].GetComponent<Bubble>() != null)
                    {
                        tempGameObjectField[strongRow[i]].GetComponent<Bubble>().CheckForFall2();
                    }
                    tempGameObjectField.Remove(strongRow[i]);
                }
            }
            StartCoroutine(WaitTime2());
        }
    }

    //шарик может вызвать эту функцию и проверить есть ли он в словаре
    public bool CheckFallDown2(string key)
    {
        if (tempGameObjectField.ContainsKey(key))
        {
            tempGameObjectField.Remove(key);
            return true;
        }
        else
        {
            return false;
        }
    }

    //найденные зависшие в воздухе шарики удаляем из списков и отправляем в падение
    IEnumerator WaitTime2()
    {
        yield return CheckFallDown();

        for (int i = 0; i < bubblesList.Count; i++)
        {
            if (bubblesList[i].GetComponent<Bubble>() != null)
            {
                string key = bubblesList[i].GetComponent<Bubble>().GetKey();

                if (tempGameObjectField.ContainsKey(key))
                {
                    tempBubbles.Add(tempGameObjectField[key]);
                }
            } 
        }

        for (int i = 0; i < tempBubbles.Count; i++)
        {
            string key = tempBubbles[i].GetComponent<Bubble>().GetKey();

            if (gameObjectField.ContainsKey(key))
            {
                tempField.Remove(key);
                bubblesField.Remove(key);
                gameObjectField.Remove(key);
                bubblesList.Remove(tempBubbles[i]);
            }

            tempBubbles[i].tag = "Untagged";
            tempBubbles[i].GetComponent<SpringJoint2D>().enabled = false;
        }

        check2 = true;
        tempField.Clear();
        tempBubbles.Clear();
    }

    private Color GetColor(string color)
    {
        Color bubbleColor;
        switch (color)
        {
            case "None":
                bubbleColor = Color.clear;
                break;
            case "Yellow":
                bubbleColor = Color.yellow;
                break;
            case "Blue":
                bubbleColor = Color.blue;
                break;
            case "Red":
                bubbleColor = Color.red;
                break;
            case "Green":
                bubbleColor = Color.green;
                break;
            default:
                bubbleColor = Color.clear;
                break;
        }
        return bubbleColor;
    }

    //читаем данные из файла
    private void Read()
    {
        string path = Application.streamingAssetsPath + "/field.csv";
        if (File.Exists(path))
        {
            string[] Lines = File.ReadAllLines(path);
            for (int i = Lines.Length - 1; i >= 0; i--)
            {
                string[] Columns = Lines[i].Split(new string[] { "	" }, StringSplitOptions.RemoveEmptyEntries);
                List<Color> bubblesColorTemp = new List<Color>();
                for (int j = 0; j < Columns.Length; j++)
                {
                    Color color = GetColor(Columns[j]);
                    bubblesColorTemp.Add(color);
                }
                bubblesColor.Add(bubblesColorTemp);
            }
        }
    }
}
