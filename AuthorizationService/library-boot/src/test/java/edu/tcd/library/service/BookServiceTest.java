package edu.tcd.library.service;

import cn.hutool.json.JSONArray;
import cn.hutool.json.JSONObject;
import cn.hutool.json.JSONUtil;
import edu.tcd.library.admin.entity.Book;
import edu.tcd.library.admin.scheduler.GutendexBookScheduler;
import edu.tcd.library.admin.service.BookService;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.util.Assert;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

@SpringBootTest
public class BookServiceTest {

    @Autowired
    private BookService bookService;

    @Autowired
    private GutendexBookScheduler scheduler;

    private final OkHttpClient client = new OkHttpClient();

    @Test
    void testAddBook() {
        Book book = new Book();
        book.setId(1);
        book.setTitle("sss");
        bookService.save(book);
    }

    @Test
    void testBatchFetchAndSaveToFile() {
        int startPage = 1;
        int endPage = 2;
        try {
            for (int page = startPage; page <= endPage; page++) {
                String url = "https://gutendex.com/books/?page=" + page;
                System.out.println("requesting: " + url);

                Request request = new Request.Builder()
                        .url(url)
                        .get()
                        .header("Content-Type", "application/json")
                        .build();

                try (Response response = client.newCall(request).execute()) {
                    if (!response.isSuccessful()) {
                        System.err.println("get " + page + " page failed, status code: " + response.code());
                        continue;
                    }

                    if (response.body() != null) {
                        String responseData = response.body().string();
                        JSONObject jsonObject = JSONUtil.parseObj(responseData);
                        JSONArray results = jsonObject.getJSONArray("results");
                        List<Book> gutendexBookList = new ArrayList<>();
                        for (Object result : results) {
                            Book bean = JSONUtil.toBean(result.toString(), Book.class);
                            gutendexBookList.add(bean);
                        }
                        Assert.notNull(gutendexBookList, "GutendexBook list is null");

                        bookService.saveBatch(gutendexBookList);

                        System.out.println(page + " page data saved successfully。");
                    }
                }

                if (page < endPage) {
                    Thread.sleep(1500);
                }
            }

            System.out.println("Finish batch file fetching！");

        } catch (IOException | InterruptedException e) {
            System.err.println("An exception occurred during batch requests or file writing.");
            e.printStackTrace();
        }
    }

    @Test
    void testSaveFromLatestPage(){
        scheduler.cronTask();
    }
}
