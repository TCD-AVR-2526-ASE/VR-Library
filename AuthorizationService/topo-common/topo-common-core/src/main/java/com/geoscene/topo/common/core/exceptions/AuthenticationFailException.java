package com.geoscene.topo.common.core.exceptions;

/**
 * 授权异常
 */
public class AuthenticationFailException extends RuntimeException {

    public AuthenticationFailException(String message) {
        super(message);
    }
}
