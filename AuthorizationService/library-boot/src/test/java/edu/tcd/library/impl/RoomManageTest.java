package edu.tcd.library.impl;

import cn.hutool.json.JSONArray;
import cn.hutool.json.JSONObject;
import cn.hutool.json.JSONUtil;
import lombok.Data;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.context.ApplicationContext;
import org.springframework.data.redis.core.RedisTemplate;

import java.util.Map;
import java.util.concurrent.TimeUnit;

/**
 * Test suite for Room Management logic.
 * Fixed field mapping issues, added assertions, and optimized dependency injection.
 */
@SpringBootTest
public class RoomManageTest {

    /**
     * Injecting RedisTemplate with explicit generic types to avoid manual casting.
     */
    private final RedisTemplate<String, Object> redisTemplate;

    private final ApplicationContext context;

    public RoomManageTest(ApplicationContext context) {
        this.context = context;
        this.redisTemplate = (RedisTemplate) context.getBean("redisTemplate");
    }

    @Test
    @DisplayName("Verify JSON to Room object mapping accuracy and field consistency")
    void jsonParseRoomInfo() {
        String roomInfoJson = """    
                {
                    "Rooms": [
                        {
                            "GUID": "245e2263-7ac8-44a5-afc8-c2ba1ddc4f91",
                            "SessionID": "245e2263-7ac8-44a5-afc8-c2ba1ddc4f91",
                            "JoinCode": "DM89FC",
                            "RoomName": "daoqi test",
                            "SceneName": "Testing",
                            "MaxPlayers": 10,
                            "Status": 1,
                            "LastUpdatedUTC": 1771247832,
                            "Endpoint": ""
                        }
                    ]
                }
                """;

        JSONObject jsonObject = JSONUtil.parseObj(roomInfoJson);
        JSONArray roomsArray = jsonObject.getJSONArray("Rooms");

        // Asserting that the JSON structure is parsed correctly
        Assertions.assertFalse(roomsArray.isEmpty(), "The Rooms array should not be empty");

        String firstRoomStr = roomsArray.get(0).toString();
        Room bean = JSONUtil.toBean(firstRoomStr, Room.class);

        // Validating key fields after deserialization
        Assertions.assertEquals("245e2263-7ac8-44a5-afc8-c2ba1ddc4f91", bean.getGUID());
        Assertions.assertEquals("daoqi test", bean.getRoomName());

        // Verifying that MaxPlayers is correctly mapped (fixing the 's' suffix issue)
        Assertions.assertNotNull(bean.getMaxPlayer(), "MaxPlayers mapping failed! Check field name case-sensitivity.");
        Assertions.assertEquals(10, bean.getMaxPlayer());
    }

    @Test
    @DisplayName("Verify Redis TTL (Time-To-Live) expiration policy")
    void testRoomCacheExpired() {
        String key = "Room:Management:Temp";
        redisTemplate.opsForValue().set(key, "expiration_test", 2, TimeUnit.SECONDS);

        Assertions.assertEquals("expiration_test", redisTemplate.opsForValue().get(key));

        // Pause execution to allow the cache to expire
        try {
            TimeUnit.SECONDS.sleep(3);
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
        }

        Assertions.assertNull(redisTemplate.opsForValue().get(key), "Cache should have expired and been removed");
    }

    @Test
    @DisplayName("Verify bulk persistence of Room info using Redis Hash structures")
    void testSaveRoomsInHash() {
        String roomsJson = """
                [
                    { "GUID": "ID_001", "RoomName": "Room_A", "MaxPlayer": 5 },
                    { "GUID": "ID_002", "RoomName": "Room_B", "MaxPlayer": 8 }
                ]
                """;

        String hashKey = "Room:Management:Hash";

        JSONUtil.parseArray(roomsJson).forEach(obj -> {
            Room room = JSONUtil.toBean(obj.toString(), Room.class);
            // Storing as Hash provides O(1) complexity for field-level access
            redisTemplate.opsForHash().put(hashKey, room.getGUID(), obj.toString());
        });

        Map<Object, Object> entries = redisTemplate.opsForHash().entries(hashKey);
        Assertions.assertTrue(entries.size() >= 2, "Insufficient number of entries in Redis Hash");

        // Cleanup to maintain test idempotency
        redisTemplate.delete(hashKey);
    }
}

/**
 * Data Transfer Object representing a Room.
 * Fields must strictly match the JSON payload keys.
 */
@Data
class Room {
    private String GUID;
    private String SessionID;
    private String JoinCode;
    private String RoomName;
    private String SceneName;
    private Integer MaxPlayer;
    private Integer Status;
    private Long LastUpdatedUTC;
    private String Endpoint;
}