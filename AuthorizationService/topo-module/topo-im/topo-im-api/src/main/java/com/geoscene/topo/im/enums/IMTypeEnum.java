package com.geoscene.topo.im.enums;

import lombok.Getter;

/**
 * IM子系统消息类型枚举
 */
@Getter
public enum IMTypeEnum {

    HEARTBEAT("heartbeat", "心跳"),

    TASK("task", "任务"),

    APPLY("apply", "申请"),

    DOWNLOAD("download", "下载申请"),

    STORE_PUBLIC("storePublic", "公有库审批"),

    MESSAGE("message", "消息"),

    FILE("file", "文件"),

    READ("read", "消息已读");

    private final String name;

    private final String description;

    IMTypeEnum(String name, String description) {
        this.name = name;
        this.description = description;
    }

}
