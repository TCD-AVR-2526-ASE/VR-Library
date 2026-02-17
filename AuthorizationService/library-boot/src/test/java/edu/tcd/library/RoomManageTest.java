package edu.tcd.library;


import cn.hutool.json.JSON;
import cn.hutool.json.JSONArray;
import cn.hutool.json.JSONObject;
import cn.hutool.json.JSONUtil;
import edu.tcd.library.im.server.IMServer;
import lombok.Data;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.context.ApplicationContext;
import org.springframework.data.redis.core.RedisTemplate;

import java.util.Map;
import java.util.concurrent.TimeUnit;

@SpringBootTest
public class RoomManageTest {

    @Autowired
    private ApplicationContext context;


    @Test
    void jsonParseRoomInfo() {
        String roomInfo = """    
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
        JSONObject parse = JSONUtil.parseObj(roomInfo);
        String rooms = parse.getStr("Rooms");
        JSONArray roomsArray = JSONUtil.parseArray(rooms);
        String firstRoomStr = roomsArray.get(0).toString();
        Room firstRoom = JSONUtil.toBean(firstRoomStr, Room.class);
        System.out.println(firstRoom.getRoomName());
    }

    @Test
    void testRoomCacheExpired() {
        RedisTemplate template = (RedisTemplate) context.getBean("redisTemplate");
        template.opsForValue().set("Room:Management", "value", 20, TimeUnit.SECONDS);
    }


    @Test
    void testSaveRoomCache() {
        String roomInfo = """
                {
                            "GUID": "245e2263-7ac8-44a5-afc8-c2ba1ddc4f93",
                            "SessionID": "245e2263-7ac8-44a5-afc8-c2ba1ddc4f91",
                            "JoinCode": "DM89FC",
                            "RoomName": "daoqi test2",
                            "SceneName": "Testing",
                            "MaxPlayers": 10,
                            "Status": 1,
                            "LastUpdatedUTC": 1771247832,
                            "Endpoint": ""
                        }
                """;

        RedisTemplate template = (RedisTemplate) context.getBean("redisTemplate");
        template.opsForHash().put("Room:Management", "245e2263-7ac8-44a5-afc8-c2ba1ddc4f93", roomInfo);
    }


    @Test
    void testSaveRooms() {
        String rooms = """
                [
                    {
                        "GUID": "245e2263-7ac8-44a5-afc8-c2ba1ddc4f95",
                        "SessionID": "245e2263-7ac8-44a5-afc8-c2ba1ddc4f91",
                        "JoinCode": "DM89FC",
                        "RoomName": "daoqi test1",
                        "SceneName": "Testing",
                        "MaxPlayers": 10,
                        "Status": 1,
                        "LastUpdatedUTC": 1771247832,
                        "Endpoint": ""
                    },
                    {
                        "GUID": "245e2263-7ac8-44a5-afc8-c2ba1ddc4f93",
                        "SessionID": "245e2263-7ac8-44a5-afc8-c2ba1ddc4f91",
                        "JoinCode": "DM89FC",
                        "RoomName": "daoqi test2",
                        "SceneName": "Testing",
                        "MaxPlayers": 10,
                        "Status": 1,
                        "LastUpdatedUTC": 1771247832,
                        "Endpoint": ""
                    },
                    {
                        "GUID": "245e2263-7ac8-44a5-afc8-c2ba1ddc4f91",
                        "SessionID": "245e2263-7ac8-44a5-afc8-c2ba1ddc4f91",
                        "JoinCode": "DM89FC",
                        "RoomName": "daoqi test3",
                        "SceneName": "Testing",
                        "MaxPlayers": 10,
                        "Status": 1,
                        "LastUpdatedUTC": 1771247832,
                        "Endpoint": ""
                    }
                ]
                """;
        RedisTemplate template = (RedisTemplate) context.getBean("redisTemplate");
        JSONUtil.parseArray(rooms).forEach(room -> {
            Room bean = JSONUtil.toBean(room.toString(), Room.class);
            String guid = bean.getGUID();
            template.opsForHash().put("Room:Management", guid, room.toString());
        });
    }

    @Test
    void testGetAllRooms() {
        RedisTemplate template = (RedisTemplate) context.getBean("redisTemplate");
        Map<Object, Object> entries = template.opsForHash().entries("Room:Management");
        System.out.println(entries.size());
    }
}

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
