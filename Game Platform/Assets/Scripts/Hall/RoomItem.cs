using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomItem : MonoBehaviour
{
    public void SetRoomInfo(string gameName, string caption, bool hasPassword, int status, int count, int maxCount)
    {
        Text game_text = this.gameObject.transform.Find("game_image/Text").GetComponent<Text>();
        Text status_text = this.gameObject.transform.Find("status_text").GetComponent<Text>();
        Text count_text = this.gameObject.transform.Find("count_image/Text").GetComponent<Text>();
        Text caption_text = this.gameObject.transform.Find("caption_image/Text").GetComponent<Text>();

        game_text.text = gameName;
        {
            string p1 = "";
            switch (status)
            {
                case 1:
                    p1 = "<color=#7CFC00FF>可加入</color>";
                    break;
                case 2:
                    p1 = "<color=#B22222FF>已满</color>";
                    break;
                case 3:
                    p1 = "<color=#B22222FF>游戏中</color>";
                    break;
            }
            string p2 = hasPassword ? "密码" : "";
            status_text.text = string.Format("{0}  <color=#FFD700FF>{1}</color>", p1, p2);
        }
        count_text.text = count + " / " + maxCount;
        caption_text.text = caption;
    }

    public void SetVisiblity(bool visiblity)
    {
        this.gameObject.SetActive(visiblity);
    }

}
