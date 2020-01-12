using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class EllipsePath : MonoBehaviour
{
    public GameObject goTemp;
    public GameObject focusGo;         //焦点物体
    public float centerX = 0;         //中心X
    public float centerY = 0;         //中心Y
    public float xLength = 500;         //长轴
    public float yLength = 200;         //短轴
    public float speed = -0.2f;         //速度
    public int count = 10;              //展示数量
    public UnityAction finishedCallback;

    
    private GameObject[] gos;
    private float[] angles;
    private bool pause = true;
    private bool speedUp = false;
    private float targetAngle = 0f;
    private float curAngle = 0f;

    private float time = 0f;
    private float addAngle = 0f;
    private int itemIndex = -1;
    private int realIndex = -1;
    private int[] indexList;
    private int maxCount = 0;
    private bool stopAdd = false;
    public AnimationCurve animCurve;

    public delegate void VoidDelegate(int index, int realIndex, GameObject obj);
    public VoidDelegate onUpdateItem;

    // Use this for initialization
    void Start()
    {
        angles = new float[count];
        gos = new GameObject[count];
        indexList = new int[count];
        setMaxCount(45);
    }

    public void setMaxCount(int _count)
    {
        maxCount = _count;
        initEllipse();
    }

    public void refreshAll()
    {
        updateItems();
    }

    void initEllipse()
    {
       

        float angleOff = -360.0f / count;
        for (int i = 0; i < count; i++)
        {
            gos[i] = (GameObject)GameObject.Instantiate(goTemp); ;
            gos[i].transform.SetParent(gameObject.transform);
            gos[i].SetActive(true);
            this.angles[i] = -630f - angleOff * i;
            indexList[i] = i;
        }
        addAngle = angleOff;
        pause = false;
        updateGos();
        updateItems();
    }

    void updateGos()
    {
        List<int> list = new List<int>();
        list.Add((int)yLength * 100 + count);
        for (int i = 0; i < count; i++)
        {
            angles[i] = angles[i] + speed;
            float hudu = (angles[i] / 180) * Mathf.PI;
            float xx = centerX + xLength * Mathf.Cos(hudu);
            float yy = centerY + yLength * Mathf.Sin(hudu);
            float scale = Mathf.Abs(yy / yLength - 1) * 0.2f + 0.4f;

            gos[i].transform.localPosition = new Vector3(xx, yy);
            gos[i].transform.localScale = new Vector3(scale, scale);
            gos[i].GetComponent<CanvasGroup>().alpha = Mathf.Abs(yy / yLength - 1) * 0.5f;

            int floor = Mathf.CeilToInt(yLength - yy);
            list.Add(floor * 100 + i);
        }

        list.Sort();
        int sortIndex = 0;
        foreach (int floor in list)
        {
            int index = floor % 100;

            if (index < count)
            {
                gos[index].transform.SetSiblingIndex(sortIndex);
            }
            else
            {
                focusGo.transform.SetSiblingIndex(sortIndex);
            }
            sortIndex++;
        }

        if (Mathf.Ceil(-addAngle) >= 360.0f / count)
        {
            addAngle = addAngle + 360.0f / count;
            itemIndex++;
            realIndex++;
            indexList[itemIndex % count] = (count + realIndex) % maxCount;
            updateItems(itemIndex % count);
        }
    }

    void updateGosBy(float val, bool reset = false)
    {
        List<int> list = new List<int>();
        list.Add((int)yLength * 100 + count);
        for (int i = 0; i < count; i++)
        {
            float angle = angles[i] + val;
            float hudu = (angle / 180) * Mathf.PI;
            float xx = centerX + xLength * Mathf.Cos(hudu);
            float yy = centerY + yLength * Mathf.Sin(hudu);
            float scale = Mathf.Abs(yy / yLength - 1) * 0.2f + 0.4f;

            gos[i].transform.localPosition = new Vector3(xx, yy);
            gos[i].transform.localScale = new Vector3(scale, scale);
            gos[i].GetComponent<CanvasGroup>().alpha = Mathf.Abs(yy / yLength - 1) * 0.5f;

            int floor = Mathf.CeilToInt(yLength - yy);
            list.Add(floor * 100 + i);
            if (reset)
            {
                angles[i] = angle;
            }
        }

        list.Sort();

        int sortIndex = 0;
        foreach (int floor in list)
        {
            int index = floor % 100;

            if (index < count)
            {
                gos[index].transform.SetSiblingIndex(sortIndex);
            }
            else
            {
                focusGo.transform.SetSiblingIndex(sortIndex);
            }

            sortIndex++;
        }

        if (Mathf.Ceil(-addAngle) >= 360.0f / count)
        {
            Debug.Log(addAngle);
            addAngle = addAngle + 360.0f / count;
            itemIndex++;
            realIndex++;
            indexList[itemIndex % count] = (count + realIndex) % maxCount;
            updateItems(itemIndex % count);
        }
    }

    void updateItems(int index = -1)
    {
        if (index >= 0)
        {
            if (onUpdateItem != null)
            {
                onUpdateItem(index, indexList[index], gos[index]);
            }
            gos[index].transform.Find("Text").GetComponent<Text>().text = indexList[index].ToString();
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                if (onUpdateItem != null)
                {
                    onUpdateItem(i, indexList[i], gos[i]);
                }
                gos[i].transform.Find("Text").GetComponent<Text>().text = indexList[i].ToString();
            }
        }
    }

    public void setTarget(int index)
    {
        speedUp = true;
        pause = false;

        int curIndex = (count + itemIndex) % maxCount;
        float angle = angles[itemIndex % count] % -360f;

        targetAngle = -angle - 270f - 360f * 2;
        realIndex = (index - 26 + maxCount) % maxCount;
        curAngle = 0;
        time = 0;
    }

    public void reStart()
    {
        pause = false;
        stopAdd = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (pause || maxCount <= 0) { return; }
        if (speedUp)
        {
            time += Time.deltaTime;
            float val = targetAngle * animCurve.Evaluate(time * 0.5f);
            addAngle -= curAngle - val;
            if (val <= targetAngle)
            {
                speedUp = false;
                pause = true;
                updateGosBy(targetAngle, true);
                if (finishedCallback != null)
                {
                    finishedCallback();
                }
            }
            else
            {
                updateGosBy(val);
            }
            curAngle = val;
        }
        else
        {
            addAngle += speed;
            this.updateGos();
        }
    }
}
