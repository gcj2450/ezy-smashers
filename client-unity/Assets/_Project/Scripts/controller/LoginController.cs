using com.tvd12.ezyfoxserver.client.constant;
using com.tvd12.ezyfoxserver.client.logger;
using com.tvd12.ezyfoxserver.client.request;
using com.tvd12.ezyfoxserver.client.support;
using com.tvd12.ezyfoxserver.client.unity;
using UnityEngine;
using UnityEngine.Events;
using Object = System.Object;

public class LoginController : EzyAbstractController
{
    [SerializeField]
    private SocketConfigVariable socketConfigHolderVariable;

	EzySocketConfig config;

    [SerializeField]
	private StringVariable username;
	
	[SerializeField]
	private StringVariable password;
	
	[SerializeField]
	private UnityEvent<string> myPlayerJoinedLobbyEvent;

	private new void OnEnable()
	{
		base.OnEnable();
		config = GetSocketConfig();

        AddHandler<Object>(Commands.JOIN_LOBBY, OnJoinedLobby);
	}

	public void Login()
	{
		LOGGER.debug("Login username = " + username.Value + ", password = " + password.Value);
		LOGGER.debug("Socket clientName = " + socketProxy.getClient().getName());
		
		socketProxy.onLoginSuccess<Object>(HandleLoginSuccess);
		socketProxy.onAppAccessed<Object>(HandleAppAccessed);
		
		// Login to socket server
		socketProxy.setLoginUsername(username.Value);
		socketProxy.setLoginPassword(password.Value);
#if UNITY_WEBGL && !UNITY_EDITOR
		socketProxy.setUrl(config.Value.WebSocketUrl);
#else
        socketProxy.setUrl(config.TcpUrl);
		socketProxy.setUdpPort(config.UdpPort);
		socketProxy.setDefaultAppName(config.AppName);
		socketProxy.setTransportType(EzyTransportType.UDP);
		socketProxy.onUdpHandshake<Object>(HandleUdpHandshake);
#endif
		socketProxy.connect();
	}
	
	private void HandleLoginSuccess(EzySocketProxy proxy, Object data)
	{
		LOGGER.debug("Log in successfully");
#if UNITY_WEBGL && !UNITY_EDITOR
		socketProxy.send(new EzyAppAccessRequest(socketConfigVariable.Value.AppName));
#endif
	}

	private void HandleUdpHandshake(EzySocketProxy proxy, Object data)
	{
		LOGGER.debug("HandleUdpHandshake");
		socketProxy.send(new EzyAppAccessRequest(config.AppName));
	}

    
	private void HandleAppAccessed(EzyAppProxy proxy, Object data)
	{
		LOGGER.debug("App access successfully");
		appProxy.send(Commands.JOIN_LOBBY);
	}

	void OnJoinedLobby(EzyAppProxy appProxy, Object data)
	{
		myPlayerJoinedLobbyEvent?.Invoke(username.Value);
	}

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
