using Unity.MLAgents.SideChannels;
using UnityEngine;

public class ScoreManager : SideChannel
{
    private int goodScore = 0;
    private int badScore = 0;

    public void AddGoodScore()
    {
        goodScore++;
    }

    public void AddBadScore()
    {
        badScore++;
    }

    public void SendSphereData()
    {
        using (var msg = new OutgoingMessage())
        {
            msg.WriteInt32(goodScore);
            msg.WriteInt32(badScore);
            QueueMessageToSend(msg);
        }
    }
    protected override void OnMessageReceived(IncomingMessage msg)
    {
        // No-op: implement logic here if you want to handle incoming messages.
    }
}