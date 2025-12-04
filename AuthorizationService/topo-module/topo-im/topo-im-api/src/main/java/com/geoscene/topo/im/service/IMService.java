package com.geoscene.topo.im.service;


import com.geoscene.topo.im.bo.IMReceiveMsg;
import com.geoscene.topo.im.bo.IMSocketSendMsg;

import java.util.List;

/**
 * IM系统消息
 */
public interface IMService {

    /**
     * 个人消息
     *
     * @param userId  个人id
     * @param sendMsg 消息体
     */
    void personalMessage(Long userId, IMSocketSendMsg sendMsg);


    /**
     * 群体消息
     *
     * @param group   群体id
     * @param sendMsg 消息体
     */
    void groupMessage(List<Long> group, IMSocketSendMsg sendMsg);

    /**
     * 全员消息
     *
     * @param sendMsg 消息体
     */
    void globalMessage(IMSocketSendMsg sendMsg);

    /**
     * 获取用户离线期间消息信息
     *
     * @param userId 用户id
     * @return 离线期间消息信息列表
     */
    List<Object> getOfflineMessage(Long userId);

    /**
     * 接受控制器传来的消息
     *
     * @param msg 消息内容
     * @return
     */
    Boolean receive(IMReceiveMsg msg);
}
