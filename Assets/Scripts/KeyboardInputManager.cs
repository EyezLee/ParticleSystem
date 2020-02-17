using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum KeyInput
{
    ADD_PLAYER = '1',
    MOVE_PLAYER = 'w'
}

public class KeyboardInputManager : MonoBehaviour
{
    static KeyboardInputManager km;

    Dictionary<char, Action> KeyboardActions = new Dictionary<char, Action>();

    public static void GetKeyAction(KeyInput keyCode, Action func)
    {
        if(!km.KeyboardActions.ContainsKey((char)keyCode))
        {
            km.KeyboardActions[(char)keyCode] = func;
        }
    }

    private void Awake()
    {
        km = this;
    }
    private void Update()
    {
        if(Input.anyKey)
        {
            foreach(char c in Input.inputString)
            {
                // if contains call function
                if(km.KeyboardActions.ContainsKey(c))
                {
                    km.KeyboardActions[c].Invoke();
                }
            }
        }
    }
}
