using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MessageType
{
    RpcHeartbeatAck,
    RpcPlayCard,
    RpcVoteOnClient,
    RpcRevealCard,
    CmdRevealCard,
    CmdSetClientScore,
    CmdDrawCard,
    CmdSyncGameState,
    CmdHeartbeat,
    CmdOnClientDisconnect,
    CmdBeginVoting,
    CmdOnClientPlayedCard,
    CmdSetGuid,
    CmdRemoveCard,
    CmdBeginNewRound
}