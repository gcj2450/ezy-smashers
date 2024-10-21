using System;
using System.Collections.Generic;
using com.tvd12.ezyfoxserver.client.entity;
using com.tvd12.ezyfoxserver.client.factory;
using com.tvd12.ezyfoxserver.client.support;
using com.tvd12.ezyfoxserver.client.unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyController : EzyAbstractController
{
    [SerializeField]
    private SocketConfigVariable socketConfigHolderVariable;
    EzySocketConfig config;

	public Button CreateRoomBtn;
    public Button RefreshRoomListBtn;

	public ListUI listUI;
	public GameObject roomButtonPrefab;

    private void Awake()
    {
		CreateRoomBtn.onClick.AddListener(OnCreateMMORoom);
		RefreshRoomListBtn.onClick.AddListener(RefreshRoomIdList);
    }

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
        PlayerJoinedMmoRoom(roomId);
	}

    public void PlayerJoinedMmoRoom(int roomId)
    {
        RoomRepository.GetInstance().UpdatePlayingRoomId(roomId);
        SceneManager.LoadScene("GameLoungeScene");
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

		SetRoomIdList(roomIds);

	}

    public void SetRoomIdList(List<int> roomIdList)
    {
        roomIdList.Sort();
        LOGGER.debug("SetRoomIdList: " + string.Join(",", roomIdList));
        listUI.RemoveAllItems();
        foreach (int roomId in roomIdList)
        {
            GameObject go = listUI.AddItem(roomButtonPrefab);
            ButtonUI buttonUI = go.GetComponent<ButtonUI>();
            buttonUI.Index = roomId;
            buttonUI.onClickEvent.AddListener(RequestJoinMMORoom);
            go.GetComponentInChildren<Text>().text = "Room #" + roomId;
        }
    }

    #region public methods

    /// <summary>
    /// 刷新房间列表
    /// </summary>
    public void RefreshRoomIdList()
	{
		LOGGER.debug("OnRefreshRoomIdList");
		appProxy.send(Commands.GET_MMO_ROOM_ID_LIST);
	}

    /// <summary>
    /// 创建房间
    /// </summary>
    public void OnCreateMMORoom()
	{
		LOGGER.debug("OnCreateMMORoom");
		appProxy.send(Commands.CREATE_MMO_ROOM);
	}

    /// <summary>
    /// 加入房间请求
    /// </summary>
    /// <param name="roomId"></param>
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
