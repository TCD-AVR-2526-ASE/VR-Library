package edu.tcd.library.admin.entity;

import lombok.Data;
import lombok.EqualsAndHashCode;

import java.io.Serial;
import java.io.Serializable;

/**
 * Extended System Administrator Entity including department information
 */
@Data
@EqualsAndHashCode(callSuper = true)
public class UmsAdminExtend extends UmsAdmin implements Serializable {

    @Serial
    private static final long serialVersionUID = 1L;

}