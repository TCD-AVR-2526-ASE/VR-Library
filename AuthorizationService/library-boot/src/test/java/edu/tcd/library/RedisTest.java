package edu.tcd.library;

import cn.hutool.extra.spring.SpringUtil;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.context.ApplicationContext;
import org.springframework.data.redis.core.RedisTemplate;

import java.util.concurrent.TimeUnit;

@SpringBootTest
public class RedisTest {

    @Autowired
    private ApplicationContext context;

    @Test
    void testAddKey() {
        RedisTemplate template = (RedisTemplate) context.getBean("redisTemplate");
        template.opsForValue().set("key", "value");
    }

    @Test
    void testAddCache() {
        RedisTemplate template = (RedisTemplate) context.getBean("redisTemplate");
        template.opsForValue().set("cache", "111", 10, TimeUnit.SECONDS);
    }

}
