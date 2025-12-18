using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Debug = UnityEngine.Debug;
using System.Text;

public class ChatItem
{
    public string userId;
    public string talkerName;
    public bool isMy;
    public string chat;
    public AudioClip clip;

    public ChatItem(string _userId,string _talkerName, bool _isMy, string _chat)
    {
        this.userId = _userId;
        this.talkerName = _talkerName;
        this.isMy = _isMy;
        this.chat = _chat;
    }

    public ChatItem(string _userId, string _talkerName, bool _isMy, AudioClip _clip)
    {
        this.userId = _userId;
        this.talkerName = _talkerName;
        this.isMy = _isMy;
        this.clip = _clip;
    }
}

public class ChatPanel : BasePopup
{
    private Button m_HideMaskBtn;

    public GameObject leftBubblePrefab;
    public GameObject rightBubblePrefab;

    private ScrollRect scrollRect;
    private Scrollbar scrollbar;

    private RectTransform content;

    [SerializeField]
    private float stepVertical; //上下两个气泡的垂直间隔
    [SerializeField]
    private float stepHorizontal; //左右两个气泡的水平间隔
    [SerializeField]
    private float maxTextWidth;//文本内容的最大宽度

    private float lastPos; //上一个气泡最下方的位置
    private float halfHeadLength;//头像高度的一半

    public Button sendBtn;
    public InputField sendField;

    public List<ChatItem> chatItems = new List<ChatItem>();

    private Transform m_VoicePanel;
    private GameObject m_VoicePressBtn;
    private GameObject m_VoiceRelease;

    private bool m_IsInVoiceSpeak;
    private bool m_VoiceSpeakState;
    private AudioClip m_RecordClip;

    private DateTime m_SVTime;

    [SerializeField]private int m_Frequency;

    Dictionary<string,string> s = new Dictionary<string, string>();

    public bool ischatList;
    private RawImage tempHead;

    void Awake()
    {
        Init();
        InitI();

        lastPos = -60;
        sendBtn.onClick.AddListener(()=> {

            if (GameManager.GetSingleton().myNetID < 0)
            {
                PopupCommon.GetSingleton().ShowView("请入坐再聊天！", null, false);
                sendField.text = "";
                return;
            }

            if (sendField.text != "")
            {
                NetMngr.GetSingleton().Send(InterfaceGame.sendChat, new object[] { 0, sendField.text });
                sendField.text = "";
                HideView();
            }
        });
        // sendField.onValueChanged.AddListener((value) =>
		// {
        //     string tempText = sendField.text;
		// 	if(tempText.Length%20 == 0){
                
        //         sendField.text = tempText + "\n"; 
        //     }

		// });

        m_HideMaskBtn = transform.Find("HideMask").GetComponent<Button>();

        m_VoicePanel = transform.Find("SpeakPanel");
        m_VoiceRelease = transform.Find("SpeakPanel/Cancel").gameObject;
        m_VoicePressBtn = transform.Find("SpeakBtn").gameObject;
        { 
            EventTriggerListener listener = EventTriggerListener.Get(m_VoiceRelease);
            listener.onEnter += go =>
            {
                m_VoiceSpeakState = false;
            };
            listener.onExit += go =>
            {
                m_VoiceSpeakState = true;
            };
        }
        {
            EventTriggerListener listener = EventTriggerListener.Get(m_VoicePressBtn);
            listener.onDown += PressVoice;
        }
        gameObject.SetActive(false);

        m_HideMaskBtn.onClick.AddListener(() =>
        {
            HideView();
        });
    }

    public void AddChatItem(string _userId, string _talkerName, bool _isMy, AudioClip _clip)
    {
        ChatItem item = new ChatItem(_userId, _talkerName, _isMy, _clip);
        chatItems.Add(item);
        {
            AddBubble(_userId, _talkerName, _clip, _isMy);
        }
    }

    private void PlayClip(AudioClip _clip)
    {
        AudioSource audioSource = GameObject.FindObjectOfType<AudioSource>();
        if (audioSource == null)
        {
            audioSource = Tao_Controller.instance.gameObject.AddComponent<AudioSource>();
        }
        if (audioSource != null)
        {
            audioSource.clip = _clip;
            audioSource.Play();
        }
    }

