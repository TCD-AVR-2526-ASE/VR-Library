package edu.tcd.library.im.enums;

import lombok.Getter;

/**
 * Enumeration of message types for the IM subsystem
 */
@Getter
public enum IMTypeEnum {

    HEARTBEAT("heartbeat", "Heartbeat"),

    TASK("task", "Task"),

    APPLY("apply", "Application"),

    DOWNLOAD("download", "Download Request"),

    STORE_PUBLIC("storePublic", "Public Repository Approval"),

    MESSAGE("message", "Message"),

    FILE("file", "File"),

    READ("read", "Message Read Receipt");

    private final String name;

    private final String description;

    IMTypeEnum(String name, String description) {
        this.name = name;
        this.description = description;
    }

}