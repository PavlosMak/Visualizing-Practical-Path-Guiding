using System;
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
        
        if (maxDepth <= currentDepth) {
            rightChild = null;
            leftChild = null;
            isLeaf = true;
            angleTree = new BinaryNode(0,360,3,0, null, this);
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

    public BinaryNode Query(Vector2 point, float angle) {

        var spatialNode = Query(point);
    
        var binTree =  spatialNode.angleTree.Query(angle);
        return binTree;
    }

    
    // public void DrawAllLeaves() {
    //     if(isLeaf) {
    //         DrawRect();
    //     } else {
    //         this.rightChild.DrawAllLeaves();
    //         this.leftChild.DrawAllLeaves();
    //     }
    // }

    public void RecordVertex(Vector2 point, float theta, Color radiance) {
    
        // only record a point if it is in the spatial region covered by the tree
        if (!area.Contains(point)) {
            throw new Exception("Point not in tree");
        }
        
        if (this.isLeaf) {
            Debug.Log("Recorded vertex in " + area);
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
                this.Subdivide();
            }
            
        } else {
            this.rightChild.Adapt(k);
            this.leftChild.Adapt(k);
        }
    }

    // public void DrawRect() {
    //     Debug.DrawRay(area.min,new Vector2(area.max.x, area.min.y) - area.min, Color.green); //Bottom
    //     Debug.DrawRay(area.min, new Vector2(area.min.x, area.max.y) - area.min, Color.green); //Left
    //     var bottomRight = new Vector2(area.max.x,area.min.y);
    //     var topLeft = new Vector2(area.min.x,area.max.y);
    //     Debug.DrawRay(bottomRight, area.max - bottomRight, Color.green); //Right
    //     Debug.DrawRay(topLeft,area.max-topLeft, Color.green); //Top
    // } 
    
}

public class AdaptiveSDTree : MonoBehaviour {
   
    // params
    [SerializeField] private Rect area;
    [SerializeField] private int maxDepth = 6;
    [SerializeField] private GameObject box3Prefab;
    
    // root of actual tree
    private AdaptiveSDNode root;
    
    // debugging
    [SerializeField] private GameObject point;
    [SerializeField] private bool showArea = false;
    private bool performQuery = false;
    private bool showAllLeaves = false;

    //Added here for demonstration purposes
    private AdaptiveSDNode lastFound;
    
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
     root = new AdaptiveSDNode(area, maxDepth,0,0);   
    }

    public void RecordRadiance(Vector2 pos, float theta, Color radiance) {
        root.RecordVertex(pos, theta, radiance);  
    }

    private void Update() {
    
        Vector3 mousePos =  Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        if (Input.GetKeyDown(KeyCode.B)) {
            var binTreeNode = root.Query(mousePos, 0);

            var bounds = binTreeNode.GetBounds();
            var boxGo = Instantiate(box3Prefab, bounds.center, Quaternion.identity);
    
            var scaleX = Mathf.Abs(bounds.max.x - bounds.min.x);
            var scaleY = Mathf.Abs(bounds.max.y - bounds.min.y);
            var scaleZ = Mathf.Abs(bounds.max.z - bounds.min.z);

            boxGo.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);

        }
    }

    // void Update() {
    //     if(Input.GetButtonDown("Fire2")) {
    //         Vector3 mousePos =  Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //         point.transform.position = new Vector3(mousePos.x,mousePos.y,0);
    //     }
    //     if(Input.GetKeyDown("space")) {
    //         performQuery = !performQuery;
    //     }
    //     if (Input.GetKeyDown(KeyCode.L)) {
    //         showAllLeaves = !showAllLeaves;
    //     }
    //     if (Input.GetKeyDown(KeyCode.Delete)) {
    //         lastFound.Subdivide();
    //     }
    //     if (Input.GetKeyDown(KeyCode.B)) {
    //         Debug.Assert(root != null);
    //         Debug.Assert(root.isLeaf == false, "Root should not be a leaf");
    //         // root.Adapt();
    //     }
    //     if(performQuery) {
    //         Vector2 pointCoord = new Vector2(point.transform.position.x, point.transform.position.y);
    //         lastFound = root.Query(pointCoord);
    //         // root.RecordVertex(pointCoord, Color.black);
    //         lastFound.DrawRect();
    //     } 
    //     if(showAllLeaves) {
    //         root.DrawAllLeaves();
    //     }
    //     if(showArea) {
    //         root.DrawRect();
    //     }
    // }
}
