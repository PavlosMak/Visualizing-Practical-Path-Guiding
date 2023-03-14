using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BinaryNode {

    private float min;
    private float max;
    public bool isLeaf = false;
    private BinaryNode rightChild;
    private BinaryNode leftChild;

    private int records;
    private Color radiance;

    private static Arc arc;


    public BinaryNode(float min, float max, int maxDepth, int currentDepth) {
        int newDepth = currentDepth + 1;
        this.min = min;
        this.max = max;
        if(maxDepth <= currentDepth) {
            rightChild = null;
            leftChild = null;
            isLeaf = true;
            records = 0;
            radiance = Color.black;
        } else {
            Split(maxDepth, newDepth);
        }
    }

    public void SetArc(Arc arc) {
        BinaryNode.arc = arc;
    }

    private void Split(int maxDepth, int newDepth) {
        float split = (min + max) / 2.0f;
        rightChild = new BinaryNode(min, split, maxDepth, newDepth);
        leftChild = new BinaryNode(split, max, maxDepth, newDepth);
    }

    public void Subdivide() {
        this.isLeaf = false;
        Split(0,0); // Split just to two
    }

    public float GetCenter() {
        return (min + max) / 2.0f;
    }

    public float GetSize() {
        return Mathf.Abs(max - min);
    }

    public void DrawAllLeaves() {
        if(isLeaf) {
            Debug.Log("Drew: " + min + "-" + max);
            Draw();   
        } else {
            this.rightChild.DrawAllLeaves();
            this.leftChild.DrawAllLeaves();
        }
    }

    public void AddRecord(float angle, Color radiance) {
        if(this.isLeaf && min <= angle & angle <= max) {
            Debug.Log("Added record in: " + min + "-" + max);
            this.records += 1;
            this.radiance += radiance;
            return;
        }
        float center = this.GetCenter();
        if (angle <= center) {
            rightChild.AddRecord(angle,radiance);
        } else {
            leftChild.AddRecord(angle, radiance);
        }
    }

    private bool ShouldSplit() {
        return (min < -50);
    }    

    public void Adapt() {
        if(this.isLeaf) {
            if(ShouldSplit()) {
                this.Subdivide();
            }
        } else {
            this.rightChild.Adapt();
            this.leftChild.Adapt();
        }
    }

    public bool Query(float point) {
        if(isLeaf && min <= point & point <= max) {
            // Debug.Log("Found point in " + min + "-" + max);
            return true;
        } else if(isLeaf) {
            // Debug.Log("Did not find point!");
            return false;
        }
        float split = this.GetCenter();
        if (point <= split) {
            return rightChild.Query(point);
        } else {
            return leftChild.Query(point);
        }        
    }

    public void Draw() {
        arc.ColorSegment(min, max, Random.ColorHSV());
    }    
}

public class BinaryTree : MonoBehaviour
{

    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private int maxDepth = 4;
    [SerializeField] private LineRenderer lineRenderer;

    private BinaryNode root;
    private Arc arc;    
    private bool showLeaves = false;

    // Start is called before the first frame update
    void Start()
    {
        root = new BinaryNode(minX,maxX,maxDepth,0);
        arc = gameObject.GetComponent<Arc>();
        root.SetArc(arc);
        arc.DrawArc(minX, maxX, .05f, lineRenderer, 100);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Z)) {
            showLeaves = !showLeaves;
        } 
        if(showLeaves) {
            arc.ClearDrawnSegments();
            root.DrawAllLeaves();
            showLeaves = false;
        }
        if(Input.GetKeyDown(KeyCode.H)) {
            root.Adapt();
        }
    }
}
