using UnityEngine;
using System.Collections;

public static class Helper
{

    public static IEnumerator WaitForSeconds(int seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    
}
