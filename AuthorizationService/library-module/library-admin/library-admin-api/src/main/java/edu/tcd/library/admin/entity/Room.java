package edu.tcd.library.admin.entity;

import com.fasterxml.jackson.annotation.JsonProperty;
import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Data;

import java.io.Serial;
import java.io.Serializable;

@Data
public class Room implements Serializable {

    @Serial
    private static final long serialVersionUID = 1L;

    @Schema(description = "Room GUID, Identify Key")
    @JsonProperty(value = "GUID")
    private String GUID;

    @Schema(description = "Unity Cloud Session Id")
    @JsonProperty(value = "SessionID")
    private String SessionID;

    @Schema(description = "Avatar URL")
    @JsonProperty(value = "JoinCode")
    private String JoinCode;

    @Schema(description = "Room Name")
    @JsonProperty(value = "RoomName")
    private String RoomName;

    @Schema(description = "Room Scene Name")
    @JsonProperty(value = "SceneName")
    private String SceneName;

    @Schema(description = "Room Max Players")
    @JsonProperty(value = "MaxPlayers")
    private Integer MaxPlayers;

    @Schema(description = "Room Status")
    @JsonProperty(value = "Status")
    private Integer Status;

    @Schema(description = "Room Last Update Timestamp")
    @JsonProperty(value = "LastUpdatedUtc")
    private Long LastUpdatedUtc;

    @Schema(description = "Room Endpoint")
    @JsonProperty(value = "Endpoint")
    private String Endpoint;
}

