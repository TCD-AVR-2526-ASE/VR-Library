package com.geoscene.topo.common.minio.domain;

import lombok.Builder;
import lombok.Data;

import java.io.Serializable;
import java.util.List;

@Data
@Builder
public class ItemObject implements Serializable {
    private Boolean isFolder;

    private String name;

    private String lastModified;

    private Long size;

    private List<ItemObject> children;
}
