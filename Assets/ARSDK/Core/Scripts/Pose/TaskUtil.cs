using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ARCeye
{
    public class TaskUtil
    {
        public static async Task WaitUntil(System.Func<bool> action)
        {
            while(!action.Invoke())
            {
                await Task.Delay(50);
            }
        }
    }
}