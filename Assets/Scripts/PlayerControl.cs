using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] float speed = 0.5f;

    List<GameObject> playerGroup = new List<GameObject>();
    void Born()
    {
        // init pos
        GameObject newPlayer = Instantiate(player);
        newPlayer.transform.position = Rand();
        newPlayer.transform.SetParent(transform);
        playerGroup.Add(newPlayer);
    }

    IEnumerator SmoothMove()
    {
        foreach (var p in playerGroup)
        {
            Vector3 startPos = p.transform.position;
            float startTime = Time.time;
            Vector3 endPos = Rand();
            while (p.transform.position != endPos)
            {
                //yield break;
                p.transform.position = Vector3.Lerp(startPos, endPos, Time.time - startTime);
                yield return null;
            }
        }
                //yield return null;
    }

    void Move()
    {
        StartCoroutine("SmoothMove");
    }

    private void Start()
    {
        KeyboardInputManager.GetKeyAction(KeyInput.ADD_PLAYER, Born);
        KeyboardInputManager.GetKeyAction(KeyInput.MOVE_PLAYER, Move);
    }

    #region utilities 
    // random function
    Vector3 Rand()
    {
        float poolX = (float)GameManager.GM.FM.BoundBox[1] / 2; // [-x, x]
        float poolY = (float)GameManager.GM.FM.BoundBox[3] / 2; // [-y, y]
        Vector3 pos = new Vector3(Random.Range(-poolX, poolX), Random.Range(-poolY, poolY), 0);
        return pos;
    }
    #endregion
}
