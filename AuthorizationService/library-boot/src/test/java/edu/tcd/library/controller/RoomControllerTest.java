package edu.tcd.library.controller;

import edu.tcd.library.admin.entity.Room;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.springframework.http.MediaType;
import java.util.List;

import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.*;
import static org.springframework.test.web.servlet.result.MockMvcResultHandlers.print;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@DisplayName("Room Management Module Test")
class RoomControllerTest extends BaseIntegrationTest {

    @Test
    @DisplayName("Batch Add Rooms")
    void addRooms_Success() throws Exception {
        Room room = new Room();
        room.setGUID("123");
        room.setRoomName("Trinity Lab 1");
        room.setMaxPlayers(4);
        room.setSceneName("LabScene");

        List<Room> rooms = List.of(room);

        mockMvc.perform(withAuth(post("/room/addRooms"))
                        .contentType(MediaType.APPLICATION_JSON)
                        .content(objectMapper.writeValueAsString(rooms)))
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.code").value(200));
    }

    @Test
    @DisplayName("List All Rooms")
    void listAllRooms_Success() throws Exception {
        mockMvc.perform(withAuth(get("/room/listAll")))
                .andDo(print())
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.data").isArray());
    }
}