package com.geoscene.topo.common.core.exceptions;

import com.geoscene.topo.common.core.api.IErrorCode;
import lombok.Getter;

/**
 * 通用ErrorCode枚举异常
 */
@Getter
public class ErrorCodeException extends RuntimeException {

    private final IErrorCode code;

    public ErrorCodeException(IErrorCode code) {
        super(code.getMessage());
        this.code = code;
    }

}
