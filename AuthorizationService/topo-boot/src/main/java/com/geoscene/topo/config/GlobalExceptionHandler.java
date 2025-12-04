package com.geoscene.topo.config;

import cn.dev33.satoken.exception.NotLoginException;
import com.geoscene.topo.common.core.api.CommonResult;
import com.geoscene.topo.common.core.exceptions.ErrorCodeException;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.converter.HttpMessageConversionException;
import org.springframework.validation.BindException;
import org.springframework.validation.BindingResult;
import org.springframework.validation.FieldError;
import org.springframework.web.bind.annotation.ControllerAdvice;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.ResponseBody;
import org.springframework.web.multipart.support.MissingServletRequestPartException;

import static com.geoscene.topo.common.core.constants.MessageConstant.NOT_LOGIN;

@Slf4j
@ControllerAdvice
public class GlobalExceptionHandler {


    /**
     * 参数校验异常
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
     * request part校验
     */
    @ResponseBody
    @ExceptionHandler(value = MissingServletRequestPartException.class)
    public CommonResult<String> handleMissingServletRequestPartException(MissingServletRequestPartException ex) {
        log.error(ex.getMessage());
        String requestPartName = ex.getRequestPartName();
        return CommonResult.validateFailed("缺少必填参数部分:" + requestPartName);
    }


    /**
     * 内容转换异常
     */
    @ResponseBody
    @ExceptionHandler(value = HttpMessageConversionException.class)
    public CommonResult<String> handleHttpMessageConversionException(HttpMessageConversionException ex) {
        log.error(ex.getMessage());
        String message = "内容转换异常，请检查输入格式！";
        return CommonResult.validateFailed(message);
    }

    /**
     * 通用ErrorCode枚举异常捕获
     */
    @ResponseBody
    @ExceptionHandler(value = ErrorCodeException.class)
    public CommonResult<String> handleErrorCodeException(ErrorCodeException ex) {
        log.error(ex.getMessage());
        return CommonResult.failed(ex.getCode());
    }

    /**
     * 未登录异常
     */
    @ResponseBody
    @ExceptionHandler(value = NotLoginException.class)
    public CommonResult<String> handleNotLoginException(NotLoginException ex) {
        log.error(ex.getMessage());
        return CommonResult.validateFailed(NOT_LOGIN);
    }


    /**
     * 运行时异常捕获
     */
    @ResponseBody
    @ExceptionHandler(value = RuntimeException.class)
    public CommonResult<String> handleRuntimeException(RuntimeException ex) {
        log.error(ex.getMessage());
        return CommonResult.failed(ex.getMessage());
    }


}
