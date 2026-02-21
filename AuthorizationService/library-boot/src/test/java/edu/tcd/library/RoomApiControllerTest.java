package edu.tcd.library;


import cn.hutool.json.JSONUtil;
import edu.tcd.library.admin.entity.Room;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.MediaType;
import org.springframework.test.web.servlet.MockMvc;

import java.util.ArrayList;
import java.util.List;

import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

/**
 * API Integration Test for Room Management.
 * Based on OpenAPI 3.0.1 Specification.
 */
@SpringBootTest
@AutoConfigureMockMvc
public class RoomApiControllerTest {

    @Autowired
    private MockMvc mockMvc;

    @Test
    @DisplayName("POST /room/addRooms - Verify bulk room addition")
    void shouldAddRoomsSuccessfully() throws Exception {
        // Prepare mock data according to OpenAPI 'Room' schema
        Room room = new Room();
        room.setGUID("test-guid-001");
        room.setRoomName("Imperial Library");
        room.setMaxPlayers(20); // Reverted to singular as per OpenAPI spec
        room.setStatus(1);

        List<Room> rooms = new ArrayList<>();
        rooms.add(room);

        mockMvc.perform(post("/room/addRooms")
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(JSONUtil.toJsonStr(rooms)))
                .andExpect(status().isOk())
                // Asserting CommonResultBoolean structure
                .andExpect(jsonPath("$.code").exists())
                .andExpect(jsonPath("$.data").isBoolean());
    }

    @Test
    @DisplayName("GET /room/listAll - Verify retrieval of all rooms")
    void shouldListAllRooms() throws Exception {
        mockMvc.perform(get("/room/listAll")
                        .accept(MediaType.ALL))
                .andExpect(status().isOk())
                // Asserting CommonResultListRoom structure
                .andExpect(jsonPath("$.code").value(200)) // Assuming 200 is success code
                .andExpect(jsonPath("$.data").isArray())
                .andExpect(jsonPath("$.message").isString());
    }
}