    public void AddBubble(string _userId, string talkerNamer, AudioClip _clip, bool isMy)
    {
        GameObject newBubble = isMy ? Instantiate(rightBubblePrefab, this.content) : Instantiate(leftBubblePrefab, this.content);
        //halfHeadLength = newBubble.transform.Find("head/Head").GetComponent<RectTransform>().rect.height / 2;
        //halfHeadLength = 0;
        //设置气泡内容
        newBubble.transform.Find("VoiceBg").gameObject.SetActive(true);
        string countStr = _clip.length.ToString("0") + "''";
        newBubble.transform.Find("VoiceBg/Count").GetComponent<Text>().text = countStr;
        newBubble.transform.Find("VoiceBg").GetComponent<Button>().onClick.AddListener(()=>
        {
            PlayClip(_clip);
        });
        PlayClip(_clip);
        //Text text = newBubble.transform.Find("bubble/text").GetComponent<Text>();
        //text.text = content;
        //if (text.preferredWidth > maxTextWidth)
        //{
        //    text.GetComponent<LayoutElement>().preferredWidth = maxTextWidth;
        //}
        RawImage head = newBubble.gameObject.transform.Find("head/Head").GetComponent<RawImage>();
        Text talkerName = newBubble.transform.Find("TalkerName").GetComponent<Text>();
        talkerName.text = talkerNamer;
        for (int j = 0; j < GameManager.GetSingleton().MapLocalSeatPlayer.Count; j++)
        {
            PlayInfo info = GameUIManager.GetSingleton().roomNumSitActivePlayerTrans.GetChild(GameManager.GetSingleton().MapLocalSeatPlayer[j]).GetComponent<PlayInfo>();
            if (info != null && info.userID == _userId)
            {
                if (info.cricleImage.overrideSprite.texture != null)
                {
                    head.texture = info.cricleImage.overrideSprite.texture;
                }
                break;
            }
        }
        //计算气泡的水平位置
        float hPos = 0;
        if (isMy)
        {
            hPos = this.content.rect.width;
        }
        //float hPos = isMy ? stepHorizontal / 2 : -stepHorizontal / 2;
        //计算气泡的垂直位置
        //float vPos = -stepVertical - halfHeadLength + lastPos;
        newBubble.transform.localPosition = new Vector2(hPos, lastPos);

        //更新lastPos
        //Image bubbleImage = newBubble.transform.Find("bubble").GetComponent<Image>();
        //float imageLength = GetContentSizeFitterPreferredSize(bubbleImage.GetComponent<RectTransform>(), bubbleImage.GetComponent<ContentSizeFitter>()).y;
        //if (imageLength > stepVertical)
        //{
        //    lastPos = lastPos - imageLength;
        //}
        //else
        //{
        //    lastPos = lastPos - stepVertical;
        //}

        //lastPos = vPos - imageLength;
        //更新content的长度
        //if (-lastPos > this.content.rect.height-300)
        //{
        //    this.content.sizeDelta = new Vector2(this.content.rect.width, -lastPos+300);
        //}
        

        scrollRect.verticalNormalizedPosition = 0;//使滑动条滚轮在最下方

    }

    public void AddChatItem(string _userId,string talkerName, bool _isMy, string _chat)
    {
        ChatItem item = new ChatItem(_userId,talkerName, _isMy, _chat);
        chatItems.Add(item);

        //if (this.isActiveAndEnabled)
        {
            AddBubble(_userId, talkerName, _chat, _isMy);
        }
    }

