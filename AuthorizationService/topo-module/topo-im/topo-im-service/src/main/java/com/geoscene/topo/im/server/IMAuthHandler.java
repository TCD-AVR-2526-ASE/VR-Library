package com.geoscene.topo.im.server;

import cn.dev33.satoken.stp.StpUtil;
import cn.hutool.core.collection.CollUtil;
import cn.hutool.core.convert.Convert;
import cn.hutool.core.net.url.UrlBuilder;
import cn.hutool.core.util.ObjectUtil;
import cn.hutool.core.util.URLUtil;
import cn.hutool.json.JSONObject;
import cn.hutool.json.JSONUtil;
import com.geoscene.topo.admin.service.UmsAdminService;
import com.geoscene.topo.admin.vo.CurrentUserVO;
import com.geoscene.topo.im.bo.IMSocketSendMsg;
import com.geoscene.topo.im.config.NettyConfig;
import com.geoscene.topo.im.enums.IMTypeEnum;
import com.geoscene.topo.im.service.IMService;
import io.netty.channel.*;
import io.netty.handler.codec.http.FullHttpRequest;
import io.netty.handler.codec.http.websocketx.TextWebSocketFrame;
import io.netty.handler.timeout.IdleState;
import io.netty.handler.timeout.IdleStateEvent;
import io.netty.util.AttributeKey;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import java.net.SocketAddress;
import java.util.List;
import java.util.Map;

import static com.geoscene.topo.common.security.constants.TokenConstant.TOKEN_HEADER;
import static com.geoscene.topo.common.security.constants.TokenConstant.TOKEN_PREFIX;

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
     * 心跳保活机制
     *
     * @param ctx
     * @param evt
     * @throws Exception
     */
    @Override
    public void userEventTriggered(ChannelHandlerContext ctx, Object evt) throws Exception {
        if (evt instanceof IdleStateEvent event) {
            //写空闲发送心跳包
            if (IdleState.WRITER_IDLE.equals(event.state())) {
                IMSocketSendMsg msg = new IMSocketSendMsg();
                msg.setType(IMTypeEnum.HEARTBEAT.getName());
                ctx.writeAndFlush(new TextWebSocketFrame(JSONUtil.toJsonStr(msg)))
                        .addListener(ChannelFutureListener.CLOSE_ON_FAILURE);
            }
            //读空闲
            if (IdleState.READER_IDLE.equals(event.state())) {
                final String remoteAddress = parseChannelRemoteAddr(ctx.channel());
                log.warn("netty server pipeline: IDLE exception [{}]", remoteAddress);
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
        //需要判断是否为对应会话的channel，防止误删
        if (ctx.channel() == NettyConfig.getChannel(userid)) {
            NettyConfig.delChannel(userid);
        }
    }

    @Override
    protected void channelRead0(ChannelHandlerContext ctx, FullHttpRequest request) throws Exception {
        try {
            String uri = request.uri();
            log.info("格式化请求连接URL... {}", uri);
            Map<CharSequence, CharSequence> queryMap = UrlBuilder.ofHttp(uri).getQuery().getQueryMap();
            String token = queryMap.get(TOKEN_HEADER).toString();
            String realToken = token.replace(TOKEN_PREFIX, "");

            Long userid = Convert.toLong(StpUtil.stpLogic.getLoginIdByToken(realToken));
            CurrentUserVO vo = adminService.getAdminInfoById(userid);
            ctx.channel().attr(AttributeKey.valueOf(TOKEN_HEADER)).setIfAbsent(JSONUtil.toJsonStr(vo));

            //用户多端登录只保留最后的连接channel
            if (ObjectUtil.isNotNull(NettyConfig.getChannel(userid.toString()))) {
                NettyConfig.delChannel(userid.toString());
            }

            //注册channel缓存
            NettyConfig.addChannel(userid.toString(), ctx.channel());

            request.setUri(URLUtil.getPath(uri));
            ctx.fireChannelRead(request.retain());

            //离线消息
            List<Object> offlineMessage = imService.getOfflineMessage(userid);
            if (CollUtil.isNotEmpty(offlineMessage)) {
                for (Object msg : offlineMessage) {
                    IMSocketSendMsg sendMsg = JSONUtil.toBean(msg.toString(), IMSocketSendMsg.class);
                    imService.personalMessage(userid, sendMsg);
                }
            }

        } catch (Exception ex) {
            log.error("im token校验失败:{}", ex.getMessage());
            throw new RuntimeException("im token校验失败");
        }
    }

    @Override
    public void exceptionCaught(ChannelHandlerContext ctx, Throwable cause) throws Exception {
        super.exceptionCaught(ctx, cause);
    }

    /**
     * 获取Channel的远程IP地址
     *
     * @param channel
     * @return
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
