using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    public GameObject explosionEffect;

    //список содержит инфо о занятых соседних позициях
    public List<GameObject> busyNeighbors = new List<GameObject>();
    //список каждого шарика содержит информацию о соседях того же цвета
    private List<GameObject> sameNeighbors = new List<GameObject>();
    //список содержит инфо о всех/свободных соседних позициях
    private List<Vector2> allAnchors = new List<Vector2>();
    public List<Vector2> freeAnchors;

    private SpriteRenderer bubbleRenderer;
    private Transform bubblePosition;
    private GameObject manager;

    private string positionKey;
    private Color bubbleColor;

    private float getOffsetX;
    private float getOffsetY;

    //параметры при появлении шарика
    public void SetOptions(Color color, Vector2 position)
    {
        bubbleRenderer.color = color;
        bubblePosition.position = position;

        SpringJoint2D springJoint2D = gameObject.AddComponent<SpringJoint2D>();
        springJoint2D.connectedAnchor = position;
        springJoint2D.autoConfigureDistance = false;
        springJoint2D.distance = 0.005f;
        springJoint2D.dampingRatio = 0.2f;

        Rigidbody2D rigidbody = gameObject.GetComponent<Rigidbody2D>();
        rigidbody.angularDrag = 2f;
        rigidbody.drag = 2f;

        manager = GameObject.Find("Manager");
        bubbleColor = bubbleRenderer.color;

        //индивидуальный ключ шарика в зависимости от координат
        int value = 1;
        double positionX = Math.Round(position.x, value);
        double positionY = Math.Round(position.y, value);
        positionKey = positionX.ToString() + positionY.ToString();
    }

    //информация о координатах соседних точек
    public void AllAnchors(Vector2 position, float offsetX, float offsetY)
    {
        allAnchors.Add(new Vector2(position.x - offsetX * 2f, position.y));
        allAnchors.Add(new Vector2(position.x + offsetX * 2f, position.y));
        allAnchors.Add(new Vector2(position.x - offsetX, position.y - offsetY));
        allAnchors.Add(new Vector2(position.x - offsetX, position.y + offsetY));
        allAnchors.Add(new Vector2(position.x + offsetX, position.y - offsetY));
        allAnchors.Add(new Vector2(position.x + offsetX, position.y + offsetY));

        freeAnchors = new List<Vector2>(allAnchors);

        getOffsetX = offsetX;
        getOffsetY = offsetY;
    }

    //передать в ударивший шарик информацию о смещениях
    public float[] Offsets()
    {
        float[] offsets = { getOffsetX, getOffsetY };
        return offsets;
    }

    public Color GetColor()
    {
        Color color = bubbleColor;
        return color;
    }

    public string GetKey()
    {
        return positionKey;
    }

    //добавляем "соседа" того же цвета
    public void Neighbours(GameObject neighbour)
    {
        sameNeighbors.Add(neighbour);
    }

    //добавляем информацию о всех соседях
    public void BusyNeighbors(GameObject neighbour)
    {
        busyNeighbors.Add(neighbour);

        int value = 1;
        double neighbourX = Math.Round(neighbour.GetComponent<SpringJoint2D>().connectedAnchor.x, value);
        double neighbourY = Math.Round(neighbour.GetComponent<SpringJoint2D>().connectedAnchor.y, value);

        for (int i = 0; i < freeAnchors.Count; i++)
        {
            double freeAnchorX = Math.Round(freeAnchors[i].x, value);
            double freeAnchorY = Math.Round(freeAnchors[i].y, value);

            if (neighbourX == freeAnchorX && neighbourY == freeAnchorY)
            {
                freeAnchors.RemoveAt(i);
            }
        }
    }

    //возвращаем доступную точку привязки, ближайшую к месту удара
    public Vector2 NearestPoint(Vector2 hitPoint)
    {
        Vector2 finalPoint = new Vector2();
        float distance = 100f;

        for (int i = 0; i < freeAnchors.Count; i++)
        {
            float newDistance = (freeAnchors[i] - hitPoint).sqrMagnitude;
            if (distance > newDistance)
            {
                distance = newDistance;
                finalPoint = freeAnchors[i];
            }
        }
        return finalPoint;
    }

    //шарик добавляется в списки со своими параметрами
    public void AddToField()
    {
        manager.GetComponent<Field>().Add(positionKey, bubbleColor, gameObject);
    }

    //шарик удаляется из списков
    public void RemoveFromField()
    {
        manager.GetComponent<Field>().Remove(positionKey);
    }

    //если шарик содержит достаточное количество соседей одинакового цвета, то будем составлять список всех соседей
    public void CheckForBurst()
    {
        if (sameNeighbors.Count > 1)
        {
            manager.GetComponent<Field>().BurstList(positionKey, bubbleColor, gameObject);
            CheckForBurst2();
        }
        if (sameNeighbors.Count == 1)
        {
            if (sameNeighbors[0].GetComponent<Bubble>().sameNeighbors.Count > 1)
            {
                manager.GetComponent<Field>().BurstList(positionKey, bubbleColor, gameObject);
                CheckForBurst2();
            }
        }
    }

    //составление списка одинаковых соседей и вызов для них этой же функции
    public void CheckForBurst2()
    {
        manager.GetComponent<Field>().BurstList(positionKey, bubbleColor, gameObject);
        List<GameObject> tempNeighbors = new List<GameObject>();

        for (int i = 0; i < sameNeighbors.Count; i++)
        {
            string neighborsKey = sameNeighbors[i].GetComponent<Bubble>().GetKey();
            Color neighborsColor = sameNeighbors[i].GetComponent<Bubble>().GetColor();

            manager.GetComponent<Field>().BurstList(neighborsKey, neighborsColor, sameNeighbors[i]);
            sameNeighbors[i].GetComponent<Bubble>().DeleteSameNeighbors(gameObject);
            tempNeighbors.Add(sameNeighbors[i]);
        }

        for (int i = 0; i < tempNeighbors.Count; i++)
        {
            tempNeighbors[i].GetComponent<Bubble>().CheckForBurst2();
        }

        if (tempNeighbors.Count == 0)
        {
            manager.GetComponent<Field>().BurstWhenAllCollect();
        }
    }

    //подготовка шарика к падению очищением от него списков соседей
    public void CheckForFall()
    {
        for (int i = 0; i < busyNeighbors.Count; i++)
        {
            busyNeighbors[i].GetComponent<Bubble>().DeleteBusyNeighbors(gameObject);
        }
    }

    //составление списка соседей падающих шариков и вызов для них этой же функции
    public void CheckForFall2()
    {
        List<GameObject> tempNeighbors = new List<GameObject>();

        for (int i = 0; i < busyNeighbors.Count; i++)
        {
            string neighborsKey = busyNeighbors[i].GetComponent<Bubble>().GetKey();
            if (manager.GetComponent<Field>().CheckFallDown2(neighborsKey))
            {
                tempNeighbors.Add(busyNeighbors[i]);
            } 
        }

        for (int i = 0; i < tempNeighbors.Count; i++)
        {
            tempNeighbors[i].GetComponent<Bubble>().CheckForFall2();
        }
    }

    //подготовка к удалению шарика очищением от него списков соседей
    public void Delete()
    {
        for (int i = 0; i < busyNeighbors.Count; i++)
        {
            busyNeighbors[i].GetComponent<Bubble>().DeleteBusyNeighbors(gameObject);
        }
        for (int i = 0; i < sameNeighbors.Count; i++)
        {
            sameNeighbors[i].GetComponent<Bubble>().DeleteSameNeighbors(gameObject);
        }
    }

    //очищение списка соседей и освобождение доступных координат
    public void DeleteBusyNeighbors(GameObject neighbour)
    {
        for (int i = 0; i < busyNeighbors.Count; i++)
        {
            if (busyNeighbors[i] == neighbour)
            {
                int value = 1;
                double neighbourX = Math.Round(neighbour.GetComponent<SpringJoint2D>().connectedAnchor.x, value);
                double neighbourY = Math.Round(neighbour.GetComponent<SpringJoint2D>().connectedAnchor.y, value);

                for (int j = 0; j < allAnchors.Count; j++)
                {
                    double anchorX = Math.Round(allAnchors[j].x, value);
                    double anchorY = Math.Round(allAnchors[j].y, value);

                    if (neighbourX == anchorX && neighbourY == anchorY)
                    {
                        freeAnchors.Add(allAnchors[j]);
                    }
                }
                busyNeighbors.RemoveAt(i);
            }
        }
    }

    //очищение списка одинаковых соседей
    public void DeleteSameNeighbors(GameObject neighbour)
    {
        for (int i = 0; i < sameNeighbors.Count; i++)
        {
            if (sameNeighbors[i] == neighbour)
            {
                sameNeighbors.RemoveAt(i);
            }
        }
    }

    private void Awake()
    {
        bubbleRenderer = gameObject.GetComponent<SpriteRenderer>();
        bubblePosition = gameObject.transform;
    }

    //при падении на "землю" шарики взрываются
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (gameObject.scene.isLoaded)
        {
            GameObject explosion = Instantiate(explosionEffect);
            explosion.transform.position = transform.position;

            if (manager)
            {
                manager.GetComponent<Field>().PlusPoints();
            }
        }
    }

}
