package edu.tcd.library.im.server;

import cn.hutool.json.JSONObject;
import cn.hutool.json.JSONUtil;
import edu.tcd.library.im.bo.IMSocketSendMsg;
import edu.tcd.library.im.enums.IMTypeEnum;
import edu.tcd.library.im.service.IMService;
import io.netty.channel.ChannelHandlerContext;
import io.netty.channel.SimpleChannelInboundHandler;
import io.netty.handler.codec.http.websocketx.TextWebSocketFrame;
import io.netty.util.AttributeKey;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import static edu.tcd.library.common.security.constants.TokenConstant.TOKEN_HEADER;

/**
 * Handler for processing IM chat messages and heartbeat signals.
 */
@Component
@Slf4j
public class IMChatHandler extends SimpleChannelInboundHandler<Object> {

    private final IMService imService;

    public IMChatHandler(IMService imService) {
        this.imService = imService;
    }

    @Override
    public void exceptionCaught(ChannelHandlerContext ctx, Throwable cause) {
        // Close the connection when an exception occurs
        log.error("IM handler error: {}", cause.getMessage());
        ctx.close();
    }

    @Override
    protected void channelRead0(ChannelHandlerContext ctx, Object msg) throws Exception {
        // Verify user token
        AttributeKey<String> attributeKey = AttributeKey.valueOf(TOKEN_HEADER);

        // Retrieve user token from the channel attributes
        String token = ctx.channel().attr(attributeKey).get();
        log.info("IM receive token: {}", token);

        // Note: Currently, only the heartbeat is monitored here
        if (msg instanceof TextWebSocketFrame) {
            TextWebSocketFrame textFrame = (TextWebSocketFrame) msg;
            log.info("IM receive text: {}", textFrame.text());
            JSONObject entries = JSONUtil.parseObj(textFrame.text());
            String type = entries.getStr("type");
            if (IMTypeEnum.KICKOFF.getName().equals(type)) {
                Long userId = entries.getLong("userId");
                IMSocketSendMsg sendMsg = new IMSocketSendMsg();
                sendMsg.setType(IMTypeEnum.KICKOFF.getName());
                sendMsg.setData("You were kicked from the room.");
                imService.personalMessage(userId, sendMsg);
            }

        }
    }
}