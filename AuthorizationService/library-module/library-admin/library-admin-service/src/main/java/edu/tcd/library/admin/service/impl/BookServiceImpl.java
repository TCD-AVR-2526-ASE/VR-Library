package edu.tcd.library.admin.service.impl;

import com.baomidou.mybatisplus.extension.service.impl.ServiceImpl;
import edu.tcd.library.admin.entity.Book;
import edu.tcd.library.admin.mapper.BookMapper;
import edu.tcd.library.admin.service.BookService;
import org.springframework.stereotype.Service;

@Service
public class BookServiceImpl extends ServiceImpl<BookMapper, Book> implements BookService {
}
