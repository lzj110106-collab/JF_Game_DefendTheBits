using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_kill : MonoBehaviour {

    bool  moveCount = true;
    //private void LateUpdate()
    //{
    //    if (Input.GetMouseButton(0) && moveCount == true)
    //    {
    //        OnMouseDown();
    //    }
    //    if (this.transform.parent.tag != "Boom")
    //    {
    //        moveCount = true;
    //    }
    //}
    // 开始接触
    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag != "Boom")
        {
            // 销毁当前游戏物体
            var collider_name = collider.gameObject;
            GameObject killObject = collider_name.GetComponent<EnemyCharacter>().gameObject;
            Debug.Log("开始接触Name is " + killObject.name);

            killObject.GetComponent<EnemyCharacter>().KilledRightNow();
            Destroy(this.transform.gameObject);
        }
    }

    //private void OnMouseDown()
    //{
    //    Vector3 Pos = Camera.main.WorldToScreenPoint(transform.position);
    //    Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Pos.z);
    //    //transform.position = Camera.main.ScreenToWorldPoint(mousePos);
    //    transform.position = new Vector3(Camera.main.ScreenToWorldPoint(mousePos).x, 0f, Camera.main.ScreenToWorldPoint(mousePos).z);
       
    //}

    //private void OnTriggerExit(Collider other)
    //{
       
    //    if (other.tag == "Boom")
    //    {
    //        Debug.Log(" other1 : " + other.gameObject.name);
    //        //if (Input.GetMouseButtonUp(0))
    //        //{
    //        GameObject temp = other.transform.gameObject;
    //        this.transform.parent = temp.transform;
    //        this.transform.localPosition = new Vector3(0f,0.125f,0f);
    //        moveCount = false;
    //        //}

    //    }
    //}
}
