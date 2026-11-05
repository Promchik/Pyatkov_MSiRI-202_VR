using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class SpawnerScript : MonoBehaviour
{

    public int[] Mattess;
    public GameObject cubie;

    void Start()
    {
        if (cubie == null) return;

        foreach (int a in Mattess)
        {
            int aa = a;
            aa = aa / 2;

            if (aa > 50)
            {
                print(aa);
            }

            Vector3 spawnPosition = cubie.transform.position + new Vector3(a * 1.5f, 0, 0);
            GameObject newCube = Instantiate(cubie, spawnPosition, cubie.transform.rotation);

           
            Destroy(newCube.GetComponent<SpawnerScript>());

            newCube.name = "кубик " + a;
        }
    }
}

