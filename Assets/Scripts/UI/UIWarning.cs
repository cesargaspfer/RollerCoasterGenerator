using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIWarning : MonoBehaviour
{
    private float timer;

    public void Warn(string warnKey)
    {
        this.transform.GetChild(0).GetComponent<Text>().text = Translator.inst.GetTranslation(warnKey);
        if(timer > 0f)
            timer = 3f;
        else
            StartCoroutine(TimerToClose());
    }

    private IEnumerator TimerToClose()
    {
        this.GetComponent<Animator>().Play("ShowUIWarning");
        timer = 3f;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        timer = 0f;
        this.GetComponent<Animator>().Play("HideUIWarning");
    }
}
