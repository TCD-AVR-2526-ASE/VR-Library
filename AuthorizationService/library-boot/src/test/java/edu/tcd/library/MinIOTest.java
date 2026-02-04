package edu.tcd.library;


import edu.tcd.library.common.minio.service.MinioService;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.InputStream;

@SpringBootTest
public class MinIOTest {

    @Autowired
    private MinioService minioService;


    @Test
    void testCreateBucket() throws Exception {
        minioService.createBucket("test");
    }

    @Test
    void testUploadFile() throws FileNotFoundException {
        File file = new File("/Users/dodge/programs/avr/ase/VR-Library/AuthorizationService/library-boot/pom.xml");
        long length = file.length();
        InputStream inputStream = new FileInputStream(file);
        minioService.upload(inputStream, "test", "pom.xml", length);
    }

    @Test
    void testRemoveFile(){
        minioService.remove("2026-02-01/6e6664f0-0489-48f9-ac0d-688315b8af6a.xml","test");
    }

    @Test
    void testDeleteBucket() throws FileNotFoundException {
        minioService.removeBucket("test");
    }
}
