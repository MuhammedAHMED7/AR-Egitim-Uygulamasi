using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CubeMove : MonoBehaviour
{
    public TextMeshPro text;

    void Start()
    {
        if (text != null)
            text.gameObject.SetActive(false);
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        transform.Translate(x * 5 * Time.deltaTime, 0, z * 5 * Time.deltaTime);
       

        //MOUSE ÝLE SAÐ-SOL DÖNME
        float mouseX = Input.GetAxis("Mouse X") * 100f * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

        // Yazý kameraya baksýn
        if (text != null)
        {
            text.transform.LookAt(Camera.main.transform);
            text.transform.Rotate(0, 180, 0);
        }
    }

    void OnMouseDown()
    {
        Debug.Log("Týklandý!");
        GetComponent<Renderer>().material.color = Color.red;

        if (text != null)
            text.gameObject.SetActive(!text.gameObject.activeSelf);
    }
    void OnMouseEnter()
    {
        GetComponent<Renderer>().material.color = Color.yellow;
    }

    void OnMouseExit()
    {
        GetComponent<Renderer>().material.color = Color.white;
    }
}