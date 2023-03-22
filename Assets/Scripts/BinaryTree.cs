using System;
using UnityEngine;

public class BinaryNode {
    private float min;
    private float max;
    public bool isLeaf;

    private BinaryNode rightChild;
    private BinaryNode leftChild;
    private BinaryNode treeRoot;

    private int records;
    public Color radiance;

    public BinaryNode(float min, float max, int maxDepth, int currentDepth, BinaryNode treeRoot) {
        if (currentDepth == 0) {
            this.treeRoot = this;
        }
        else {
            this.treeRoot = treeRoot;
        }

        int newDepth = currentDepth + 1;
        this.min = min;
        this.max = max;

        if (maxDepth <= currentDepth) {
            rightChild = null;
            leftChild = null;
            isLeaf = true;
            records = 0;
            radiance = Color.black;
        }
        else {
            Split(maxDepth, newDepth);
        }
    }

    // alternative constructor to create a leaf
    public BinaryNode(float min, float max, int records, Color radiance, BinaryNode treeRoot) {
        this.min = min;
        this.max = max;
        this.isLeaf = true;
        this.records = records;
        this.radiance = radiance;
        this.treeRoot = treeRoot;
    }

    public BinaryNode GetRightChild() {
        return rightChild;
    }

    public BinaryNode GetLeftChild() {
        return leftChild;
    }

    private void Split(int maxDepth, int newDepth) {
        float split = (min + max) / 2.0f;
        rightChild = new BinaryNode(min, split, maxDepth, newDepth, this.treeRoot);
        leftChild = new BinaryNode(split, max, maxDepth, newDepth, this.treeRoot);
    }

    public void Subdivide() {
        this.isLeaf = false;
        Split(0, 0); // Split just to two
    }

    public float GetCenter() {
        return (min + max) / 2.0f;
    }

    public float GetSize() {
        return Mathf.Abs(max - min);
    }

    public void AddRecord(float angle, Color radiance) {
        angle = angle % 360;

        if (angle < min || angle > max) {
            throw new Exception("Angle out of bounds: " + angle);
        }

        if (this == treeRoot && radiance != Color.black) {
            this.records += 1;
        }

        if (this.isLeaf && radiance != Color.black) {
            Debug.Log("Added record in: " + min + "-" + max);
            this.records += 1;
            this.radiance += radiance;
            return;
        }

        float center = this.GetCenter();
        if (angle <= center) {
            rightChild.AddRecord(angle, radiance);
        }
        else {
            leftChild.AddRecord(angle, radiance);
        }
    }

    private bool ShouldSplit() {
        var totalFlux = this.treeRoot.records;
        var ourFlux = this.records;

        return (float)ourFlux / totalFlux > 0.02;
    }

    public void Adapt() {
        if (this.isLeaf) {
            if (ShouldSplit()) {
                this.Subdivide();
            }
        }
        else {
            this.rightChild.Adapt();
            this.leftChild.Adapt();
        }
    }

    public BinaryNode Query(float angle) {
        var inBounds = min <= angle && angle <= max;

        if (!inBounds) {
            throw new Exception("angle not in node bounds!");
        }

        if (isLeaf) {
            return this;
        }

        float split = this.GetCenter();
        if (angle <= split) {
            return rightChild.Query(angle);
        }
        else {
            return leftChild.Query(angle);
        }
    }

    public Color GetColor() {
        if (records == 0) {
            return Color.black;
        }

        return PtUtils.multScalarColor(1f / records, radiance);
    }

    public float GetMin() {
        return min;
    }

    public float GetMax() {
        return max;
    }

    public BinaryNode DeepCopy() {
        if (this.isLeaf) {
            return new BinaryNode(this.min, this.max, records, radiance, treeRoot);
        }

        var node = new BinaryNode(this.min, this.max, records, radiance, treeRoot);
        node.leftChild = leftChild.DeepCopy();
        node.rightChild = rightChild.DeepCopy();
        node.isLeaf = false;

        return node;
    }
}