package com.geoscene.topo.im.server;

import io.netty.channel.ChannelHandlerContext;
import io.netty.channel.SimpleChannelInboundHandler;
import io.netty.handler.codec.http.websocketx.TextWebSocketFrame;
import io.netty.util.AttributeKey;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Component;

import static com.geoscene.topo.common.security.constants.TokenConstant.TOKEN_HEADER;


@Component
@Slf4j
public class IMChatHandler extends SimpleChannelInboundHandler<Object> {

    @Override
    public void exceptionCaught(ChannelHandlerContext ctx, Throwable cause) {
        // 当出现异常就关闭连接
        log.error("IM handler error:{}", cause.getMessage());
        ctx.close();
    }

    @Override
    protected void channelRead0(ChannelHandlerContext ctx, Object msg) throws Exception {
        //检查用户token
        AttributeKey<String> attributeKey = AttributeKey.valueOf(TOKEN_HEADER);
        //从通道中获取用户token
        String token = ctx.channel().attr(attributeKey).get();
        System.out.println(token);

        //这里目前只监测了心跳
        //TODO 系统聊天功能实现
        if (msg instanceof TextWebSocketFrame) {
            TextWebSocketFrame textFrame = (TextWebSocketFrame) msg;
            System.out.println(textFrame.text());
        }
    }
}