package com.geoscene.topo.im.bo;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.io.Serial;
import java.io.Serializable;
import java.util.Date;
import java.util.List;

/**
 * im模块接受的消息
 */
@Data
@AllArgsConstructor
@NoArgsConstructor
@Builder
public class IMReceiveMsg implements Serializable {

    @Serial
    private static final long serialVersionUID = 1L;

    private Date sendTime;

    private String sender;

    private Long senderId;

    private List<Long> receiverIds;

    private String type;

    private String data;

    private String identity;
}
