using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSeat : MonoBehaviour
{
    public Image headImage;
    public Text nameText;
    public Text seatNumber;
    public Sprite defaultHead;
    public GameObject checkedMark;
    public GameObject readyMark;
    public GameObject speakingMark;
    public enum StasusMark { Checked, Ready, Speaking };

    public void SetPlayer(int seatNo, Sprite sprite = null, string playerName = null)
    {
        //设置编号
        seatNumber.text = (seatNo + 1).ToString();
        //设置头像
        if (sprite == null)
            sprite = defaultHead;
        headImage.sprite = sprite;
        //设置名称
        if (playerName == null)
            playerName = "空座位";
        nameText.text = playerName;
    }

    public void SetStasusMark(StasusMark stasusMark, bool show)
    {
        switch (stasusMark)
        {
            case StasusMark.Checked:
                checkedMark.SetActive(show);
                break;
            case StasusMark.Ready:
                readyMark.SetActive(show);
                break;
            case StasusMark.Speaking:
                speakingMark.SetActive(show);
                break;
        }
    }
}
