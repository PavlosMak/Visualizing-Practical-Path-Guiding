using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public struct QueryResult {
    public AdaptiveSDNode spaceNode {get;}
    public BinaryNode angleNode {get;}

    public QueryResult(AdaptiveSDNode spaceNode, BinaryNode angleNode) {
        this.spaceNode = spaceNode;
        this.angleNode = angleNode;
    }
}

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
        
        if (maxDepth <= currentDepth) {
            rightChild = null;
            leftChild = null;
            isLeaf = true;
            angleTree = new BinaryNode(0,360,2,0, null);
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
        //Adjust the angle trees in the child space nodes
        this.rightChild.angleTree = this.angleTree;
        this.rightChild.angleTree.Adapt();

        this.leftChild.angleTree = this.angleTree;
        this.leftChild.angleTree.Adapt();

        this.angleTree = null;
    }

    public AdaptiveSDNode Query(Vector2 point) {
        if(isLeaf && area.Contains(point)) {
            Debug.Log("Found point in " + area);
            return this;
        } else if(isLeaf) {
            throw new Exception("Point out of bounds");
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

    public QueryResult Query(Vector2 point, float angle) {
        var spatialNode = Query(point);
        var binTree = spatialNode.angleTree.Query(angle);
        return new QueryResult(spatialNode, binTree);
    }

    
    public void DrawAllLeaves() {
        if(isLeaf) {
            DrawRect();
        } else {
            this.rightChild.DrawAllLeaves();
            this.leftChild.DrawAllLeaves();
        }
    }

    public void DrawAllSpaceLeaves() {
        if (isLeaf) {
            DrawRect();
        } else {
            this.rightChild.DrawAllSpaceLeaves();
            this.leftChild.DrawAllSpaceLeaves();
        }
    }

    public void RecordVertex(Vector2 point, float theta, Color radiance) {
    
        // only record a point if it is in the spatial region covered by the tree
        if (!area.Contains(point)) {
            throw new Exception("Point not in tree: " + point);
        }
        
        if (this.isLeaf) {
            // Debug.Log("Recorded vertex in " + area);
            this.recordedVertices += 1;
            this.radiance += radiance; 
            
            this.angleTree.AddRecord(theta, radiance);
            
            return;
        }
        
        // check on which side of the node the point is in
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
            rightChild.RecordVertex(point, theta, radiance);
        } else {
            leftChild.RecordVertex(point, theta, radiance);
        }        
    }

    private bool ShouldSplit(int k) {
        float c = 1;
        return recordedVertices >= c * Mathf.Sqrt(2^k);
    }

    public void Adapt(int k) {
        
        if (this.isLeaf) {
            // check if we should subdivide
            if (ShouldSplit(k)) {
                Debug.Log("Subdividing");
                this.Subdivide();
            }
        } else {
            this.rightChild.Adapt(k);
            this.leftChild.Adapt(k);
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

    public List<QueryResult> GetAllLeaves() {
        var results = new List<QueryResult>();
        var spaceNodes = new Stack<AdaptiveSDNode>();
        
        spaceNodes.Push(this);
        while (spaceNodes.Count > 0) {
            AdaptiveSDNode finger = spaceNodes.Pop();
            if (!finger.isLeaf) {
                spaceNodes.Push(finger.rightChild);
                spaceNodes.Push(finger.leftChild);
                continue;
            } 
            
            // for each space leaf, traverse its angle bintree
            var angleNodes = new Stack<BinaryNode>();
            angleNodes.Push(finger.angleTree);
            while (angleNodes.Count > 0) {
                BinaryNode angleFinger = angleNodes.Pop();
                if (angleFinger.isLeaf) {
                    results.Add(new QueryResult(finger, angleFinger));
                } else {
                    angleNodes.Push(angleFinger.GetRightChild());
                    angleNodes.Push(angleFinger.GetLeftChild());
                }
            }
        }
        
        return results;
    }

}

public class AdaptiveSDTree : MonoBehaviour {
   
    // params
    [SerializeField] private Rect area;
    [SerializeField] private int initialSpatialSubdivision = 6;
    [SerializeField] private GameObject box3Prefab;
    
    // root of actual tree
    private AdaptiveSDNode root;
    
    // buttons
    [SerializeField] private bool showAllLeaves;
    [SerializeField] private bool drawSpatialLeaves;
    
    // keep track of drawn leaves
    private List<GameObject> lastDrawnLeaves = null;

    // Singleton instance reference
    public static AdaptiveSDTree Instance { get; private set; }
    
    private void Awake() { 
        // setup instance reference
        if (Instance != null && Instance != this) { 
            Destroy(this); 
        } 
        else { 
            Instance = this; 
        } 
    }

    private void Start() {
        root = new AdaptiveSDNode(area, initialSpatialSubdivision,0,0);   
    }

    public void RecordRadiance(Vector2 pos, float theta, Color radiance) {
        root.RecordVertex(pos, theta, radiance);  
    }

    public void Adapt(int iteration) {
        root.Adapt(iteration);
    }
    
    private void Update() {
    
        Vector3 mousePos =  Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        if (showAllLeaves) { 
            DrawLeaves();
            showAllLeaves = !showAllLeaves;
        }
        
        if (Input.GetKeyDown(KeyCode.J)) {
            ClearPreviousLeafs();
        }
        
        if (drawSpatialLeaves) {
            root.DrawAllSpaceLeaves();
        }
    }

    void ClearPreviousLeafs() {
        if(lastDrawnLeaves != null ) {
            foreach(var leaf in lastDrawnLeaves) {
                Destroy(leaf);
            }
        } 
        lastDrawnLeaves = new List<GameObject>();
    }

    void DrawLeaves() {
        
        ClearPreviousLeafs();
        
        var leaves = root.GetAllLeaves();
        foreach (var queryRes in leaves) {
            var leaf = DrawQueryResult(queryRes);
            lastDrawnLeaves.Add(leaf);  
        }
    }
    
    GameObject DrawQueryResult(QueryResult queryRes) {
        
        if (lastDrawnLeaves == null) {
            lastDrawnLeaves = new List<GameObject>();
        }
        
        // queryRes.spaceNode.DrawRect();
        var spaceLeaf = queryRes.spaceNode;
        var spaceLeafRect = spaceLeaf.area;
        
        // min and max of (2D) spatial bounding box
        var min2D = spaceLeafRect.min;
        var max2D = spaceLeafRect.max;

        // scaled min/max angles
        var minAng = 5.0f * queryRes.angleNode.GetMin() / 360f;
        var maxAng = 5.0f * queryRes.angleNode.GetMax() / 360f;

        var bounds = new Bounds();
        bounds.SetMinMax(
            new Vector3(min2D.x, min2D.y, minAng),
            new Vector3(max2D.x, max2D.y, maxAng));
    
        // // z coords are min and max angle
        // var bounds = new Bounds();
        // var newMin = new Vector3(min2D.x, min2D.y, queryRes.angleNode.GetMin());
        // var newMax = new Vector3(max2D.x, max2D.y, queryRes.angleNode.GetMax());
        //
        // bounds.SetMinMax(newMin, newMax);
        //
        // // scale bounds
        // bounds.center = new Vector3(bounds.center.x, bounds.center.y, 5.0f * (bounds.center.z / 360.0f)); 
    
        // draw the leaf
        var leafGo = Instantiate(box3Prefab, bounds.center, Quaternion.identity);
        var scaleX = Mathf.Abs(bounds.max.x - bounds.min.x);
        var scaleY = Mathf.Abs(bounds.max.y - bounds.min.y);
        var scaleZ = Mathf.Abs(bounds.max.z - bounds.min.z);
        leafGo.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);

        leafGo.name += spaceLeafRect;

        var orange = bounds.min;
        var blue = new Vector3(scaleX, 0, 0);
        var red = new Vector3(0, scaleY, 0);
        var green = new Vector3(0, 0, scaleZ);

        Vector3[] lrPositions = {
            orange,
            orange + blue,
            orange + blue + red,
            orange + red,
            orange,
            orange + green,
            orange + green + blue,
            orange + blue,
            orange + green + blue,
            orange + green + blue + red,
            orange + blue + red,
            orange + green + blue + red,
            orange + red + green,
            orange + green,
            orange + red + green,
            orange + red
        };

        var lr = leafGo.GetComponent<LineRenderer>();
        lr.positionCount = lrPositions.Length;
        lr.SetPositions(lrPositions);

        return leafGo;
    } 
}
