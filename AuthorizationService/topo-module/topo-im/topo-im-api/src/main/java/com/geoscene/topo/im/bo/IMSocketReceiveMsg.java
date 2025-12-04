package com.geoscene.topo.im.bo;

import lombok.Data;

import java.io.Serializable;

/**
 * socket从客户端接收到的消息内容
 */
@Data
public class IMSocketReceiveMsg implements Serializable {

    private static final long serialVersionUID = 1L;

    private String type;

    private String data;

    private String identity;
}
