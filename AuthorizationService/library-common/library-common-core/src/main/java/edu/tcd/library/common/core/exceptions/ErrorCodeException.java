package edu.tcd.library.common.core.exceptions;

import edu.tcd.library.common.core.api.IErrorCode;
import lombok.Getter;

@Getter
public class ErrorCodeException extends RuntimeException {

    private final IErrorCode code;

    public ErrorCodeException(IErrorCode code) {
        super(code.getMessage());
        this.code = code;
    }

}
