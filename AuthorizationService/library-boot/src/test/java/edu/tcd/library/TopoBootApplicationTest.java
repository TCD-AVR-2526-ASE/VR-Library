package edu.tcd.library;

import edu.tcd.library.common.minio.service.MinioService;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;

@SpringBootTest
class TopoBootApplicationTest {

    @Autowired
    private MinioService minioService;

    @Test
    void testGetBucket() {
        Boolean test = minioService.bucketExists("test");
        System.out.println(test);
    }

    @Test
    void testSupplement() {

    }


    static class User {
        private String name;

        private int age;

        private String gender;

        public String getName() {
            return name;
        }

        public void setName(String name) {
        }
    }
}