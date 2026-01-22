package com.geoscene.topo.im.server;

import com.geoscene.topo.im.config.NettyConfig;
import io.netty.bootstrap.ServerBootstrap;
import io.netty.channel.ChannelFuture;
import io.netty.channel.ChannelInitializer;
import io.netty.channel.ChannelOption;
import io.netty.channel.ChannelPipeline;
import io.netty.channel.nio.NioEventLoopGroup;
import io.netty.channel.socket.SocketChannel;
import io.netty.channel.socket.nio.NioServerSocketChannel;
import io.netty.handler.codec.http.HttpObjectAggregator;
import io.netty.handler.codec.http.HttpServerCodec;
import io.netty.handler.codec.http.websocketx.WebSocketServerProtocolHandler;
import io.netty.handler.timeout.IdleStateHandler;
import jakarta.annotation.PreDestroy;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import java.util.concurrent.TimeUnit;

@Slf4j
@Component
public class IMServer {

    private final NettyConfig config;

    private final IMAuthHandler tokenHandler;

    NioEventLoopGroup bossGroup = new NioEventLoopGroup(1);
    NioEventLoopGroup workerGroup = new NioEventLoopGroup();

    public IMServer(NettyConfig config, IMAuthHandler tokenHandler) {
        this.config = config;
        this.tokenHandler = tokenHandler;
    }

    public void run() {
        ServerBootstrap serverBootstrap = new ServerBootstrap();
        try {
            ServerBootstrap bootstrap = serverBootstrap.group(bossGroup, workerGroup)
                    .channel(NioServerSocketChannel.class)
                    .childHandler(
                            new ChannelInitializer<SocketChannel>() {
                                @Override
                                public void initChannel(SocketChannel ch) throws Exception {
                                    ChannelPipeline pipeline = ch.pipeline();
                                    // Add HTTP server codec
                                    pipeline.addLast(new HttpServerCodec());
                                    // Add HTTP object aggregator
                                    pipeline.addLast(new HttpObjectAggregator(65536));
                                    // 心跳检查
                                    pipeline.addLast(new IdleStateHandler(10, 5, 15, TimeUnit.SECONDS));
                                    //解析ws token
                                    pipeline.addLast(tokenHandler);
                                    // Add WebSocket server protocol handler
                                    pipeline.addLast(new WebSocketServerProtocolHandler("/im"));
                                    // Add custom WebSocket handler
                                    pipeline.addLast(new IMChatHandler());
                                }
                            }
                    )
                    .option(ChannelOption.SO_BACKLOG, 128)
                    .childOption(ChannelOption.SO_KEEPALIVE, true);

            ChannelFuture future = bootstrap.bind(config.getPort()).sync();

            //优雅关闭
            future.channel().closeFuture().sync();

        } catch (Exception ex) {
            ex.printStackTrace();
            log.error("im socket错误:{}", ex.getMessage());
        } finally {
            bossGroup.shutdownGracefully();
            workerGroup.shutdownGracefully();
        }
    }

    @PreDestroy
    public void destroy() throws Exception {
        bossGroup.shutdownGracefully();
        workerGroup.shutdownGracefully();
    }


}
