package edu.tcd.library.config;

import cn.dev33.satoken.exception.NotLoginException;
import edu.tcd.library.common.core.api.CommonResult;
import edu.tcd.library.common.core.exceptions.ErrorCodeException;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.converter.HttpMessageConversionException;
import org.springframework.validation.BindException;
import org.springframework.validation.BindingResult;
import org.springframework.validation.FieldError;
import org.springframework.web.bind.annotation.ControllerAdvice;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.ResponseBody;
import org.springframework.web.multipart.support.MissingServletRequestPartException;

import static edu.tcd.library.common.core.constants.MessageConstant.NOT_LOGIN;

@Slf4j
@ControllerAdvice
public class GlobalExceptionHandler {


    /**
     * Parameter validation exception
     */
    @ResponseBody
    @ExceptionHandler(value = BindException.class)
    public CommonResult<String> handleBindException(BindException ex) {
        log.error(ex.getMessage());
        BindingResult bindingResult = ex.getBindingResult();
        String message = null;
        if (bindingResult.hasErrors()) {
            FieldError fieldError = bindingResult.getFieldError();
            if (fieldError != null) {
                message = fieldError.getField() + ":" + fieldError.getDefaultMessage();
            }
        }
        return CommonResult.validateFailed(message);
    }


    /**
     * Request part validation
     */
    @ResponseBody
    @ExceptionHandler(value = MissingServletRequestPartException.class)
    public CommonResult<String> handleMissingServletRequestPartException(MissingServletRequestPartException ex) {
        log.error(ex.getMessage());
        String requestPartName = ex.getRequestPartName();
        return CommonResult.validateFailed("Missing required request part: " + requestPartName);
    }


    /**
     * Content conversion exception
     */
    @ResponseBody
    @ExceptionHandler(value = HttpMessageConversionException.class)
    public CommonResult<String> handleHttpMessageConversionException(HttpMessageConversionException ex) {
        log.error(ex.getMessage());
        String message = "Content conversion error, please check input format!";
        return CommonResult.validateFailed(message);
    }

    /**
     * General ErrorCode enum exception handling
     */
    @ResponseBody
    @ExceptionHandler(value = ErrorCodeException.class)
    public CommonResult<String> handleErrorCodeException(ErrorCodeException ex) {
        log.error(ex.getMessage());
        return CommonResult.failed(ex.getCode());
    }

    /**
     * Not logged in exception
     */
    @ResponseBody
    @ExceptionHandler(value = NotLoginException.class)
    public CommonResult<String> handleNotLoginException(NotLoginException ex) {
        log.error(ex.getMessage());
        return CommonResult.validateFailed(NOT_LOGIN);
    }


    /**
     * Runtime exception handling
     */
    @ResponseBody
    @ExceptionHandler(value = RuntimeException.class)
    public CommonResult<String> handleRuntimeException(RuntimeException ex) {
        log.error(ex.getMessage());
        return CommonResult.failed(ex.getMessage());
    }


}