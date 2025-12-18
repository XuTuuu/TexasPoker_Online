using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PopupReplay : MonoBehaviour {
    public Button btn;
	// Use this for initialization
	void Awake () {
        btn = transform.Find("Image").GetComponent<Button>();
        btn.onClick.AddListener(delegate {
            StaticFunction.Getsingleton().UnloadScene(1);
        });
	}
    private void Start()
    {
        Debug.Log("PopupReplay Start");
    }
    public void StartGame()
    {
        Debug.Log("PopupReplay StartGame");
        StartCoroutine(ReviewGame());
    }

    private IEnumerator ReviewGame()
    {
        HallManager.GetSingleton().theGamePaiTopPanel.gameObject.SetActive(false);
        for (int i = 0; i < StaticData.reviewData.Length; i++)
        {
            Hashtable table = TinyJSON.jsonDecode(StaticData.reviewData[i]) as Hashtable;
            if (table == null)
                continue;
            //Debug.Log(table);
            string method = table["m"].ToString();
            Hashtable args = table["a"] as Hashtable;
            //Debug.Log("回调方法:"+method);
            Message m_MyMessage = new Message();
            m_MyMessage.hashtable = table;
            NetMngr.GetSingleton().regidReview(m_MyMessage);
            yield return new WaitForSeconds(1f);
        }
        yield return new WaitForSeconds(1f);
    }
}
