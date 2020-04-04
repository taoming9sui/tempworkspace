using System;
using System.Collections;
using System.Collections.Generic;
using WebSocketSharp;

public class PlayerSocket : WebSocket
{
    private bool m_valid = false;

    public new EventHandler OnOpen;
    public new EventHandler<CloseEventArgs> OnClose;
    public new EventHandler<MessageEventArgs> OnMessage;
    public new EventHandler<ErrorEventArgs> OnError;
    public new bool IsAlive { get { return base.IsAlive && m_valid; } }


    public PlayerSocket(string url) : base(url) {
        this.Compression = CompressionMethod.Deflate;

        base.OnClose += M_OnClose;
        base.OnMessage += M_OnMessage;
        base.OnError += M_OnError;

    }

    private void M_OnMessage(object sender, MessageEventArgs e)
    {
        if (m_valid)
        {
            //合法的连接
            this.OnMessage(sender, e);
        }
        else
        {
            //需要验证 回应会话
            string data = e.Data;
            if(data == "Valid")
            {
                //匹配成功
                m_valid = true;
                if (this.OnOpen != null) this.OnOpen(this, new EventArgs());
            }
            else
            {
                //发送匹配码
                string sessionId = data;
                string salt = DateTime.UtcNow.ToShortTimeString();
                string code = string.Format("{0}-{1}", sessionId, salt);
                string md5 = SecurityHelper.CreateMD5(code);
                this.Send(md5);
            }

        }
    }
    private void M_OnClose(object sender, CloseEventArgs e)
    {
        if (this.OnClose != null) this.OnClose(sender, e);
    }
    private void M_OnError(object sender, ErrorEventArgs e)
    {
        if (this.OnError != null) this.OnError(sender, e);
    }
}
