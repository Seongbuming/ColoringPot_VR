﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using Valve.VR.InteractionSystem;
using Valve.VR;

class Dojagi
{
    public string fileName;
    public GameObject gameobject;
    public int location;    // -1일 시, 창고에 박아둠
}

public class ExhibitionTable : MonoBehaviour
{
    // 갤러리에서 보이는 각 조형물의 객체
    // 
    // 기능
    // - 조형물이 존재할 경우, 조형물을 씬 안에 생성함
    // - 조형물의 삭제 기능
    // - 조형물의 위치 이동 기능
    //

    public SteamVR_Action_Vector2 touchpadControl;
    public float moveSpeed = 2.0f;
    public GameObject Click;

    private string dojagiPath = "Assets/SavedPottery/";
    private string savePath = "Assets/SaveGallery/";

    private List<GameObject> dojagiPrefabs;
    private List<Dojagi> dojagiInfos;

    private GameObject player;
    private string[] datas;
    private GameObject tables; // 각 도자기들의 부모는 table. table 들의 부모는 tables.

    // MonoBehavior
    private void Awake()
    {

        dojagiPrefabs = new List<GameObject>();
        dojagiInfos = new List<Dojagi>();
        tables = GameObject.Find("Tables");
        player = GameObject.Find("Player");

        InitSaveInfos();
        InitDisplayDojagi();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            DeleteDojagi(Click);
        }

        // 플레이어 이동
        Vector2 t = touchpadControl.GetAxis(SteamVR_Input_Sources.Any);
        if (t != Vector2.zero)
        {
            Vector3 dir = Camera.main.transform.localRotation * Vector3.forward;
            //카메라가 바라보는 방향으로 팩맨도 바라보게 합니다.
            //팩맨의 Rotation.x값을 freeze해놓았지만 움직여서 따로 Rotation값을 0으로 세팅해주었습니다.
            transform.localRotation = new Quaternion(0, transform.localRotation.y, 0, transform.localRotation.w);

            if (t.y >= 0)
            {
                //바라보는 시점 방향으로 이동합니다.
                player.transform.position += dir * moveSpeed * Time.deltaTime;
            }
            else
            {
                player.transform.position += -dir * moveSpeed * Time.deltaTime;

            }
        }
    }

    // Private Func
    private void InitSaveInfos()
    {
        // 파일 path 확인
        if (string.IsNullOrEmpty(dojagiPath))
        {
            Debug.LogError("ExhibitionTable Path is NULL: " + dojagiPath);
            return;
        }
        
        if (string.IsNullOrEmpty(savePath))
        {
            Debug.LogError("ExhibitionTable Path is NULL: " + savePath);
            return;
        }

        DirectoryInfo di = new DirectoryInfo(dojagiPath);
        if (di.Exists == false)
        {
            Debug.LogError("ExhibitionTable Dict is Null");
            return;
        }

        // 도자기 파일들 불러오기
        List<FileInfo> fi = di.GetFiles().ToList();
        for (int i = 0; i < fi.Count; i++)
        {
            if (fi[i].Name.Contains(".meta")) continue;

            GameObject a = AssetDatabase.LoadAssetAtPath<GameObject>(dojagiPath + fi[i].Name);
            a.transform.localPosition = Vector3.zero;
            dojagiPrefabs.Add(a);
        }

        // 세이브 파일 불러오기
        datas = System.IO.File.ReadAllLines(savePath + "save.txt");
        for (int i =0; i< datas.Length; i++)
        {
            string[] data = datas[i].Split(',');

            Dojagi temp = new Dojagi();
            temp.fileName = data[0].ToString();
            temp.location = int.Parse(data[1].ToString());
            dojagiInfos.Add(temp);

        }

    }

    private void InitDisplayDojagi()
    {
        // 도자기들을 실제로 배치하는 함수
        for (int i = 0; i < dojagiPrefabs.Count; i++)
        {
            GameObject target = Instantiate(dojagiPrefabs[i]);
            target.name = target.name.Replace("(Clone)", "");

            foreach (Dojagi dt in dojagiInfos)
            {
                if (dt.fileName == target.name)
                {
                    if (dt.location == -1) continue;

                    target.transform.parent = tables.transform.GetChild(dt.location);
                    target.transform.localPosition = Vector3.zero;
                    target.transform.localScale = Vector3.one;
                }

            }
        }
    }

    private void DeleteDojagi( GameObject target )
    {
        // 인자는 raycast 등으로 선택한 도자기 오브젝트
        int index = -1;
        for (int i = 0; i < dojagiInfos.Count; i++)
        {
            Debug.Log("i " + i.ToString());
            if (target.name == dojagiInfos[i].fileName)
            {
                index = i;
            }
        }

        if (index == -1)
        {
            Debug.Log("삭제할 대상이 없음");
            return;
        }

        Destroy(target);
        System.IO.File.Delete(dojagiPath + dojagiInfos[index].fileName + ".prefab");
        System.IO.File.Delete(dojagiPath + dojagiInfos[index].fileName + ".prefab.meta");

        dojagiInfos.RemoveAt(index);
    }

}
