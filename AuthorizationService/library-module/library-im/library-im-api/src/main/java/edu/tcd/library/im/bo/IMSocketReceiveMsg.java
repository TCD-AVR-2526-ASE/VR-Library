package edu.tcd.library.im.bo;

import lombok.Data;

import java.io.Serializable;

/**
 * Socket message payload received from the client
 */
@Data
public class IMSocketReceiveMsg implements Serializable {

    private static final long serialVersionUID = 1L;

    /**
     * Message type
     */
    private String type;

    /**
     * Message content or data payload
     */
    private String data;

    /**
     * Unique identifier or identity reference
     */
    private String identity;
}