using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum MessageType
{
    CmdDrawCard, 
    RpcPlayCard, 
    CmdSyncGameState,
    CmdHeartbeat,
    RpcHeartbeatAck
}