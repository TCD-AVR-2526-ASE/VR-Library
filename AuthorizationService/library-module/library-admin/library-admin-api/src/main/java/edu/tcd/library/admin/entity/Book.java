package edu.tcd.library.admin.entity;

import cn.hutool.json.JSONArray;
import cn.hutool.json.JSONObject;
import com.baomidou.mybatisplus.annotation.TableField;
import com.baomidou.mybatisplus.annotation.TableName;
import com.baomidou.mybatisplus.extension.handlers.JacksonTypeHandler;
import edu.tcd.library.common.mybatis.handler.JsonTypeHandler;
import io.swagger.v3.oas.annotations.media.Schema;
import lombok.Data;

import java.io.Serial;
import java.io.Serializable;

@Data
@TableName(value = "book", autoResultMap = true)
public class Book implements Serializable {

    @Serial
    private static final long serialVersionUID = 1L;

    private Integer id;

    @Schema(description = "Book Title")
    private String title;

    @TableField(typeHandler = JsonTypeHandler.class)
    @Schema(description = "Book Authors")
    private String authors;

    @TableField(typeHandler = JsonTypeHandler.class)
    @Schema(description = "Book Subjects")
    private String subjects;

    @TableField(typeHandler = JsonTypeHandler.class)
    @Schema(description = "Book Shelve And Categories")
    private String bookshelves;
}
