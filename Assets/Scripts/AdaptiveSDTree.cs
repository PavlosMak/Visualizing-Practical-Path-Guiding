using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AdaptiveSDNode {
    public Rect area;
    public bool isLeaf = false;
    private int axis;  
    private AdaptiveSDNode rightChild; //right means lower than mid of area
    private AdaptiveSDNode leftChild;
    private BinaryNode angleTree;
    private Color radiance;
    private int recordedVertices;

    public AdaptiveSDNode(Rect area, int maxDepth, int currentDepth, int currentAxis) {
        this.area = area;
        this.axis = currentAxis;
        int newAxis = (currentAxis + 1) % 2;
        int newDepth = currentDepth + 1;
        if(maxDepth <= currentDepth) {
            rightChild = null;
            leftChild = null;
            isLeaf = true;
            angleTree = new BinaryNode(-90,90,3,0);
            radiance = Color.black;
            recordedVertices = 0; //For now we initialize with zero vertices
        } else {
            Split(currentAxis,maxDepth,newDepth);
        }
    }


    private void Split(int currentAxis, int maxDepth, int newDepth) {
        int newAxis = (currentAxis + 1) % 2;
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
        rightChild = new AdaptiveSDNode(rightArea, maxDepth, newDepth, newAxis);
        leftChild = new AdaptiveSDNode(leftArea, maxDepth, newDepth, newAxis);
    }

    // What happens to the recorded vertices when we split? 
    public void Subdivide() {
        this.isLeaf = false;
        Split(this.axis, 0, 0); //Split just to two
    }

    public AdaptiveSDNode Query(Vector2 point) {
        if(isLeaf && area.Contains(point)) {
            Debug.Log("Found point in " + area);
            return this;
        } else if(isLeaf) {
            throw new System.Exception("Point out of bounds");
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

    public void DrawAllLeaves() {
        if(isLeaf) {
            DrawRect();
        } else {
            this.rightChild.DrawAllLeaves();
            this.leftChild.DrawAllLeaves();
        }
    }

    public void recordVertex(Vector2 point, Color radiance) {
        if(this.isLeaf && area.Contains(point)) {
            Debug.Log("Recorded vertex in " + area);
            this.recordedVertices += 1;
            this.radiance += radiance; 
            return;
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
            rightChild.recordVertex(point, radiance);
        } else {
            leftChild.recordVertex(point, radiance);
        }        
    }

    public void Adapt() {
        if(this.isLeaf) {
            if(recordedVertices >= 3) { 
                this.Subdivide();
            }
        } else {
            Debug.Assert(rightChild != null, "Right child was null");
            this.rightChild.Adapt();
            Debug.Assert(leftChild != null, "Left child was null");
            this.leftChild.Adapt();
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

public class AdaptiveSDTree : MonoBehaviour
{
    [SerializeField] private Rect area;
    [SerializeField] private int maxDepth = 6;
    [SerializeField] private bool showArea = false;
    [SerializeField] private GameObject point;

    private AdaptiveSDNode root;
    private bool performQuery = false;
    private bool showAllLeaves = false;
    private bool adaptTree = false;

    //Added here for demonstration purposes
    private AdaptiveSDNode lastFound;

    // Start is called before the first frame update
    void Start()
    {
     root = new AdaptiveSDNode(area,maxDepth,0,0);   
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Fire2")) {
            Vector3 mousePos =  Camera.main.ScreenToWorldPoint(Input.mousePosition);
            point.transform.position = new Vector3(mousePos.x,mousePos.y,0);
        }
        if(Input.GetKeyDown("space")) {
            performQuery = !performQuery;
        }
        if (Input.GetKeyDown(KeyCode.L)) {
            showAllLeaves = !showAllLeaves;
        }
        if (Input.GetKeyDown(KeyCode.Delete)) {
            lastFound.Subdivide();
        }
        if (Input.GetKeyDown(KeyCode.B)) {
            Debug.Assert(root != null);
            Debug.Assert(root.isLeaf == false, "Root should not be a leaf");
            root.Adapt();
        }
        if(performQuery) {
            Vector2 pointCoord = new Vector2(point.transform.position.x, point.transform.position.y);
            lastFound = root.Query(pointCoord);
            root.recordVertex(pointCoord, Color.black);
            lastFound.DrawRect();
        } 
        if(showAllLeaves) {
            root.DrawAllLeaves();
        }
        if(showArea) {
            root.DrawRect();
        }
    }
}
