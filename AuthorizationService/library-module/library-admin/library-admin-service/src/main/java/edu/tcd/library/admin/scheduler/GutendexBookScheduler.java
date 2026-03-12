package edu.tcd.library.admin.scheduler;

import cn.hutool.core.convert.Convert;
import cn.hutool.core.util.ObjectUtil;
import cn.hutool.json.JSONArray;
import cn.hutool.json.JSONObject;
import cn.hutool.json.JSONUtil;
import edu.tcd.library.admin.entity.Book;
import edu.tcd.library.admin.service.BookService;
import edu.tcd.library.common.core.utils.RedisUtils;
import lombok.extern.slf4j.Slf4j;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.scheduling.annotation.Scheduled;
import org.springframework.stereotype.Component;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.TimeUnit;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

@Component
@Slf4j
public class GutendexBookScheduler {

    @Autowired
    private BookService bookService;

    //cached current page
    private final String BOOK_PAGE_CACHE_KEY = "Book:Page:Cache";
    //redis lock for writing, guarantee only one scheduled task
    private final String SCHEDULER_LOCK_KEY = "Book:Scheduler:Lock";

    private final RedisUtils redisService = new RedisUtils();

    private final OkHttpClient client = new OkHttpClient.Builder()
            .connectTimeout(60, TimeUnit.SECONDS)
            .readTimeout(60, TimeUnit.SECONDS)
            .writeTimeout(60, TimeUnit.SECONDS)
            .build();

    @Scheduled(cron = "0 */1 * * * ?")
    public void cronTask() {
        log.info("Triggering Gutendex Book Scheduler...");

        boolean isLocked = redisService.setIfAbsent(SCHEDULER_LOCK_KEY, "LOCKED", 4, TimeUnit.MINUTES);
        if (!isLocked) {
            log.warn("Previous Gutendex scheduler task is still running. Skipping this cycle to prevent overlap.");
            return;
        }

        try {
            Object cache = redisService.get(BOOK_PAGE_CACHE_KEY);
            int currentPage = ObjectUtil.isNotEmpty(cache) ? Convert.toInt(cache) : 1;

            //max page request cycle numbers in one schedule task
            int maxPagesPerCycle = 100;
            int pagesProcessed = 0;

            while (pagesProcessed < maxPagesPerCycle) {
                String currentUrl = String.format("https://gutendex.com/books/?page=%s&sort=ascending", currentPage);
                log.info("Fetching Gutendex page: {}", currentPage);

                String nextUrl = fetchAndSaveSinglePage(currentUrl);

                if (ObjectUtil.isNotEmpty(nextUrl)) {
                    currentPage = Convert.toInt(getNextPageNumber(nextUrl));
                    redisService.set(BOOK_PAGE_CACHE_KEY, String.valueOf(currentPage));
                    pagesProcessed++;

                    Thread.sleep(200);
                } else {
                    log.info("No more pages available or API reached the end.");
                    break;
                }
            }
        } catch (Exception e) {
            log.error("An unexpected error occurred during scheduled task execution.", e);
        } finally {
            redisService.delete(SCHEDULER_LOCK_KEY);
            log.info("Gutendex Book Scheduler cycle completed.");
        }
    }


    private String fetchAndSaveSinglePage(String targetUrl) {
        Request request = new Request.Builder()
                .url(targetUrl)
                .get()
                .header("Content-Type", "application/json")
                .build();

        try (Response response = client.newCall(request).execute()) {
            if (response.isSuccessful() && response.body() != null) {
                String responseData = response.body().string();
                JSONObject jsonObject = JSONUtil.parseObj(responseData);
                JSONArray results = jsonObject.getJSONArray("results");

                List<Book> gutendexBookList = new ArrayList<>();
                for (Object result : results) {
                    Book bean = JSONUtil.toBean(result.toString(), Book.class);
                    Book checkExist = bookService.getById(bean.getId());
                    if (checkExist == null) {
                        gutendexBookList.add(bean);
                    }
                }

                if (!gutendexBookList.isEmpty()) {
                    bookService.saveBatch(gutendexBookList);
                }

                return jsonObject.getStr("next");
            } else {
                log.error("API request failed. HTTP Status: {}", response.code());
            }
        } catch (Exception e) {
            log.error("An exception occurred while fetching/saving single page: " + targetUrl, e);
        }
        return null;
    }

    private String getNextPageNumber(String url) {
        Pattern pattern = Pattern.compile("page=(\\d+)");
        Matcher matcher = pattern.matcher(url);
        if (matcher.find()) {
            return matcher.group(1);
        } else {
            throw new IllegalArgumentException("Page number not found in URL: " + url);
        }
    }
}