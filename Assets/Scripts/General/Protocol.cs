﻿public enum MessageType
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
    RpcHeartbeatAck,
    RpcSetName
}