package edu.tcd.library.im.server;

import cn.dev33.satoken.stp.StpUtil;
import cn.hutool.core.collection.CollUtil;
import cn.hutool.core.convert.Convert;
import cn.hutool.core.net.url.UrlBuilder;
import cn.hutool.core.util.ObjectUtil;
import cn.hutool.core.util.URLUtil;
import cn.hutool.json.JSONUtil;
import edu.tcd.library.admin.service.UmsAdminService;
import edu.tcd.library.admin.vo.CurrentUserVO;
import edu.tcd.library.im.bo.IMSocketSendMsg;
import edu.tcd.library.im.config.NettyConfig;
import edu.tcd.library.im.enums.IMTypeEnum;
import edu.tcd.library.im.service.IMService;
import io.netty.channel.*;
import io.netty.handler.codec.http.FullHttpRequest;
import io.netty.handler.codec.http.websocketx.TextWebSocketFrame;
import io.netty.handler.timeout.IdleState;
import io.netty.handler.timeout.IdleStateEvent;
import io.netty.util.AttributeKey;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Component;

import java.net.SocketAddress;
import java.util.List;
import java.util.Map;

import static edu.tcd.library.common.security.constants.TokenConstant.TOKEN_HEADER;
import static edu.tcd.library.common.security.constants.TokenConstant.TOKEN_PREFIX;

/**
 * IM Authentication and Connection Handler
 */
@Component
@Slf4j
@ChannelHandler.Sharable
public class IMAuthHandler extends SimpleChannelInboundHandler<FullHttpRequest> {

    private final IMService imService;

    private final UmsAdminService adminService;

    public IMAuthHandler(IMService imService, UmsAdminService adminService) {
        this.imService = imService;
        this.adminService = adminService;
    }

    /**
     * Heartbeat mechanism for monitoring connection idle states
     */
    @Override
    public void userEventTriggered(ChannelHandlerContext ctx, Object evt) throws Exception {
        if (evt instanceof IdleStateEvent event) {
            if (IdleState.WRITER_IDLE.equals(event.state())) {
                IMSocketSendMsg msg = new IMSocketSendMsg();
                msg.setType(IMTypeEnum.HEARTBEAT.getName());
                ctx.writeAndFlush(new TextWebSocketFrame(JSONUtil.toJsonStr(msg)))
                        .addListener(ChannelFutureListener.CLOSE_ON_FAILURE);
            }
            if (IdleState.READER_IDLE.equals(event.state())) {
                final String remoteAddress = parseChannelRemoteAddr(ctx.channel());
                log.warn("Netty server pipeline: IDLE exception [{}]", remoteAddress);
                NettyConfig.delChannel(ctx.channel());
            }
        }
        ctx.fireUserEventTriggered(evt);
    }

    @Override
    public void channelActive(ChannelHandlerContext ctx) throws Exception {
        super.channelActive(ctx);
    }

    @Override
    public void channelInactive(ChannelHandlerContext ctx) throws Exception {
        AttributeKey<String> attributeKey = AttributeKey.valueOf(TOKEN_HEADER);
        String token = ctx.channel().attr(attributeKey).get();
        CurrentUserVO vo = JSONUtil.toBean(token, CurrentUserVO.class);
        String userid = vo.getUserId().toString();

        // Remove channel from config if it matches the disconnected one
        if (ctx.channel() == NettyConfig.getChannel(userid)) {
            NettyConfig.delChannel(userid);
        }
    }

    @Override
    protected void channelRead0(ChannelHandlerContext ctx, FullHttpRequest request) throws Exception {
        try {
            String uri = request.uri();
            log.info("Formatting request connection URL... {}", uri);
            Map<CharSequence, CharSequence> queryMap = UrlBuilder.ofHttp(uri).getQuery().getQueryMap();
            String token = queryMap.get(TOKEN_HEADER).toString();
            String realToken = token.replace(TOKEN_PREFIX, "");

            // Resolve userId and fetch admin info
            Long userid = Convert.toLong(StpUtil.stpLogic.getLoginIdByToken(realToken));
            CurrentUserVO vo = adminService.getAdminInfoById(userid);
            ctx.channel().attr(AttributeKey.valueOf(TOKEN_HEADER)).setIfAbsent(JSONUtil.toJsonStr(vo));

            // Multi-device login: retain only the latest connection channel
            if (ObjectUtil.isNotNull(NettyConfig.getChannel(userid.toString()))) {
                NettyConfig.delChannel(userid.toString());
            }

            // Register channel to cache
            NettyConfig.addChannel(userid.toString(), ctx.channel());

            request.setUri(URLUtil.getPath(uri));
            ctx.fireChannelRead(request.retain());

            // Process offline messages
            List<Object> offlineMessage = imService.getOfflineMessage(userid);
            if (CollUtil.isNotEmpty(offlineMessage)) {
                for (Object msg : offlineMessage) {
                    IMSocketSendMsg sendMsg = JSONUtil.toBean(msg.toString(), IMSocketSendMsg.class);
                    imService.personalMessage(userid, sendMsg);
                }
            }

        } catch (Exception ex) {
            log.error("IM token validation failed: {}", ex.getMessage());
            throw new RuntimeException("IM token validation failed");
        }
    }

    @Override
    public void exceptionCaught(ChannelHandlerContext ctx, Throwable cause) throws Exception {
        super.exceptionCaught(ctx, cause);
    }

    /**
     * Parses the remote IP address of the Channel
     *
     * @param channel Netty channel
     * @return Formatted remote address string
     */
    private String parseChannelRemoteAddr(Channel channel) {
        if (null == channel) {
            return "";
        }
        SocketAddress remote = channel.remoteAddress();
        final String addr = remote != null ? remote.toString() : "";

        if (!addr.isEmpty()) {
            int index = addr.lastIndexOf("/");
            if (index >= 0) {
                return addr.substring(index + 1);
            }
            return addr;
        }
        return "";
    }
}