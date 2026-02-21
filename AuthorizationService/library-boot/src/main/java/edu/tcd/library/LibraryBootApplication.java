package edu.tcd.library;

import edu.tcd.library.im.server.IMServer;
import org.mybatis.spring.annotation.MapperScan;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.ConfigurableApplicationContext;

@MapperScan(basePackages = "edu.tcd.library.**.mapper")
@SpringBootApplication(scanBasePackages = "edu.tcd.library")
public class LibraryBootApplication {

    public static void main(String[] args) {
        ConfigurableApplicationContext context = SpringApplication.run(LibraryBootApplication.class, args);
        IMServer imServer = context.getBean(IMServer.class);
        imServer.run();
    }
}
