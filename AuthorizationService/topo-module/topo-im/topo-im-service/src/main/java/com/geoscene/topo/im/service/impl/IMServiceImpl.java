package com.geoscene.topo.im.service.impl;

import cn.hutool.core.util.ObjectUtil;
import cn.hutool.extra.spring.SpringUtil;
import cn.hutool.json.JSONUtil;
import com.geoscene.topo.admin.entity.UmsAdmin;
import com.geoscene.topo.admin.service.UmsAdminService;
import com.geoscene.topo.common.core.utils.RedisUtils;
import com.geoscene.topo.im.bo.IMReceiveMsg;
import com.geoscene.topo.im.bo.IMSocketSendMsg;
import com.geoscene.topo.im.config.NettyConfig;
import com.geoscene.topo.im.service.IMService;
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

@Service
public class IMServiceImpl implements IMService {

    private final RedisUtils redisService = new RedisUtils();

    private final RedisTemplate<String, Object> redisTemplate = SpringUtil.getBean("redisTemplate");

    @Autowired
    private UmsAdminService adminService;


    private final static String OFFLINE_MSG_LIST = "offline:msg:list";

    @Override
    public void personalMessage(Long userId, IMSocketSendMsg sendMsg) {
        Channel channel = NettyConfig.getChannel(userId.toString());

        if (ObjectUtil.isNull(channel)) {
            String redisKey = String.format("%s:%s", OFFLINE_MSG_LIST, userId);
            redisService.lPush(redisKey, JSONUtil.toJsonStr(sendMsg));
        } else {
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
        List<UmsAdmin> allUsers = adminService.list();
        for (UmsAdmin user : allUsers) {
            personalMessage(user.getId(), sendMsg);
        }
    }

    @Override
    public List<Object> getOfflineMessage(Long userId) {
        String redisKey = String.format("%s:%s", OFFLINE_MSG_LIST, userId);
        List<Object> execute = redisTemplate.execute(new SessionCallback<List<Object>>() {
            @Override
            public List<Object> execute(@NotNull RedisOperations operations) throws DataAccessException {
                operations.multi();
                operations.opsForList().range(redisKey, 0, -1);
                operations.delete(redisKey);
                return operations.exec();
            }
        });
        return (List<Object>) execute.get(0);
    }


    @Override
    public Boolean receive(IMReceiveMsg msg) {
        List<Long> receiverIds = msg.getReceiverIds();
        Assert.notEmpty(receiverIds, "消息接收人不能为空！");
        String message = String.format("您收到来自%s的消息:%s", msg.getSender(), msg.getData());
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
