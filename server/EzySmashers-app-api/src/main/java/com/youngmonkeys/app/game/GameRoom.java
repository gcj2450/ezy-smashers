package com.youngmonkeys.app.game;

import com.tvd12.gamebox.entity.MMOPlayer;
import com.tvd12.gamebox.entity.MMORoom;
import com.tvd12.gamebox.handler.MMORoomUpdatedHandler;
import com.tvd12.gamebox.manager.PlayerManager;
import com.tvd12.gamebox.manager.SynchronizedPlayerManager;
import com.tvd12.gamebox.math.Vec3;
import lombok.Getter;

import java.util.List;
import java.util.Map;

public class GameRoom extends MMORoom {
    @Getter
    protected MMOPlayer master;
	
    public GameRoom(Builder builder) {
        super(builder);
    }
	
	@Override
	public void update() {
		super.update();
	}
	
	public void addPlayer(MMOPlayer player) {
        PlayerManager<MMOPlayer> playerManager = this.getPlayerManager();
        if(playerManager.containsPlayer(player)) {
            return;
        }
        synchronized (this) {
            if(playerManager.isEmpty()) {
                master = player;
            }
            playerManager.addPlayer(player);
        }
    }

    public void removePlayer(MMOPlayer player) {
        PlayerManager<MMOPlayer> playerManager = this.getPlayerManager();
        synchronized (this) {
            playerManager.removePlayer(player);
            if(master == player && !playerManager.isEmpty()) {
                master = playerManager.getPlayerByIndex(0);
            }
        }
    }

    public boolean isEmpty() {
        return this.getPlayerManager().isEmpty();
    }
    
    public static Builder builder() {
        return new Builder();
    }
    
    public static class Builder extends MMORoom.Builder {
        
        @Override
        protected GameRoom newProduct() {
            return new GameRoom(this);
        }
    
        @Override
        public GameRoom build() {
            return (GameRoom) super.build();
        }
    }
}
