using com.tvd12.ezyfoxserver.client.unity;
using UnityEngine;

public class SocketEventProcessor : EzyAbstractEventProcessor
{
    [SerializeField]
    private SocketConfigVariable socketConfig;

    protected override string GetZoneName()
    {
        return socketConfig.Value.ZoneName;
    }
}
