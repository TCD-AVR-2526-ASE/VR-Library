package edu.tcd.library.im.config;

import io.netty.channel.Channel;
import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.context.annotation.Configuration;

import java.net.Inet4Address;
import java.net.InetAddress;
import java.net.UnknownHostException;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.locks.ReentrantReadWriteLock;

@Configuration
@ConfigurationProperties(prefix = "netty")
public class NettyConfig {

    private String serviceName;

    private Integer port;

    private final String ip;

    private Integer heartbeatTime;

    private static final Map<String, Channel> channelMap = new ConcurrentHashMap<>();

    private static ReentrantReadWriteLock lock = new ReentrantReadWriteLock(true);


    public NettyConfig() throws UnknownHostException {
        InetAddress address = Inet4Address.getLocalHost();
        this.ip = address.getHostAddress();
    }

    public static void addChannel(String key, Channel channel) {
        try {
            lock.writeLock().lock();
            channelMap.put(key, channel);
        } finally {
            lock.writeLock().unlock();
        }
    }

    public static Channel getChannel(String key) {
        try {
            lock.readLock().lock();
            return channelMap.getOrDefault(key, null);
        } finally {
            lock.readLock().unlock();
        }
    }

    public static void delChannel(String key) {
        try {
            lock.writeLock().lock();
            Channel channel = channelMap.get(key);
            if (channel.isActive()) {
                channel.close();
            }
            channelMap.remove(key);
        } finally {
            lock.writeLock().unlock();
        }
    }

    public static void delChannel(Channel channel) {
        try {
            lock.writeLock().lock();
            channelMap.entrySet().removeIf(
                    entry -> entry.getValue().equals(channel));
            if (channel.isActive()) {
                channel.close();
            }
        } finally {
            lock.writeLock().unlock();
        }
    }


    public String getServiceName() {
        return serviceName;
    }

    public void setServiceName(String serviceName) {
        this.serviceName = serviceName;
    }

    public Integer getPort() {
        return port;
    }

    public void setPort(Integer port) {
        this.port = port;
    }

    public String getIp() {
        return ip;
    }

    public Integer getHeartbeatTime() {
        return heartbeatTime;
    }

    public void setHeartbeatTime(Integer heartbeatTime) {
        this.heartbeatTime = heartbeatTime;
    }
}
