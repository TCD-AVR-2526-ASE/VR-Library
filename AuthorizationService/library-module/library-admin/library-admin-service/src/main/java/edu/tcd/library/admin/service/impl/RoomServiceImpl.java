package edu.tcd.library.admin.service.impl;

import cn.hutool.core.bean.BeanUtil;
import cn.hutool.core.util.ObjectUtil;
import cn.hutool.json.JSONUtil;
import edu.tcd.library.admin.entity.Room;
import edu.tcd.library.admin.service.RoomService;
import edu.tcd.library.common.core.domain.UserDto;
import edu.tcd.library.common.core.utils.RedisUtils;
import edu.tcd.library.common.security.utils.SecurityUtils;
import org.springframework.data.redis.core.RedisTemplate;
import org.springframework.stereotype.Service;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

@Service
public class RoomServiceImpl implements RoomService {

    private final String ROOM_CACHE_KEY = "Room:Management:";

    private final RedisUtils redisService = new RedisUtils();

    @Override
    public List<Room> listAll() {
        UserDto userDto = SecurityUtils.getUserCache();
        Long userID = userDto.getId();
        String roomUserCacheKey = ROOM_CACHE_KEY + userID;
        Map<Object, Object> roomMaps = redisService.hGetAll(roomUserCacheKey);
        List<Room> roomList = new ArrayList<>();
        roomMaps.forEach((key, value) -> {
            Room room = JSONUtil.toBean(JSONUtil.toJsonStr(value), Room.class);
            roomList.add(room);
        });
        return roomList;
    }

    @Override
    public Boolean addRooms(List<Room> rooms) {
        UserDto userDto = SecurityUtils.getUserCache();
        Long userID = userDto.getId();
        String roomUserCacheKey = ROOM_CACHE_KEY + userID;
        Map<String, Object> roomMaps = new HashMap<>();
        for (Room room : rooms) {
            String guid = room.getGUID();
            roomMaps.put(guid, JSONUtil.toJsonStr(room));
        }
        redisService.hSetAll(roomUserCacheKey, roomMaps);
        return true;
    }
}
