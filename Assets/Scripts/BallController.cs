using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    //префабы для отрисовки линий
    public LineRenderer blackLine;
    public LineRenderer redLine;
    public LineRenderer invisibleLine;

    private LineRenderer invisibleRenderer;
    private LineRenderer myLine;
    private EdgeCollider2D edgeCollider;
    private List<LineRenderer> blackRenderers = new List<LineRenderer>();
    private List<LineRenderer> redRenderers = new List<LineRenderer>();
    private SpriteRenderer ballRenderer;
    private Rigidbody2D newRigidbody;

    //координаты ограничения передвижения шарика
    private float stopLine = -3.1f;
    private float stopLine1 = -3.0f;
    private float stopLeftX = -2.2f;
    private float stopRightX = 2.2f;
    private float stopDownY = -4.7f;

    //коэффициент величины угла при натяжении
    private float spreadAngle = 2.5f;

    private float seconds = 0.3f;
    private float startTime;
    private float speed = 1f;
    private float variableSpeed = 5f;
    private float journeyLength;
    
    private Vector3[] blackPoints = new Vector3[3];
    private Vector3[] redPoints = new Vector3[5];
    private Vector3 emptyValue = new Vector3(0f, 0f, 0f);
    private Vector3 startPoint = new Vector3(0f, -3f, 0f);
    private Vector3 transformPositionOld = new Vector3();
    private Vector3 transformPositionNew = new Vector3();
    private List<Vector2> blackHit = new List<Vector2>();
    private List<Vector2> redHit = new List<Vector2>();
    private Vector2 stateHit;

    private bool isSpawner;
    private bool isSpringJoint;
    private bool fly1;
    private bool fly2;
    private bool forseFly;

    private Ray2D ray;
    private Ray2D ray2;

    //расчет падения и отклонения лучей
    private void LineDirection()
    {
        Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 firstTarget = new Vector2(startPoint.x - position.x, startPoint.y - position.y);
        transform.position = position;
        blackPoints[0] = position;
        ray = new Ray2D(position, firstTarget);
        int layerBorder = 1 << 6;
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 20.0f, layerBorder);

        if (hit.collider != null)
        {
            blackPoints[1] = hit.point;
            Debug.DrawRay(ray.origin, ray.direction * 20.0f, Color.green);

            //вспомогательная точка виртуального треугольника для построения корректного угла луча
            float virtualPointY = hit.point.y + Mathf.Abs(hit.point.y - transform.position.y);
            Vector2 virtualPoint = new Vector2(transform.position.x, virtualPointY);
            Vector2 secondTarget = virtualPoint - hit.point;
            ray2 = new Ray2D(blackPoints[1], secondTarget);
            RaycastHit2D hit2 = Physics2D.Raycast(ray2.origin, ray2.direction, Mathf.Infinity, layerBorder);
            Vector3 startDistance = startPoint - transform.position;

            //если шарик передвинут на расстояние, при котором появляется разброс луча
            if (startDistance.magnitude > 0.8f)
            {
                float angleFactor = startDistance.magnitude * spreadAngle;
                Vector2 redLine = new Vector2(ray.direction.x, ray.direction.y);
                Vector2 redLine1 = Quaternion.Euler(angleFactor, angleFactor, angleFactor) * redLine;
                Vector2 redLine2 = Quaternion.Euler(-angleFactor, -angleFactor, -angleFactor) * redLine;

                Ray2D ray3 = new Ray2D(position, redLine1);
                Ray2D ray4 = new Ray2D(position, redLine2);
                RaycastHit2D hit3 = Physics2D.Raycast(ray3.origin, ray3.direction, 20.0f, layerBorder);
                RaycastHit2D hit4 = Physics2D.Raycast(ray4.origin, ray4.direction, 20.0f, layerBorder);

                Debug.DrawRay(ray3.origin, ray3.direction * 20.0f, Color.magenta);
                Debug.DrawRay(ray4.origin, ray4.direction * 20.0f, Color.cyan);

                int layerLine = 1 << 7;

                if (hit.collider.tag == "SideWalls" || hit3.collider.tag == "SideWalls" || hit4.collider.tag == "SideWalls")
                {

                    Ray2D ray5 = new Ray2D(hit3.point, ray2.direction);
                    Ray2D ray6 = new Ray2D(hit4.point, ray2.direction);

                    RaycastHit2D hit5 = Physics2D.Raycast(ray5.origin, ray5.direction, 20.0f, layerBorder);
                    RaycastHit2D hit6 = Physics2D.Raycast(ray6.origin, ray6.direction, 20.0f, layerBorder);

                    Debug.DrawRay(ray5.origin, ray5.direction * 20.0f, Color.magenta);
                    Debug.DrawRay(ray6.origin, ray6.direction * 20.0f, Color.cyan);

                    if (hit3.point.y > hit4.point.y)
                    {
                        RaycastHit2D helpHit = Physics2D.Raycast(ray6.origin, ray6.direction, 20.0f, layerLine);

                        redPoints[0] = hit3.point;
                        redPoints[1] = hit4.point;
                        redPoints[3] = hit6.point;
                        redPoints[4] = hit5.point;

                        if (hit3.collider.tag == "UpperWall")
                        {
                            redPoints[2] = hit3.point;

                            redRenderers[2].SetPosition(0, redPoints[2]);
                            redRenderers[2].SetPosition(1, redPoints[2]);

                            redRenderers[3].SetPosition(0, redPoints[0]);
                            redRenderers[3].SetPosition(1, redPoints[0]);
                        }
                        else
                        {
                            if (hit3.collider.tag == "Bubble" || hit4.collider.tag == "Bubble")
                            {
                                if (hit3.collider.tag == "Bubble")
                                {
                                    redPoints[2] = hit3.point;

                                    redRenderers[2].SetPosition(0, redPoints[2]);
                                    redRenderers[2].SetPosition(1, redPoints[2]);

                                    redRenderers[3].SetPosition(0, redPoints[0]);
                                    redRenderers[3].SetPosition(1, redPoints[0]);
                                }
                                if (hit4.collider.tag == "Bubble")
                                {
                                    redPoints[2] = hit3.point;

                                    redRenderers[2].SetPosition(0, redPoints[2]);
                                    redRenderers[2].SetPosition(1, redPoints[2]);

                                    redRenderers[3].SetPosition(0, redPoints[0]);
                                    redRenderers[3].SetPosition(1, redPoints[0]);
                                }
                            }
                            else
                            {
                                redPoints[2] = helpHit.point;
                                redRenderers[2].SetPosition(0, redPoints[2]);
                                redRenderers[2].SetPosition(1, redPoints[3]);

                                redRenderers[3].SetPosition(0, redPoints[0]);
                                redRenderers[3].SetPosition(1, redPoints[4]);
                            }
                        }
                    }
                    else
                    {
                        RaycastHit2D helpHit = Physics2D.Raycast(ray5.origin, ray5.direction, 20.0f, layerLine);

                        redPoints[0] = hit4.point;
                        redPoints[2] = hit3.point;
                        redPoints[3] = hit5.point;
                        redPoints[4] = hit6.point;

                        if (hit4.collider.tag == "UpperWall")
                        {
                            redPoints[1] = hit4.point;

                            redRenderers[2].SetPosition(0, redPoints[1]);
                            redRenderers[2].SetPosition(1, redPoints[1]);

                            redRenderers[3].SetPosition(0, redPoints[0]);
                            redRenderers[3].SetPosition(1, redPoints[0]);
                        }
                        else
                        {
                            if (hit3.collider.tag == "Bubble" || hit4.collider.tag == "Bubble")
                            {
                                if (hit4.collider.tag == "Bubble")
                                {
                                    redPoints[1] = hit4.point;

                                    redRenderers[2].SetPosition(0, redPoints[1]);
                                    redRenderers[2].SetPosition(1, redPoints[1]);

                                    redRenderers[3].SetPosition(0, redPoints[0]);
                                    redRenderers[3].SetPosition(1, redPoints[0]);
                                }
                                if (hit3.collider.tag == "Bubble")
                                {
                                    redPoints[1] = hit4.point;

                                    redRenderers[2].SetPosition(0, redPoints[1]);
                                    redRenderers[2].SetPosition(1, redPoints[1]);

                                    redRenderers[3].SetPosition(0, redPoints[0]);
                                    redRenderers[3].SetPosition(1, redPoints[0]);
                                }
                            }
                            else
                            {
                                redPoints[1] = helpHit.point;
                                redRenderers[2].SetPosition(0, redPoints[1]);
                                redRenderers[2].SetPosition(1, redPoints[3]);

                                redRenderers[3].SetPosition(0, redPoints[0]);
                                redRenderers[3].SetPosition(1, redPoints[4]);
                            }  
                        }
                    }
                }
                else
                {
                    redPoints[0] = blackPoints[0];
                    redPoints[1] = hit3.point;
                    redPoints[2] = hit4.point;
                }  
            }
            else
            {
                if (hit.collider.tag == "SideWalls")
                {
                    blackPoints[2] = hit2.point;
                    Debug.DrawRay(ray2.origin, ray2.direction * 10.0f, Color.blue);
                }
                else
                {
                    blackPoints[2] = blackPoints[1];
                }
            }
        }
        HitCheck();
    }

    //отрисовка черных и красных линий
    private void LineRendering()
    {
        Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (position.y <= stopLine)
        {
            Vector3 startDistance = startPoint - transform.position;

            if (startDistance.magnitude > 0.8f)
            {
                for (int i = 0; i < blackRenderers.Count; i++)
                {
                    blackRenderers[i].SetPosition(0, emptyValue);
                    blackRenderers[i].SetPosition(1, emptyValue);
                }

                invisibleRenderer.SetPosition(0, blackPoints[0]);
                invisibleRenderer.SetPosition(1, redPoints[0]);

                redRenderers[0].SetPosition(0, blackPoints[0]);
                redRenderers[0].SetPosition(1, redPoints[1]);

                redRenderers[1].SetPosition(0, blackPoints[0]);
                redRenderers[1].SetPosition(1, redPoints[2]);
            }
            else
            {
                blackRenderers[0].SetPosition(0, blackPoints[0]);
                blackRenderers[0].SetPosition(1, blackPoints[1]);

                blackRenderers[1].SetPosition(0, blackPoints[1]);
                blackRenderers[1].SetPosition(1, blackPoints[2]);

                for (int i = 0; i < redRenderers.Count; i++)
                {
                    redRenderers[i].SetPosition(0, emptyValue);
                    redRenderers[i].SetPosition(1, emptyValue);
                }
            }
        }
        else
        {
            for (int i = 0; i < blackRenderers.Count; i++)
            {
                blackRenderers[i].SetPosition(0, emptyValue);
                blackRenderers[i].SetPosition(1, emptyValue);
            }

            for (int i = 0; i < redRenderers.Count; i++)
            {
                redRenderers[i].SetPosition(0, emptyValue);
                redRenderers[i].SetPosition(1, emptyValue);
            }
        }
    }

    private void HitCheck()
    {
        blackHit.Clear();
        redHit.Clear();

        Vector3 startDistance = startPoint - transform.position;

        if (startDistance.magnitude > 0.8f)
        {
            //если линий отскока от стенки нет
            if (redRenderers[2].GetPosition(0) == redRenderers[2].GetPosition(1))
            {
                if (redRenderers[3].GetPosition(0) == redRenderers[3].GetPosition(1))
                {
                    Vector2 finalHit = HitPlace(startDistance.magnitude, false);
                    redHit.Add(finalHit);
                }
            }
            else
            {
                redHit.Add(blackPoints[1]);
                Vector2 finalHit = HitPlace(startDistance.magnitude, true);
                redHit.Add(finalHit);
            } 
        }
        else
        {
            if (blackPoints[2] == blackPoints[1])
            {
                blackHit.Add(blackPoints[1]);
            }
            else
            {
                blackHit.Add(blackPoints[1]);
                blackHit.Add(blackPoints[2]);
            }
        }    
    }

    //считаем место удара между красных линий
    private Vector2 HitPlace(float magnitude, bool secondHit)
    {
        int layerBorder = 1 << 6;
        Vector2 finalHit;
        Vector2 finalRay;
        float maxAngle = magnitude * spreadAngle;
        float randomAngle = Random.Range(-maxAngle, maxAngle);

        if (secondHit == true)
        {
            finalRay = new Vector2(ray2.direction.x, ray2.direction.y);
            Vector2 diapasonRay = Quaternion.Euler(randomAngle, randomAngle, randomAngle) * finalRay;
            RaycastHit2D hit = Physics2D.Raycast(ray2.origin, diapasonRay, 20.0f, layerBorder);
            finalHit = hit.point;
        }
        else
        {
            finalRay = new Vector2(ray.direction.x, ray.direction.y);
            Vector2 diapasonRay = Quaternion.Euler(randomAngle, randomAngle, randomAngle) * finalRay;
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, diapasonRay, 20.0f, layerBorder);
            finalHit = hit.point;
        }
        return finalHit;
    }

    //невидимая линия для отрисовки сгиба красного луча
    private void SetEdgeCollider(LineRenderer lineRenderer)
    {
        List<Vector2> edges = new List<Vector2>();

        for (int point = 0; point < lineRenderer.positionCount; point++)
        {
            Vector3 lineRendererPoint = lineRenderer.GetPosition(point);
            edges.Add(new Vector2(lineRendererPoint.x, lineRendererPoint.y));
        }

        edgeCollider.SetPoints(edges);
        Vector2 test1 = new Vector2();
        Vector2 test2 = new Vector2();

        test1 = invisibleRenderer.transform.worldToLocalMatrix.MultiplyPoint(edgeCollider.points[0]);
        test2 = invisibleRenderer.transform.worldToLocalMatrix.MultiplyPoint(edgeCollider.points[1]);

        edges.Clear();
        edges.Add(new Vector2(test1.x, test1.y));
        edges.Add(new Vector2(test2.x, test2.y));

        edgeCollider.SetPoints(edges);
    }

    private void OnMouseDrag()
    {
        Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (position.y > stopDownY && position.x < stopRightX && position.x > stopLeftX)
        {
            if (position.y <= stopLine1)
            {
                LineDirection();
                LineRendering();
            }
            else if (position.y <= stopLine)
            {
                redRenderers[0].SetPosition(0, emptyValue);
                redRenderers[0].SetPosition(1, emptyValue);
            }
        }     
    }

    private void OnMouseUp()
    {
        Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        for (int i = 0; i < blackRenderers.Count; i++)
        {
            blackRenderers[i].SetPosition(0, emptyValue);
            blackRenderers[i].SetPosition(1, emptyValue);
        }

        for (int i = 0; i < redRenderers.Count; i++)
        {
            redRenderers[i].SetPosition(0, emptyValue);
            redRenderers[i].SetPosition(1, emptyValue);
        }

        Vector3 startDistance = startPoint - transform.position;
        variableSpeed = 5f + startDistance.magnitude;

        if (position.y <= stopLine)
        {
            if (blackHit.Count > 0)
            {
                ChangeCoord(startPoint, blackHit[0], variableSpeed);
                stateHit = blackHit[0];
            }
            else if (redHit.Count > 0)
            {
                ChangeCoord(startPoint, redHit[0], variableSpeed);
                stateHit = redHit[0];

                //сообщаем о том "сильный" ли это удар
                forseFly = true;
            }

            fly1 = true;
        } 
    }

    //для расчета перемещения от одной точки к другой со временем
    private void ChangeCoord(Vector3 first, Vector3 second, float newSpeed)
    {
        transformPositionOld = first;
        transformPositionNew = second;
        speed = newSpeed;
        startTime = Time.time;
        journeyLength = Vector3.Distance(transformPositionOld, transformPositionNew);
    }

    //если шарик прилепляется (черная линия)
    private void Binding(GameObject bubble)
    {
        if (isSpringJoint == false)
        {
            gameObject.tag = "Bubble";
            gameObject.layer = 6;

            Bubble newBubble = gameObject.GetComponent<Bubble>();
            Vector2 nearestPoint = bubble.GetComponent<Bubble>().NearestPoint(stateHit);
            float[] offsets = bubble.GetComponent<Bubble>().Offsets();

            newBubble.SetOptions(ballRenderer.color, nearestPoint);
            newBubble.AllAnchors(nearestPoint, offsets[0], offsets[1]);
            StartCoroutine(WaitTime(nearestPoint, offsets[0] * 1.3f));

            isSpringJoint = true;
        }
    }

    //добавим корутину для того, чтобы дать время сработать отпружиниванию
    IEnumerator WaitTime(Vector2 point, float offsetY)
    {
        yield return new WaitForSeconds(seconds);
        CheckNeighbours(point, offsetY);
    }

    //мяч превращается в такой же шарик как остальные и запускает проверку будут ли лопаться остальные шарики
    private void CheckNeighbours(Vector2 point, float offsetY)
    {
        Vector2 newPoint = new Vector2(point.x, point.y - offsetY);
        Bubble bubble = gameObject.GetComponent<Bubble>();
        List<GameObject> checkBubbles = new List<GameObject>();

        for (int i = 0; i < bubble.freeAnchors.Count; i++)
        {
            Vector2 freeVector = bubble.freeAnchors[i];
            Vector2 targetVector = new Vector2(point.x - freeVector.x, point.y - freeVector.y);
            Ray2D newRay = new Ray2D(newPoint, targetVector);
            Debug.DrawRay(newRay.origin, newRay.direction * 0.4f, Color.red);
            RaycastHit2D hit = Physics2D.Raycast(newRay.origin, newRay.direction, 0.4f);
            if (hit.collider != null && hit.collider.tag == "Bubble")
            {
                checkBubbles.Add(hit.collider.gameObject);
            }
        }

        for (int i = 0; i < checkBubbles.Count; i++)
        {
            bubble.BusyNeighbors(checkBubbles[i]);
            checkBubbles[i].GetComponent<Bubble>().BusyNeighbors(bubble.gameObject);

            Color checkColor = checkBubbles[i].GetComponent<Bubble>().GetColor();
            if (checkColor == ballRenderer.color)
            {
                bubble.Neighbours(checkBubbles[i]);
                checkBubbles[i].GetComponent<Bubble>().Neighbours(bubble.gameObject);
            }
        }

        bubble.AddToField();
        bubble.CheckForBurst();
    }

    //если шарик пробивает другой (красная линия)
    private void Replace(GameObject bubble)
    {
        if (isSpringJoint == false)
        {
            gameObject.tag = "Bubble";
            gameObject.layer = 6;

            Bubble newBubble = gameObject.GetComponent<Bubble>();
            Vector2 newAnchorsPoint = bubble.GetComponent<SpringJoint2D>().connectedAnchor;
            float[] offsets = bubble.GetComponent<Bubble>().Offsets();
            newBubble.SetOptions(ballRenderer.color, newAnchorsPoint);

            bubble.GetComponent<Bubble>().Delete();
            bubble.GetComponent<Bubble>().RemoveFromField();
            Destroy(bubble);

            newBubble.AllAnchors(newAnchorsPoint, offsets[0], offsets[1]);
            StartCoroutine(WaitTime(newAnchorsPoint, offsets[0] * 1.3f));

            isSpringJoint = true;
        }
    }

    private void Start()
    {
        isSpringJoint = false;
        ballRenderer = gameObject.GetComponent<SpriteRenderer>();

        for (int i = 0; i < 2; i++)
        {
            LineRenderer lineRenderer = Instantiate(blackLine);
            lineRenderer.transform.SetParent(transform, false);
            blackRenderers.Add(lineRenderer);
        }
        for (int i = 0; i < 4; i++)
        {
            LineRenderer lineRenderer = Instantiate(redLine);
            lineRenderer.transform.SetParent(transform, false);
            redRenderers.Add(lineRenderer);
        }

        invisibleRenderer = Instantiate(invisibleLine);
        invisibleRenderer.transform.SetParent(transform, false);

        edgeCollider = invisibleRenderer.GetComponent<EdgeCollider2D>();
        myLine = invisibleRenderer;
    }

    private void FixedUpdate()
    {
        SetEdgeCollider(myLine);

        //fly1 движение без отскока, fly2 движение после отскока, если он есть
        if (fly1 == true)
        {
            float distCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(transformPositionOld, transformPositionNew, fractionOfJourney);

            if (transform.position == transformPositionNew)
            {
                
                fly1 = false;

                if (blackHit.Count > 1)
                {
                    ChangeCoord(blackHit[0], blackHit[1], variableSpeed);
                    stateHit = blackHit[1];

                    fly2 = true;
                }
                else if (redHit.Count > 1)
                {
                    ChangeCoord(redHit[0], redHit[1], variableSpeed);
                    stateHit = redHit[1];

                    fly2 = true;
                }
                else
                {
                    newRigidbody= gameObject.AddComponent<Rigidbody2D>();
                    newRigidbody.angularDrag = 2f;
                    newRigidbody.drag = 2f;
                }
            }
        }
        if (fly2 == true)
        {
            float distCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(transformPositionOld, transformPositionNew, fractionOfJourney);
            if (transform.position == transformPositionNew)
            {
                fly2 = false;
                newRigidbody = gameObject.AddComponent<Rigidbody2D>();
                newRigidbody.angularDrag = 2f;
                newRigidbody.drag = 2f;
            }
        }
    }

    //проверяем столкновения с другими шариками, с землёй, создание следующего шарика
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Bubble")
        {
            if (forseFly == false)
            {
                Binding(collision.gameObject);
            }
            else
            {
                Replace(collision.gameObject);
            }
        }
        else if (collision.gameObject.tag == "Ground")
        {
            Destroy(gameObject);
        }

        if (collision.gameObject.tag == "Bubble" || collision.gameObject.tag == "Ground")
        {
            if (isSpawner == false)
            {
                isSpawner = true;
                GameObject spawn = GameObject.Find("Spawner");
                spawn.GetComponent<BallSpawner>().NewCreate();
            }
        }
    }

}
