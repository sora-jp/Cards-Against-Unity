using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MessageType
{
    RpcPlayCard,
    RpcVoteOnClient,
    RpcRevealCard,
    CmdRevealCard,
    CmdSetClientScore,
    CmdDrawCard,
    CmdSyncGameState,
    CmdOnClientDisconnect,
    CmdBeginVoting,
    CmdOnClientPlayedCard,
    CmdSetGuid,
    CmdRemoveCard,
    CmdBeginNewRound,
    CmdOnClientConnected,
    CmdHeartbeat,
    RpcHeartbeatAck
}