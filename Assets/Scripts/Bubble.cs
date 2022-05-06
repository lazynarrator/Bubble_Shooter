using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    public GameObject explosionEffect;

    //������ �������� ���� � ������� �������� ��������
    public List<GameObject> busyNeighbors = new List<GameObject>();
    //������ ������� ������ �������� ���������� � ������� ���� �� �����
    private List<GameObject> sameNeighbors = new List<GameObject>();
    //������ �������� ���� � ����/��������� �������� ��������
    private List<Vector2> allAnchors = new List<Vector2>();
    public List<Vector2> freeAnchors;

    private SpriteRenderer bubbleRenderer;
    private Transform bubblePosition;
    private GameObject manager;

    private string positionKey;
    private Color bubbleColor;

    private float getOffsetX;
    private float getOffsetY;

    //��������� ��� ��������� ������
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

        //�������������� ���� ������ � ����������� �� ���������
        int value = 1;
        double positionX = Math.Round(position.x, value);
        double positionY = Math.Round(position.y, value);
        positionKey = positionX.ToString() + positionY.ToString();
    }

    //���������� � ����������� �������� �����
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

    //�������� � ��������� ����� ���������� � ���������
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

    //��������� "������" ���� �� �����
    public void Neighbours(GameObject neighbour)
    {
        sameNeighbors.Add(neighbour);
    }

    //��������� ���������� � ���� �������
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

    //���������� ��������� ����� ��������, ��������� � ����� �����
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

    //����� ����������� � ������ �� ������ �����������
    public void AddToField()
    {
        manager.GetComponent<Field>().Add(positionKey, bubbleColor, gameObject);
    }

    //����� ��������� �� �������
    public void RemoveFromField()
    {
        manager.GetComponent<Field>().Remove(positionKey);
    }

    //���� ����� �������� ����������� ���������� ������� ����������� �����, �� ����� ���������� ������ ���� �������
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

    //����������� ������ ���������� ������� � ����� ��� ��� ���� �� �������
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

    //���������� ������ � ������� ��������� �� ���� ������� �������
    public void CheckForFall()
    {
        for (int i = 0; i < busyNeighbors.Count; i++)
        {
            busyNeighbors[i].GetComponent<Bubble>().DeleteBusyNeighbors(gameObject);
        }
    }

    //����������� ������ ������� �������� ������� � ����� ��� ��� ���� �� �������
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

    //���������� � �������� ������ ��������� �� ���� ������� �������
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

    //�������� ������ ������� � ������������ ��������� ���������
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

    //�������� ������ ���������� �������
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

    //��� ������� �� "�����" ������ ����������
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
