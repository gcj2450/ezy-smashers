using System.Collections.Generic;
using com.tvd12.ezyfoxserver.client.entity;
using com.tvd12.ezyfoxserver.client.factory;
using com.tvd12.ezyfoxserver.client.support;
using com.tvd12.ezyfoxserver.client.unity;
using UnityEngine;
using UnityEngine.Events;

public class LobbyController : EzyAbstractController
{
    [SerializeField]
    private SocketConfigVariable socketConfigHolderVariable;
    EzySocketConfig config;

    [SerializeField]
	private UnityEvent<List<int>> mmoRoomIdListUpdateEvent;

	[SerializeField]
	private UnityEvent<int> playerJoinedMmoRoomEvent;
	
	protected new void OnEnable()
	{
		base.OnEnable();

        config = GetSocketConfig();

        AddHandler<EzyObject>(Commands.CREATE_MMO_ROOM, JoinRoom);
		AddHandler<EzyArray>(Commands.GET_MMO_ROOM_ID_LIST, OnMMORoomIdListResponse);
		AddHandler<EzyObject>(Commands.JOIN_MMO_ROOM, JoinRoom);
		RefreshRoomIdList();
	}

	private void JoinRoom(EzyAppProxy appProxy, EzyObject data)
	{
		int roomId = data.get<int>("roomId");
		LOGGER.debug("JoinRoom roomId = " + roomId);
		playerJoinedMmoRoomEvent?.Invoke(roomId);
	}

	private void OnMMORoomIdListResponse(EzyAppProxy appProxy, EzyArray data)
	{
		LOGGER.debug("OnMMORoomIdListResponse " + data);
		EzyArray roomIdArray = data.get<EzyArray>(0);
		List<int> roomIds = new List<int>();
		for (int i = 0; i < roomIdArray.size(); ++i)
		{
			roomIds.Add(roomIdArray.get<int>(i));
		}
		LOGGER.debug("OnMMORoomIdListResponse roomIds = " + string.Join(", ", roomIds));
		mmoRoomIdListUpdateEvent?.Invoke(roomIds);
	}

	#region public methods

	public void RefreshRoomIdList()
	{
		LOGGER.debug("OnRefreshRoomIdList");
		appProxy.send(Commands.GET_MMO_ROOM_ID_LIST);
	}

	public void OnCreateMMORoom()
	{
		LOGGER.debug("OnCreateMMORoom");
		appProxy.send(Commands.CREATE_MMO_ROOM);
	}

	public void RequestJoinMMORoom(int roomId)
	{
		LOGGER.debug("RequestJoinMMORoom: roomId = " + roomId);
		EzyObject data = EzyEntityFactory.newObjectBuilder()
			.append("roomId", roomId)
			.build();
		appProxy.send(Commands.JOIN_MMO_ROOM, data);
	}

    #endregion

    protected override EzySocketConfig GetSocketConfig()
    {
        var configVariable = socketConfigHolderVariable.Value;
        return EzySocketConfig.GetBuilder()
            .ZoneName(configVariable.ZoneName)
            .AppName(configVariable.AppName)
            .WebSocketUrl(configVariable.WebSocketUrl)
            .TcpUrl(configVariable.TcpUrl)
            .UdpPort(configVariable.UdpPort)
            .UdpUsage(configVariable.UdpUsage)
            .EnableSSL(configVariable.EnableSSL)
            .Build();
    }
}
