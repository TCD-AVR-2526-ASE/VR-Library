package edu.tcd.library.im.server;

import edu.tcd.library.im.config.NettyConfig;
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
import org.springframework.stereotype.Component;

import java.util.concurrent.TimeUnit;

/**
 * Netty IM Server initialization and configuration
 */
@Slf4j
@Component
public class IMServer {

    private final NettyConfig config;

    private final IMAuthHandler tokenHandler;

    private final NioEventLoopGroup bossGroup = new NioEventLoopGroup(1);
    private final NioEventLoopGroup workerGroup = new NioEventLoopGroup();

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

                                    // Heartbeat mechanism: readerIdle, writerIdle, allIdle
                                    pipeline.addLast(new IdleStateHandler(10, 5, 15, TimeUnit.SECONDS));

                                    // Parse WebSocket token and handle authentication
                                    pipeline.addLast(tokenHandler);

                                    // Add WebSocket server protocol handler
                                    pipeline.addLast(new WebSocketServerProtocolHandler("/im"));

                                    // Add custom WebSocket business logic handler
                                    pipeline.addLast(new IMChatHandler());
                                }
                            }
                    )
                    .option(ChannelOption.SO_BACKLOG, 128)
                    .childOption(ChannelOption.SO_KEEPALIVE, true);

            ChannelFuture future = bootstrap.bind(config.getPort()).sync();
            log.info("IM Server started on port: {}", config.getPort());

            // Wait for the server socket to close for a graceful shutdown
            future.channel().closeFuture().sync();

        } catch (Exception ex) {
            log.error("IM socket error: {}", ex.getMessage());
        } finally {
            bossGroup.shutdownGracefully();
            workerGroup.shutdownGracefully();
        }
    }

    @PreDestroy
    public void destroy() {
        bossGroup.shutdownGracefully();
        workerGroup.shutdownGracefully();
        log.info("IM Server resources released.");
    }
}