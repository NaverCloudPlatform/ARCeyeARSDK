using System.Collections;
using System.Collections.Generic;
using ARCeye;
using UnityEngine;
using UnityEngine.UI;

public class SplashViewController : ViewController<SplashView>
{
    /// <summary>
    //  Splash 화면을 출력하는 메서드.
    //  3초 뒤에 FadeOut이 되면서 complete 액션을 호출한다.
    /// </summary>

    public void HideWithFadeOut()
    {
        UIEffect.FadeOut(m_View, 1.5f, ()=>{
            Show(false);
        });
    }
}
