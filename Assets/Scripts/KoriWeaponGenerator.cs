using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class KoriWeaponGenerator : MonoBehaviour
{
    [Header("코리 머리에서 무기 생성")]
    public Transform koriHeadPosition;
    [Header("무기 생성 목표 위치 기준점")]
    public Transform weaponMiddlePoint;
    [Header("무기 생성 반경")]
    public float radius = 1f;
    public float height = 3f;

    [Header("무기 프리팹 및 생성 간격 설정")]
    public GameObject[] weaponPrefab = new GameObject[2];
    //public string[] weaponTag = new string[2];
    public float generationInterval = 5f;

    private WaitForSeconds waitInterval;
    private Coroutine generationCoroutine;

    private List<(GameObject, float, Vector3)> throwObjects = new List<(GameObject, float, Vector3)>();
    private List<int> removeIndex = new List<int>();

    private void Start()
    {
        waitInterval = new WaitForSeconds(generationInterval);
        generationCoroutine = StartCoroutine(Generator());
    }

    private void SpawnPosition(GameObject weapon)
    {
        Vector3 middlepos = weaponMiddlePoint.position;
        middlepos += new Vector3(Random.Range(-radius, radius), 0f, Random.Range(-radius, radius));

        throwObjects.Add((weapon, 0f, middlepos));
    }

    private void Update()
    {
        if(throwObjects.Count > 0)
        {
            for(int i = 0; i < throwObjects.Count; i++)
            {
                GameObject obj = throwObjects[i].Item1;
                float t = throwObjects[i].Item2 + Time.deltaTime;
                Vector3 endPos = throwObjects[i].Item3;

                if(t >= 1f)
                {
                    obj.transform.position = endPos;
                    removeIndex.Add(i);
                    continue;
                }
                obj.transform.position = new Vector3(Mathf.Lerp(koriHeadPosition.position.x, endPos.x, t), endPos.y + Mathf.Sin(t * Mathf.PI) * height, Mathf.Lerp(koriHeadPosition.position.z, endPos.z, t));
                throwObjects[i] = (obj, t, endPos);
            }
            for(int i = removeIndex.Count - 1; i >= 0; i--)
            {
                throwObjects.RemoveAt(removeIndex[i]);
            }
            removeIndex.Clear();
        }
    }

    IEnumerator Generator()
    {
        while (true)
        {
            yield return waitInterval;
            int random = Random.Range(0, weaponPrefab.Length);

            //GameObject obj = ObjectPooler.Instance.SpawnFromPool(weaponTag[random], koriHeadPosition.position, Quaternion.identity);
            //SpawnPosition(obj);

            SpawnPosition(Instantiate(weaponPrefab[random], koriHeadPosition.position, Quaternion.identity));
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (weaponMiddlePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(weaponMiddlePoint.position, radius);
        }
    }
#endif
}