    public void AddBubble(string _userId, string talkerNamer,string content, bool isMy)
    {
        GameObject newBubble = isMy ? Instantiate(rightBubblePrefab, this.content) : Instantiate(leftBubblePrefab, this.content);
        //halfHeadLength = newBubble.transform.Find("head/Head").GetComponent<RectTransform>().rect.height / 2;
        halfHeadLength = 0;
        //设置气泡内容
        Text text = newBubble.transform.Find("bubble/text").GetComponent<Text>();

        string tempText = content;
        StringBuilder sb = new StringBuilder(tempText);
        int line = 1;
        for (int i = 0; i < tempText.Length; i++)
        {
            if( i % 14 == 0 && i != 0){
                line ++;
                 sb.Insert(i, "\n");
             }
        }
        text.text = sb.ToString(); 

        
      
       
        this.content.sizeDelta  = new Vector2(1000,200 + (line-1)*150 );
        //  Debug.Log("-----------------:"+this.content.rect.width+","+ this.content.rect.height);
      
        
        //if (text.preferredWidth > maxTextWidth)
        //{
        //    text.GetComponent<LayoutElement>().preferredWidth = maxTextWidth;
        //}
        RawImage head = newBubble.gameObject.transform.Find("head/Head").GetComponent<RawImage>();
        tempHead = head;
        Text talkerName = newBubble.transform.Find("TalkerName").GetComponent<Text>();
        talkerName.text = talkerNamer;
        // if (!ischatList)
        // {
            for (int j = 0; j < GameManager.GetSingleton().MapLocalSeatPlayer.Count; j++)
            {
            Debug.Log("-------------------1111111----------------");
            PlayInfo info = GameUIManager.GetSingleton().roomNumSitActivePlayerTrans.GetChild(GameManager.GetSingleton().MapLocalSeatPlayer[j]).GetComponent<PlayInfo>();
            if (info != null && info.userID == _userId)
            {
                Debug.Log("-------------------2222----------------");
                if (info.cricleImage.overrideSprite.texture != null)
                {
                    Debug.Log("-------------------3333----------------");
                    head.texture = info.cricleImage.overrideSprite.texture;
                }
                break;
            }
            }
        // }
        // else{
        //     GameTools.GetSingleton().GetTextureNet("http://39.105.96.86:8080/upLoadFile/default_7.png", GetSprtie);
        // }
       

        



            //计算气泡的水平位置
        // float hPos = 0;
        // if (isMy)
        // {
        //     hPos = this.content.rect.width;
        // }
        //float hPos = isMy ? stepHorizontal / 2 : -stepHorizontal / 2;
        //计算气泡的垂直位置
        //float vPos = -stepVertical - halfHeadLength + lastPos;
        // newBubble.transform.localPosition = new Vector2(hPos, lastPos);

        //更新lastPos
        //Image bubbleImage = newBubble.transform.Find("bubble").GetComponent<Image>();
        //float imageLength = GetContentSizeFitterPreferredSize(bubbleImage.GetComponent<RectTransform>(), bubbleImage.GetComponent<ContentSizeFitter>()).y;
        //if (imageLength > stepVertical)
        //{
        //    lastPos = lastPos - imageLength;
        //}
        //else
        //{
        //    lastPos = lastPos - stepVertical;
        //}
       
        //lastPos = vPos - imageLength;
        //更新content的长度
        //if (-lastPos > this.content.rect.height-300)
        //{
        //    this.content.sizeDelta = new Vector2(this.content.rect.width, -lastPos+300);
        //}
        
        // this.content.sizeDelta  = new Vector2(this.content.rect.width,200 + (line-1)*150 );

        scrollRect.verticalNormalizedPosition = 0;//使滑动条滚轮在最下方
    }

    //public Vector2 GetContentSizeFitterPreferredSize(RectTransform rect, ContentSizeFitter contentSizeFitter)
    //{
    //    LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    //    return new Vector2(HandleSelfFittingAlongAxis(0, rect, contentSizeFitter),
    //        HandleSelfFittingAlongAxis(1, rect, contentSizeFitter));
    //}

    //private float HandleSelfFittingAlongAxis(int axis, RectTransform rect, ContentSizeFitter contentSizeFitter)
    //{
    //    ContentSizeFitter.FitMode fitting =
    //        (axis == 0 ? contentSizeFitter.horizontalFit : contentSizeFitter.verticalFit);
    //    if (fitting == ContentSizeFitter.FitMode.MinSize)
    //    {
    //        return LayoutUtility.GetMinSize(rect, axis);
    //    }
    //    else
    //    {
    //        return LayoutUtility.GetPreferredSize(rect, axis);
    //    }
    //}

    public void showChat()
    {
        //for (int i = 0; i < chatItems.Count; i++)
        //{
        //    AddBubble(chatItems[i].chat, chatItems[i].isMy);
        //}
    }

    public void InitI()
    {
        chatItems.Clear();
        scrollRect = GetComponentInChildren<ScrollRect>();
        scrollbar = GetComponentInChildren<Scrollbar>();
        content = transform.Find("infoPanel/Viewport/Content").GetComponent<RectTransform>();
        //lastPos = 0;
       // halfHeadLength = leftBubblePrefab.transform.Find("head/Head").GetComponent<RectTransform>().rect.height / 2;
    }

    // Use this for initialization
    void Start () {
        showChat();
    }
	
	// Update is called once per frame
	void Update () 
    {
        if (m_IsInVoiceSpeak)
        {
            if (Input.GetMouseButton(0))
            {
                return;
            }
            if (m_VoiceSpeakState)
            {
                SendVoice();
            }

            m_VoicePanel.gameObject.SetActive(false);
            m_IsInVoiceSpeak = false;
        }
	}

