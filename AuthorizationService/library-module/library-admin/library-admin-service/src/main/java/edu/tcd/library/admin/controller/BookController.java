package edu.tcd.library.admin.controller;


import cn.hutool.core.util.StrUtil;
import com.baomidou.mybatisplus.core.conditions.query.QueryWrapper;
import com.baomidou.mybatisplus.extension.plugins.pagination.Page;
import edu.tcd.library.admin.entity.Book;
import edu.tcd.library.admin.service.BookService;
import edu.tcd.library.common.core.api.CommonPage;
import edu.tcd.library.common.core.api.CommonResult;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.tags.Tag;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

@RestController
@Tag(name = "book management")
@RequestMapping("/book")
public class BookController {

    private final BookService bookService;

    public BookController(BookService bookService) {
        this.bookService = bookService;
    }

    @Operation(summary = "search books")
    @RequestMapping(value = "/page", method = RequestMethod.GET)
    public CommonResult<CommonPage<Book>> page(@RequestParam(value = "keyword", required = false) String keyword,
                                               @RequestParam(value = "author", required = false) String author,
                                               @RequestParam(value = "subject", required = false) String subject,
                                               @RequestParam(value = "bookshelve", required = false) String bookshelve,
                                               @RequestParam(value = "pageSize", defaultValue = "10") Integer pageSize,
                                               @RequestParam(value = "pageNum", defaultValue = "1") Integer pageNum) {
        QueryWrapper<Book> queryWrapper = new QueryWrapper<>();
        queryWrapper.like(StrUtil.isNotEmpty(keyword), "title", keyword);
        queryWrapper.apply(StrUtil.isNotEmpty(subject), " subjects::text ILIKE '%" + subject + "%'");
        queryWrapper.apply(StrUtil.isNotEmpty(bookshelve), " bookshelves::text ILIKE '%" + bookshelve + "%'");
        queryWrapper.apply(StrUtil.isNotEmpty(author), "EXISTS (SELECT 1 FROM jsonb_array_elements(authors) AS author WHERE author->>'name' ILIKE '%" + author + "%')");
        queryWrapper.orderByAsc("id");

        Page<Book> page = new Page<>(pageNum, pageSize);
        return CommonResult.success(CommonPage.restPage(bookService.page(page, queryWrapper)));
    }

}
