package edu.tcd.library;


import edu.tcd.library.admin.service.UmsRoleService;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;

@SpringBootTest
public class PostgresTest {

    @Autowired
    private UmsRoleService roleService;

}
