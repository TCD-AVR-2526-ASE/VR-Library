package edu.tcd.library.im.server;

import io.netty.channel.ChannelHandlerContext;
import io.netty.channel.SimpleChannelInboundHandler;
import io.netty.handler.codec.http.websocketx.TextWebSocketFrame;
import io.netty.util.AttributeKey;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Component;

import static edu.tcd.library.common.security.constants.TokenConstant.TOKEN_HEADER;

/**
 * Handler for processing IM chat messages and heartbeat signals.
 */
@Component
@Slf4j
public class IMChatHandler extends SimpleChannelInboundHandler<Object> {

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
        System.out.println(token);

        // Note: Currently, only the heartbeat is monitored here
        // TODO: Implement full system chat functionality
        if (msg instanceof TextWebSocketFrame) {
            TextWebSocketFrame textFrame = (TextWebSocketFrame) msg;
            System.out.println(textFrame.text());
        }
    }
}