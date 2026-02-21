package edu.tcd.library.common.minio.enums;

public enum BucketEnums {


    ADMIN("admin", "用户数据桶"),

    EXAM("exam", "考试数据桶"),

    TMP("tmp", "临时文件数据桶"),

    COMMUNITY("community", "社区数据桶"),

    MAP("map", "GIS数据桶");


    private String name;

    private String desc;


    BucketEnums(String name, String desc) {
        this.name = name;
        this.desc = desc;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public String getDesc() {
        return desc;
    }

    public void setDesc(String desc) {
        this.desc = desc;
    }
}
