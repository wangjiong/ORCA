using RVO;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Test01 : MonoBehaviour {
    public GameObject mSpherePrefab01;
    public GameObject mSpherePrefab02;

    List<GameObject> mSpheres = new List<GameObject>();
    IList<RVO.Vector2> goals;
    System.Random random;

    float speed = 5.0f;

    public static List<Sphere> mSphereScritps = new List<Sphere>();

    float space = 2.2f;

    int N = 20;

    void Start () {
        goals = new List<RVO.Vector2>();
        random = new System.Random();

        // 创建静态阻挡
        GameObject[] obj = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        foreach (GameObject g in obj) {
            if (g.tag.Equals("obstacle")) {
                Vector3 scale = g.transform.lossyScale;
                Vector3 position = g.transform.position;

                IList<RVO.Vector2> obstacle = new List<RVO.Vector2>();
                obstacle.Add(new RVO.Vector2(position.x + scale.x / 2, position.z + scale.z / 2));
                obstacle.Add(new RVO.Vector2(position.x - scale.x / 2, position.z + scale.z / 2));
                obstacle.Add(new RVO.Vector2(position.x - scale.x / 2, position.z - scale.z / 2));
                obstacle.Add(new RVO.Vector2(position.x + scale.x / 2, position.z - scale.z / 2));
                Simulator.Instance.addObstacle(obstacle);
            }
        }
        Simulator.Instance.processObstacles();

        // 创建小球
        Simulator.Instance.setAgentDefaults(10.0f, 10, 1f, 1.0f, 0.5f, speed, new RVO.Vector2(0.0f, 0.0f));
        CreateSquad(new Vector3(-30, 0, 0) , mSpherePrefab01 , 1);
        CreateSquad(new Vector3(30, 0, 0) , mSpherePrefab02 , 1);
        // 创建大球
        CreateGameObject(new Vector3(0, 0, 50), mSpherePrefab02 , 5);
       
    }

    // 方阵
    void CreateSquad(Vector3 position, GameObject spherePrefab , float mass) {
        for (int i = 0; i < N; i++) {
            for (int j = 0; j < N; j++) {
                // orca
                RVO.Vector2 p = new RVO.Vector2(i * space + position.x, j * space + position.z);
                int index = Simulator.Instance.addAgent(p);
                //Simulator.Instance.setAgentMass(index, mass);
                // 目标点
                goals.Add(p);
                // 物体
                GameObject g = GameObject.Instantiate(spherePrefab);
                mSpheres.Add(g);
                mSphereScritps.Add(g.AddComponent<Sphere>());
            }
        }
    }

    // 大球
    void CreateGameObject(Vector3 position, GameObject spherePrefab, float radius) {
        Simulator.Instance.setAgentDefaults(10.0f, 10, 1f, 1.0f, radius / 2f, speed , new RVO.Vector2(0.0f, 0.0f));
        // orca
        RVO.Vector2 p = new RVO.Vector2(position.x, position.z);
        int index = Simulator.Instance.addAgent(p);
        //Simulator.Instance.setAgentMass(index, 20.0f);
        // 目标点
        goals.Add(p);
        // 物体
        GameObject g = GameObject.Instantiate(mSpherePrefab01);
        g.transform.localScale = new Vector3(radius, radius, radius);
        mSpheres.Add(g);
        mSphereScritps.Add(g.AddComponent<Sphere>());
    }

    int key = 0;
    Vector3 hitPoint01;
    void Update() {
        Simulator.Instance.setTimeStep(Time.deltaTime);
        setPreferredVelocities();
        Simulator.Instance.doStep();

        for (int i = 0; i < Simulator.Instance.getNumAgents(); ++i) {
            RVO.Vector2 p = Simulator.Instance.getAgentPosition(i);
            mSpheres[i].transform.position = new Vector3(p.x(), 0, p.y());
        }
        
        if (Input.GetKey(KeyCode.Q)) {
            key = 1;
        }else if(Input.GetKey(KeyCode.W)) {
            key = 2;
        }else if (Input.GetKey(KeyCode.E)) {
            key = 3;
        }
        // 鼠标点击Plane，设置目标点
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButton(0)) {
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo)) {
                if (hitInfo.collider.name == "Plane") {
                    hitPoint01 = hitInfo.point;
                    Vector3 position = hitInfo.point;
                    int index = 0;
                    if (key == 1) {
                        index = 0;
                    }else if (key == 2) {
                        index = N * N;
                    }
                    if (key == 3) {
                        goals[goals.Count-1] = new RVO.Vector2(position.x, position.z);
                    } else if(key == 1 || key == 2) {
                        for (int i = 0; i < N; i++) {
                            for (int j = 0; j < N; j++) {
                                RVO.Vector2 p = new RVO.Vector2(i * space + position.x, j * space + position.z);
                                goals[index++] = p;
                            }
                        }
                    }
                }
            }
        }
        Debug.DrawLine(ray.origin, hitPoint01, Color.green);
    }

    void setPreferredVelocities() {
        for (int i = 0; i < Simulator.Instance.getNumAgents(); ++i) {
            RVO.Vector2 goalVector = goals[i] - Simulator.Instance.getAgentPosition(i);

            if (RVOMath.absSq(goalVector) > 1.0f) {
                goalVector = RVOMath.normalize(goalVector) * speed ;
            }

            Simulator.Instance.setAgentPrefVelocity(i, goalVector);

            float angle = (float)random.NextDouble() * 2.0f * (float)Math.PI;
            float dist = (float)random.NextDouble() * 0.0001f;

            Simulator.Instance.setAgentPrefVelocity(i, Simulator.Instance.getAgentPrefVelocity(i) +
                dist * new RVO.Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)));
        }
    }
    
}