package edu.tcd.library.im.enums;

import lombok.Getter;

/**
 * Enumeration of message types for the IM subsystem
 */
@Getter
public enum IMTypeEnum {

    HEARTBEAT("heartbeat", "Heartbeat"),

    KICKOFF("kickoff", "Kickoff");

    private final String name;

    private final String description;

    IMTypeEnum(String name, String description) {
        this.name = name;
        this.description = description;
    }

}