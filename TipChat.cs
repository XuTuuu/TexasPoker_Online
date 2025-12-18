using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class TipChat : MonoBehaviour {

    private Image tip;
    private Text tipText;

    private float timer = 0;
    private bool isStart = false;


    private static TipChat _instance;

    public static TipChat Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }
            else
            {
                _instance = new TipChat();
                return _instance;
            }
        }
    }
    private void Awake()
    {
        _instance = this;
        tip = GetComponent<Image>();
        tipText = transform.Find("Text").GetComponent<Text>();
        HideTip(0);
    }
	void Start () {
	
	}
	
	void Update ()
    {
        if (isStart)
        {
            timer += Time.deltaTime;
            if (timer >= 1)
            {
                HideTip(0.1f);
                timer = 0;
                isStart = false;
            }
        }
    }

    public void HideTip(float time)
    {
        
        // tip.DOFade(0, time);
        // tipText.DOFade(0, time).OnComplete(()=> { this.gameObject.SetActive(false); });
    }

    public void ShowTip(string name, string message, float time)
    {
        this.gameObject.SetActive(true);
		tipText.text = name + message;
        // tip.DOFade(1f, time);
        // tipText.DOFade(1f, time).OnComplete(() => isStart = true);

		int xx = Random.Range(0,3);
        string bgNamePath = "V2.0/Game/TextBg_" + xx.ToString();
        tip.sprite = Resources.Load<Sprite>(bgNamePath);
        float y = Random.Range(-300, 300);
        this.transform.localPosition = new Vector3(750, y, 0);
        tipText.transform.DOLocalMoveX(-1000, time).OnComplete(() => { this.gameObject.SetActive(false); });
        
      
    }
    // public void SetTipText(string str)
    // {
    //     tipText.text = str;
    // }
    // public void showMsg(string str)
    // {
    //     SetTipText(str);
    //     ShowTip(0.3f);
    // }

    public IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);
        HideTip(0.1f);
    }
}