    private void SendVoice()
    {
        if (GameManager.GetSingleton().myNetID < 0)
        {
            PopupCommon.GetSingleton().ShowView("请入坐再聊天！", null, false);
            return;
        }

        AudioClip clip = m_RecordClip;
        int position = Microphone.GetPosition(Microphone.devices[0]);
        if (position <= 0 || position > clip.samples)
        {
            position = clip.samples;
        }
        float[] soundData = new float[position * clip.channels];
        clip.GetData(soundData, 0);
        clip = AudioClip.Create(clip.name, position, clip.channels, clip.frequency, false);
        clip.SetData(soundData, 0);
        Microphone.End(null);
        TimeSpan sub = new TimeSpan(DateTime.Now.Ticks - m_SVTime.Ticks);
        int length = sub.Seconds * m_Frequency;
        if (length <= 0)
        {
            PopupCommon.GetSingleton().ShowView("说话时间太短", null, false);
            return;
        }
        Thread threadR = new Thread(() =>
        {
            byte[] bytes = GetRealAudio(soundData);
            Loom.QueueOnMainThread(pr =>
            {
                Thread thread = new Thread(() =>
                    {
                        byte[] bs = Tao_Controller.Compress(bytes);
                        Loom.QueueOnMainThread((param) =>
                        {
                            Tao_Controller.instance.SendVoice(bs, path =>
                            {
                                if (!string.IsNullOrEmpty(path))
                                    NetMngr.GetSingleton().Send(InterfaceGame.sendChat, new object[] { 2, path });
                            });
                        }, null);
                    }) { IsBackground = true };
                thread.Start();
            }, null);
        }) {IsBackground = true};
        threadR.Start();
    }

    private void PressVoice(GameObject go)
    {
        if (Microphone.devices.Length <= 0)
        {
            PopupCommon.GetSingleton().ShowView("请打开麦克风权限", null, false);
        }
        else
        {
            Microphone.End(null);
            m_VoiceSpeakState = true;
            m_VoicePanel.gameObject.SetActive(true);
            m_IsInVoiceSpeak = true;
            m_SVTime = DateTime.Now;
            m_RecordClip = Microphone.Start(null, false, 10, m_Frequency);
        }
    }

    private byte[] GetRealAudio(float[] soundData)
    {
        int rescaleFactor = 32767;
        byte[] outData = new byte[soundData.Length * 2];
        for (int i = 0; i < soundData.Length; i++)
        {
            short temShort = (short)(soundData[i] * rescaleFactor);
            byte[] temData = BitConverter.GetBytes(temShort);
            outData[i * 2] = temData[0];
            outData[i * 2 + 1] = temData[1];
        }
        return outData;
    }

    //public override void OnAddComplete()
    //{

    //}

    //public override void OnAddStart()
    //{
       
    //}

    //public override void OnRemoveComplete()
    //{

    //}

    //public override void OnRemoveStart()
    //{

    //}
   
    public void GetChatCallBack(Hashtable data) {
        ischatList = false;
        chatItems.Clear();
        ClearList(this.content);
        ArrayList List = data["chatList"] as ArrayList;
        for (int i = 0; i < List.Count; i++)
        {
            Hashtable ht = List[i] as Hashtable;
            bool isMy = false;
            string talkerName = string.Empty;
            string strSendID = ht["sendId"].ToString();
            for (int j = 0; j < GameUIManager.GetSingleton().roomNumSitActivePlayerTrans.childCount; j++)
            {
                PlayInfo pInfo = GameUIManager.GetSingleton().roomNumSitActivePlayerTrans.GetChild(j).GetComponent<PlayInfo>();
                if (pInfo == null)
                {
                    continue;
                }
                if (pInfo.userID == strSendID)
                {
                    talkerName = pInfo.playerName.text;
                }
                if (pInfo.userID == strSendID && GameManager.GetSingleton().myNetID == j)
                {
                    isMy = true;
                    break;
                }
            }
            AddChatItem(strSendID, talkerName, isMy, ht["message"].ToString());
        }
        
    }

     public override void ShowView(Action onComplete = null)
    {
        if (ischatList)
        {
            NetMngr.GetSingleton().Send(InterfaceGame.sendChatHis,new object[] {});
        }
        
        base.ShowView(onComplete);
    }

    public void GetSprtie(Texture s)
    {
        if (s != null)
        {
            if (tempHead.texture != null && !tempHead.texture.name.StartsWith("默认"))
            {
                GameObject.Destroy(tempHead.texture);
            }
            tempHead.texture = s;
        }
    }

    private void ClearList(Transform parent)
    {
        int childCount = parent.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }
  
   

}
