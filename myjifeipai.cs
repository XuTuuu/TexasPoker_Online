using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class myjifeipai : BasePopup {
    public Text mang;
    public Text mangTitle;
    public Text jifenpai;
    public Text mymoney;
    public Text serviceMoney;
    public Slider mslider;
    Button btn;
    int mseatNum;
    private Button guanbiBtn;
    
    private void Awake()
    {
        mang = transform.Find("mang").GetComponent<Text>();
        mangTitle = transform.Find("Text").GetComponent<Text>();
        jifenpai = transform.Find("takemoney").GetComponent<Text>();
        mymoney = transform.Find("mymoney").GetComponent<Text>();
        serviceMoney = transform.Find("serviceMoney").GetComponent<Text>();
        mslider = transform.Find("Slider").GetComponent<Slider>();
        btn = transform.Find("sure").GetComponent<Button>();
        guanbiBtn = transform.Find("guanbi").GetComponent<Button>();
        btn.onClick.AddListener(()=> {
           // GameManager.GetSingleton().takeMoney = (int)(mslider.value) ;
            if (mseatNum == -1)
            {
                 if (GameManager.GetSingleton().roomGameType == 2)//短牌
                 {
                    NetMngr.GetSingleton().Send(InterfaceGame.addCoin, new object[] { (int)(mslider.value * 50) +"" });

                 }else{
                    NetMngr.GetSingleton().Send(InterfaceGame.addCoin, new object[] { (int)(mslider.value * 100* GameManager.GetSingleton().roomXiaoMang) +"" });
                 }
            }
            else{
                if (GameManager.GetSingleton().roomGameType == 2)//短牌
                {
                    NetMngr.GetSingleton().Send(InterfaceGame.DesktopPlayerSitdownRequest, new object[] { mseatNum, (int)(mslider.value*50 * GameManager.GetSingleton().ante) });
                }
                else
                {
                    NetMngr.GetSingleton().Send(InterfaceGame.DesktopPlayerSitdownRequest, new object[] { mseatNum, (int)(mslider.value*100 * GameManager.GetSingleton().roomXiaoMang) });
                }
            }
            HideView();
        });
        mslider.onValueChanged.AddListener((v)=> {
            int num;
            if (GameManager.GetSingleton().roomGameType == 2)//短牌
            {
                num = (int)v * 50 * GameManager.GetSingleton().ante;
            }else{
                num = (int)v * 100 * GameManager.GetSingleton().roomXiaoMang;
            }
            
            jifenpai.text = num + "";
            serviceMoney.text = GameManager.GetSingleton().serviceMoney * num / 100 + "";
            Debug.Log("---num:"+num);
        });

        guanbiBtn.onClick.AddListener(()=> {
             HideView();
        }); 



        Init();
    }
    public override void ShowView(Action onComplete = null)
    {
        base.ShowView(onComplete);
        base.hideNeedSendMsg = true;
    }
    void Start () {
        //showInfo();
    }
    public void showInfo(int seatNum)
    {
        mseatNum = seatNum;

        if (GameManager.GetSingleton().roomGameType == 2)//短牌
        {
            mang.text = GameManager.GetSingleton().ante.ToString();
            mangTitle.text = "前注";
        }else{
            mang.text = GameManager.GetSingleton().roomXiaoMang + "/" + GameManager.GetSingleton().roomDaMang;
            mangTitle.text = "小盲/大盲";
        }
        
        int num = GameManager.GetSingleton().roomMinTakeMoneyRatio * 100 * GameManager.GetSingleton().roomXiaoMang;
        // int num = 100 * GameManager.GetSingleton().roomXiaoMang;
        jifenpai.text = num + "";
        serviceMoney.text = GameManager.GetSingleton().serviceMoney* num/100 + "";
        mymoney.text = StaticData.gold + "";
        mslider.minValue = GameManager.GetSingleton().roomMinTakeMoneyRatio;
        mslider.maxValue = GameManager.GetSingleton().roomMaxTakeMoneyRatio;
        mslider.value= GameManager.GetSingleton().roomMinTakeMoneyRatio;
        Debug.Log("------min:"+GameManager.GetSingleton().roomMinTakeMoneyRatio+"----max:"+GameManager.GetSingleton().roomMaxTakeMoneyRatio);
    }
}
