package edu.tcd.library.admin.controller;


import edu.tcd.library.admin.entity.Room;
import edu.tcd.library.admin.entity.UmsRole;
import edu.tcd.library.admin.service.RoomService;
import edu.tcd.library.common.core.api.CommonResult;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.tags.Tag;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;

@RestController
@Tag(name = "room management")
@RequestMapping("/room")
public class RoomController {

    private final RoomService roomService;

    public RoomController(RoomService roomService) {
        this.roomService = roomService;
    }

    @Operation(summary = "list all rooms")
    @RequestMapping(value = "/listAll", method = RequestMethod.GET)
    public CommonResult<List<Room>> listAll() {
        return CommonResult.success(roomService.listAll());
    }

    @Operation(summary = "add rooms")
    @RequestMapping(value = "/addRooms", method = RequestMethod.POST)
    public CommonResult<Boolean> addRooms(@RequestBody List<Room> rooms) {
        return CommonResult.judge(roomService.addRooms(rooms));
    }

}
