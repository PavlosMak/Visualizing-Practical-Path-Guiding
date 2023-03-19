using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BinaryNode {

    private float min;
    private float max;
    public bool isLeaf;
    
    private BinaryNode rightChild;
    private BinaryNode leftChild;
    private BinaryNode treeRoot;

    private int records;
    private Color radiance;

    private AdaptiveSDNode owner;

    private static Arc arc;

    public BinaryNode(float min, float max, int maxDepth, int currentDepth, BinaryNode treeRoot, AdaptiveSDNode owner) {

        if (currentDepth == 0) {
            this.treeRoot = this;
        }
        else {
            this.treeRoot = treeRoot;
        }

        int newDepth = currentDepth + 1;
        this.min = min;
        this.max = max;
        this.owner = owner; 
        
        if (maxDepth <= currentDepth) {
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

    public BinaryNode GetRightChild() {
        return rightChild;
    }

    public BinaryNode GetLeftChild() {
        return leftChild;
    }

    public void SetOwner(AdaptiveSDNode newOwner) {
        this.owner = newOwner;
    }

    public AdaptiveSDNode GetOwner() {
        return this.owner;
    }

    private void Split(int maxDepth, int newDepth) {
        float split = (min + max) / 2.0f;
        rightChild = new BinaryNode(min, split, maxDepth, newDepth, this.treeRoot, owner);
        leftChild = new BinaryNode(split, max, maxDepth, newDepth, this.treeRoot, owner);
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

        if(angle < min || angle > max) {
            throw new Exception("Angle out of bounds: " + angle);
        }

        if (this == treeRoot && radiance != Color.black) {
            this.records += 1;
        }
        
        if(this.isLeaf && radiance != Color.black) {
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

        var totalFlux = this.treeRoot.records;
        var ourFlux = this.records;
        
        return (float) ourFlux / totalFlux > 0.02;
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

    public BinaryNode Query(float angle) {

        var inBounds = min <= angle & angle <= max;

        if (!inBounds) {
            throw new Exception("angle not in node bounds!");
        }
        
        if (isLeaf) {
            return this;
        } 
        
        float split = this.GetCenter();
        if (angle <= split) {
            return rightChild.Query(angle);
        } else {
            return leftChild.Query(angle);
        }        
    }

    public float GetMin() {
        return min;
    }

    public float GetMax() {
        return max;
    }

    public void Draw() {
        arc.ColorSegment(min, max, Random.ColorHSV());
    }

    public Bounds GetBounds() {
        // min and max of (2D) spatial bounding box
        var min2D = this.owner.area.min;
        var max2D = this.owner.area.max;
    
        // z coords are min and max angle
        var bounds = new Bounds();
        bounds.min = new Vector3(min2D.x, min2D.y, this.min);
        bounds.max = new Vector3(max2D.x, max2D.y, this.max);
        return bounds;
    }
}
