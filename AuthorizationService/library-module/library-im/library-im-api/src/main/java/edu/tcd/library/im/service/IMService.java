package edu.tcd.library.im.service;

import edu.tcd.library.im.bo.IMReceiveMsg;
import edu.tcd.library.im.bo.IMSocketSendMsg;

import java.util.List;

/**
 * IM System Messaging Service Interface
 */
public interface IMService {

    /**
     * Sends a personal message to a specific user
     *
     * @param userId  The unique ID of the recipient
     * @param sendMsg The message payload to be sent
     */
    void personalMessage(Long userId, IMSocketSendMsg sendMsg);

    /**
     * Sends a message to a specific group of users
     *
     * @param group   List of recipient user IDs
     * @param sendMsg The message payload to be sent
     */
    void groupMessage(List<Long> group, IMSocketSendMsg sendMsg);

    /**
     * Broadcasts a message to all users in the system
     *
     * @param sendMsg The message payload to be sent
     */
    void globalMessage(IMSocketSendMsg sendMsg);

    /**
     * Retrieves messages that were queued while the user was offline
     *
     * @param userId The unique ID of the user
     * @return A list of offline message objects
     */
    List<Object> getOfflineMessage(Long userId);

    /**
     * Processes an incoming message received from the controller layer
     *
     * @param msg The incoming message content
     * @return Boolean indicating processing success
     */
    Boolean receive(IMReceiveMsg msg);
}