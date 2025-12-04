package com.geoscene.topo;

import com.geoscene.topo.im.server.IMServer;
import org.mybatis.spring.annotation.MapperScan;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.ConfigurableApplicationContext;

@MapperScan(basePackages = "com.geoscene.topo.**.mapper")
@SpringBootApplication(scanBasePackages = "com.geoscene.topo")
public class TopoBootApplication {

    public static void main(String[] args) {
        ConfigurableApplicationContext context = SpringApplication.run(TopoBootApplication.class, args);
        IMServer imServer = context.getBean(IMServer.class);
        imServer.run();
    }
}
