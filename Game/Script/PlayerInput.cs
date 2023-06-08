using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerInput : MonoBehaviour
{
    public float HorizontalInput;
    public float VerticalInput;
    //�������������й���
    public bool MouseButtonDown;
    //���Ʒ���
    public bool SpaceKeyDown;
    // Update is called once per frame
    void Update()
    {
        if(!MouseButtonDown && Time.timeScale != 0)
        {   
            MouseButtonDown = Input.GetMouseButtonDown(0);
        }
        if(!SpaceKeyDown  && Time.timeScale != 0)
        {
            SpaceKeyDown = Input.GetKeyDown(KeyCode.Space);
        }

        HorizontalInput = Input.GetAxisRaw("Horizontal");
        VerticalInput = Input.GetAxisRaw("Vertical");

    }

    private void OnDisable()
    {
        ClearCache();
    }

    public void ClearCache()
    {
        SpaceKeyDown = false;
        MouseButtonDown = false;
        HorizontalInput = 0;
        VerticalInput = 0;
    }
}
