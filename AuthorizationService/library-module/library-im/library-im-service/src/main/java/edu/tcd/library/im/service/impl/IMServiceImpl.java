package edu.tcd.library.im.service.impl;

import cn.hutool.core.util.ObjectUtil;
import cn.hutool.extra.spring.SpringUtil;
import cn.hutool.json.JSONUtil;
import edu.tcd.library.admin.entity.UmsAdmin;
import edu.tcd.library.admin.service.UmsAdminService;
import edu.tcd.library.common.core.utils.RedisUtils;
import edu.tcd.library.im.bo.IMReceiveMsg;
import edu.tcd.library.im.bo.IMSocketSendMsg;
import edu.tcd.library.im.config.NettyConfig;
import edu.tcd.library.im.service.IMService;
import io.netty.channel.Channel;
import io.netty.handler.codec.http.websocketx.TextWebSocketFrame;
import org.jetbrains.annotations.NotNull;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.dao.DataAccessException;
import org.springframework.data.redis.core.RedisOperations;
import org.springframework.data.redis.core.RedisTemplate;
import org.springframework.data.redis.core.SessionCallback;
import org.springframework.stereotype.Service;
import org.springframework.util.Assert;

import java.util.List;

/**
 * IM Service Implementation
 */
@Service
public class IMServiceImpl implements IMService {

    private final RedisUtils redisService = new RedisUtils();

    private final RedisTemplate<String, Object> redisTemplate = SpringUtil.getBean("redisTemplate");

    @Autowired
    private UmsAdminService adminService;

    private final static String OFFLINE_MSG_LIST = "offline:msg:list";

    @Override
    public void personalMessage(Long userId, IMSocketSendMsg sendMsg) {
        // Get active Netty channel for the user
        Channel channel = NettyConfig.getChannel(userId.toString());

        if (ObjectUtil.isNull(channel)) {
            // If user is offline, store message in Redis
            String redisKey = String.format("%s:%s", OFFLINE_MSG_LIST, userId);
            redisService.lPush(redisKey, JSONUtil.toJsonStr(sendMsg));
        } else {
            // If user is online, push message via WebSocket
            channel.writeAndFlush(new TextWebSocketFrame(JSONUtil.toJsonStr(sendMsg)));
        }
    }

    @Override
    public void groupMessage(List<Long> group, IMSocketSendMsg sendMsg) {
        for (Long userId : group) {
            personalMessage(userId, sendMsg);
        }
    }

    @Override
    public void globalMessage(IMSocketSendMsg sendMsg) {
        // Broadcast to all administrative users
        List<UmsAdmin> allUsers = adminService.list();
        for (UmsAdmin user : allUsers) {
            personalMessage(user.getId(), sendMsg);
        }
    }

    @Override
    public List<Object> getOfflineMessage(Long userId) {
        String redisKey = String.format("%s:%s", OFFLINE_MSG_LIST, userId);
        // Execute Redis transaction to ensure atomic read and delete
        List<Object> execute = redisTemplate.execute(new SessionCallback<List<Object>>() {
            @Override
            public List<Object> execute(@NotNull RedisOperations operations) throws DataAccessException {
                operations.multi();
                operations.opsForList().range(redisKey, 0, -1);
                operations.delete(redisKey);
                return operations.exec();
            }
        });
        // Return the result of the first command in transaction (range)
        return (List<Object>) execute.get(0);
    }

    @Override
    public Boolean receive(IMReceiveMsg msg) {
        List<Long> receiverIds = msg.getReceiverIds();
        Assert.notEmpty(receiverIds, "Message receiver list cannot be empty!");

        String message = String.format("You received a message from %s: %s", msg.getSender(), msg.getData());

        IMSocketSendMsg sendMsg = IMSocketSendMsg.builder()
                .type(msg.getType())
                .identity(msg.getIdentity())
                .data(message)
                .build();

        for (Long receiverId : receiverIds) {
            personalMessage(receiverId, sendMsg);
        }
        return true;
    }
}