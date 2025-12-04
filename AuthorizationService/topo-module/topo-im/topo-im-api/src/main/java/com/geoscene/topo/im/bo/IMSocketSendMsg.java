package com.geoscene.topo.im.bo;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.io.Serializable;

/**
 * socket发送到客户端消息内容
 */
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class IMSocketSendMsg implements Serializable {

    private static final long serialVersionUID = 1L;

    private String type;

    private String data;

    private String identity;
}
