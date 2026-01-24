package edu.tcd.library.im.bo;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.io.Serial;
import java.io.Serializable;
import java.util.Date;
import java.util.List;

/**
 * Message received by the IM module
 */
@Data
@AllArgsConstructor
@NoArgsConstructor
@Builder
public class IMReceiveMsg implements Serializable {

    @Serial
    private static final long serialVersionUID = 1L;

    /**
     * Timestamp when the message was sent
     */
    private Date sendTime;

    /**
     * Name or identifier of the sender
     */
    private String sender;

    /**
     * Unique ID of the sender
     */
    private Long senderId;

    /**
     * List of recipient user IDs
     */
    private List<Long> receiverIds;

    /**
     * Message type (corresponds to IMTypeEnum)
     */
    private String type;

    /**
     * Message content or data payload
     */
    private String data;

    /**
     * Unique identifier for the message or session identity
     */
    private String identity;
}