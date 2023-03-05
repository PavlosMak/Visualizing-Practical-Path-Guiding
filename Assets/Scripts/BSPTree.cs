using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BSPNode {
    public Rect area;
    public bool isLeaf = false;
    private int axis;  
    private BSPNode rightChild; //right means lower than mid of area
    private BSPNode leftChild;

    public BSPNode(Rect area, int maxDepth, int currentDepth, int currentAxis) {
        this.area = area;
        this.axis = currentAxis;
        int newAxis = (currentAxis + 1) % 2;
        int newDepth = currentDepth + 1;
        if(maxDepth <= currentDepth) {
            rightChild = null;
            leftChild = null;
            isLeaf = true;
        } else {
            Rect rightArea = new Rect();
            Rect leftArea = new Rect();
            if (currentAxis == 0) {
                float split = area.center.x;
                rightArea =  new Rect(area.xMin,area.yMin,area.width/2,area.height);
                leftArea = new Rect(area.xMin + area.width / 2.0f, area.yMin, area.width/2.0f, area.height);
            } else {
                float split = area.center.y;
                rightArea = new Rect(area.xMin,area.yMin,area.width,area.height/2.0f);
                leftArea = new Rect(area.xMin,area.yMin+area.height/2.0f,area.width,area.height/2.0f);
            }
            rightChild = new BSPNode(rightArea, maxDepth, newDepth, newAxis);
            leftChild = new BSPNode(leftArea, maxDepth, newDepth, newAxis);
        }
    }

    public bool Query(Vector2 point) {
        if(isLeaf && area.Contains(point)) {
            Debug.Log("Found point in " + area);
            DrawRect();
            return true;
        } else if(isLeaf) {
            return false;
        }
        float split = 0.0f;
        float coordinate = 0.0f;
        if(axis == 0) {
            //X case
            split = area.center.x;
            coordinate = point.x;
        } else {
            //Y case
            split = area.center.y;
            coordinate = point.y;
        }

        if (coordinate <= split) {
            return rightChild.Query(point);
        } else {
            return leftChild.Query(point);
        }        
    }

    public void DrawRect() {
        Debug.DrawRay(area.min,new Vector2(area.max.x, area.min.y) - area.min, Color.green); //Bottom
        Debug.DrawRay(area.min, new Vector2(area.min.x, area.max.y) - area.min, Color.green); //Left
        var bottomRight = new Vector2(area.max.x,area.min.y);
        var topLeft = new Vector2(area.min.x,area.max.y);
        Debug.DrawRay(bottomRight, area.max - bottomRight, Color.green); //Right
        Debug.DrawRay(topLeft,area.max-topLeft, Color.green); //Top
    } 
}

[ExecuteAlways]
public class BSPTree : MonoBehaviour
{

    [SerializeField] private Rect area;
    [SerializeField] private int maxDepth = 6;
    [SerializeField] private bool performQuery = false;
    [SerializeField] private bool showArea = false;
    [SerializeField] private GameObject point;

    private BSPNode root;

    // Start is called before the first frame update
    void Start()
    {
     root = new BSPNode(area,maxDepth,0,0);   
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Fire2")) {
            Vector3 mousePos =  Camera.main.ScreenToWorldPoint(Input.mousePosition);
            point.transform.position = new Vector3(mousePos.x,mousePos.y,0);
        }
        if(performQuery) {
            root.Query(new Vector2(point.transform.position.x, point.transform.position.y));
        } 
        if(showArea) {
            root.DrawRect();
        }
    }
}
