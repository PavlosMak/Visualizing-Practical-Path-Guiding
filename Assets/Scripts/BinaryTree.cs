using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BinaryNode {

    private float min;
    private float max;
    public bool isLeaf = false;
    private BinaryNode rightChild;
    private BinaryNode leftChild;

    public BinaryNode(float min, float max, int maxDepth, int currentDepth) {
        int newDepth = currentDepth + 1;
        this.min = min;
        this.max = max;
        if(maxDepth <= currentDepth) {
            rightChild = null;
            leftChild = null;
            isLeaf = true;
        } else {
            float split = (min + max) / 2.0f;
            rightChild = new BinaryNode(min, split, maxDepth, newDepth);
            leftChild = new BinaryNode(split, max, maxDepth, newDepth);
        }
    }

    public float GetCenter() {
        return (min + max) / 2.0f;
    }

    public float GetSize() {
        return Mathf.Abs(max - min);
    }

    public bool Query(float point) {
        if(isLeaf && min <= point & point <= max) {
            // Debug.Log("Found point in " + min + "-" + max);
            return true;
        } else if(isLeaf) {
            // Debug.Log("Did not find point!");
            return false;
        }
        float split = (min + max) / 2.0f;
        if (point <= split) {
            return rightChild.Query(point);
        } else {
            return leftChild.Query(point);
        }        
    }    
}

public class BinaryTree : MonoBehaviour
{

    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private int maxDepth = 6;
    [SerializeField] private GameObject point;
    [SerializeField] private GameObject sprite;

    private BinaryNode root;
    
    // Start is called before the first frame update
    void Start()
    {
     root = new BinaryNode(minX,maxX,maxDepth,0); 
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Fire2")) {
            Vector3 mousePos =  Camera.main.ScreenToWorldPoint(Input.mousePosition);
            point.transform.position = new Vector3(mousePos.x,mousePos.y,0);
        }
        root.Query(point.transform.position.x); 
    }
}
