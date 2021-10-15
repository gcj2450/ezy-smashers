package com.youngmonkeys.app.service.impl;

import com.tvd12.ezyfox.bean.annotation.EzyAutoBind;
import com.tvd12.ezyfox.bean.annotation.EzySingleton;
import com.tvd12.ezyfox.util.EzyLoggable;
import com.tvd12.gamebox.entity.MMOPlayer;
import com.tvd12.gamebox.entity.Player;
import com.tvd12.gamebox.manager.PlayerManager;
import com.tvd12.gamebox.math.Vec3;
import com.tvd12.gamebox.math.Vec3s;
import com.youngmonkeys.app.game.GameRoom;
import com.youngmonkeys.app.game.PlayerLogic;
import com.youngmonkeys.app.game.constant.GameConstants;
import com.youngmonkeys.app.game.shared.PlayerAttackData;
import com.youngmonkeys.app.game.shared.PlayerInputData;
import com.youngmonkeys.app.game.shared.PlayerSpawnData;
import com.youngmonkeys.app.service.GamePlayService;
import com.youngmonkeys.app.service.RoomService;
import lombok.Setter;

import java.util.*;
import java.util.concurrent.ThreadLocalRandom;
import java.util.stream.Collectors;

@Setter
@EzySingleton
public class GamePlayServiceImpl extends EzyLoggable implements GamePlayService {
	
	@EzyAutoBind
	RoomService roomService;
	
	@EzyAutoBind
	private PlayerManager<Player> globalPlayerManager;
	
	/**
	 * Map playerName to playerPositionHistory
	 */
	private Map<String, SortedMap<Integer, Vec3>> globalPlayersPositionHistory;
	
	@Override
	public void handlePlayerInputData(String playerName, PlayerInputData inputData, float[] nextRotation) {
		MMOPlayer player = roomService.getPlayer(playerName);
		synchronized (player) {
			Vec3 currentPosition = player.getPosition();
			Vec3 nextPosition = PlayerLogic.GetNextPosition(inputData, currentPosition);
			logger.info("next position = {}", nextPosition);
			player.setPosition(nextPosition);
			player.setRotation(nextRotation[0], nextRotation[1], nextRotation[2]);
			player.setClientTimeTick(inputData.getTime());
			
			SortedMap<Integer, Vec3> playerPositionHistory = globalPlayersPositionHistory.get(playerName);
			playerPositionHistory.put(inputData.getTime(), nextPosition);
			if (playerPositionHistory.size() > GameConstants.MAX_HISTORY_SIZE) {
				playerPositionHistory.remove(playerPositionHistory.firstKey());
			}
		}
	}
	
	@Override
	public List<PlayerSpawnData> spawnPlayers(List<String> playerNames) {
		List<PlayerSpawnData> answer = playerNames.stream().map(
				playerName -> new PlayerSpawnData(
						playerName,
						Vec3s.toArray(
								new Vec3(
										ThreadLocalRandom.current().nextFloat() * 10,
										0,
										ThreadLocalRandom.current().nextFloat() * 10
								)
						)
				)
		).collect(Collectors.toList());
		
		answer.forEach(playerSpawnData -> {
			MMOPlayer player = (MMOPlayer) globalPlayerManager.getPlayer(playerSpawnData.getPlayerName());
			synchronized (player) {
				player.setPosition(
						new Vec3(
								playerSpawnData.getPosition().get(0),
								playerSpawnData.getPosition().get(1),
								playerSpawnData.getPosition().get(2)
						)
				);
			}
		});
		
		return answer;
	}
	
	@Override
	public boolean authorizeAttack(String playerName, PlayerAttackData playerAttackData) {
		Vec3 attackPosition = new Vec3(
				playerAttackData.getAttackPosition()[0],
				playerAttackData.getAttackPosition()[1],
				playerAttackData.getAttackPosition()[2]
		);
		int victimTick = playerAttackData.getOtherClientTick();
		String victimName = playerAttackData.getVictimName();
		
		// Roll back to get victim position at victimTick, a.k.a Lag compensation
		SortedMap<Integer, Vec3> victimPositionHistory = globalPlayersPositionHistory.get(victimName);
		Vec3 pastVictimPosition;
		if (victimPositionHistory.containsKey(victimTick)) {
			pastVictimPosition = victimPositionHistory.get(victimTick);
		} else {
			pastVictimPosition = victimPositionHistory.get(victimPositionHistory.firstKey());
			throw new IllegalStateException("Server doesn't contain victimTick");
		}
		
		// Check whether that position is near attackPosition
		if (pastVictimPosition.distance(attackPosition) > GameConstants.ATTACK_RANGE_UPPER_BOUND) {
			return false;
		}
		
		// Check if attackPosition is near my player's position
		MMOPlayer myPlayer = roomService.getPlayer(playerName);
		if (myPlayer.getPosition().distance(attackPosition) > GameConstants.HAMMER_DISTANCE_UPPER_BOUND) {
			return false;
		}
		
		return true;
	}
	
	@Override
	public void resetPlayersPositionHistory(List<String> playerNames) {
		playerNames.forEach(playerName -> {
			if (!globalPlayersPositionHistory.containsKey(playerName)) {
				globalPlayersPositionHistory.put(playerName,
						Collections.synchronizedSortedMap(new TreeMap<>()));
			} else {
				globalPlayersPositionHistory.get(playerName).clear();
			}
		});
	}
}
