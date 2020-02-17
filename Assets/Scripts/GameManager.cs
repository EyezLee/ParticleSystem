using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager gm;
    public static GameManager GM { get { return gm; } }

    public FireflyManager FM;
    private void Awake()
    {
        gm = this;
    }
}
