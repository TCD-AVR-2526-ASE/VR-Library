package edu.tcd.library.im.bo;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.io.Serializable;

/**
 * Socket message payload sent to the client
 */
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class IMSocketSendMsg implements Serializable {

    private static final long serialVersionUID = 1L;

    /**
     * Message type (corresponds to IMTypeEnum)
     */
    private String type;

    /**
     * Message content or data payload
     */
    private String data;

    /**
     * Unique identifier for the message or sender identity
     */
    private String identity;
}