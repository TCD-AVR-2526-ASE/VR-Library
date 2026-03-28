package edu.tcd.library.tools;


import cn.hutool.json.JSONArray;
import cn.hutool.json.JSONObject;
import cn.hutool.json.JSONUtil;
import lombok.Data;
import okhttp3.*;
import org.junit.jupiter.api.Test;
import org.springframework.util.Assert;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardOpenOption;
import java.util.ArrayList;
import java.util.List;

public class GutendexBookTest {

    private final OkHttpClient client = new OkHttpClient();

    @Test
    void testParseBookJson() {
        String json = """
                {
                    "count": 77849,
                    "next": "https://gutendex.com/books/?page=3",
                    "previous": "https://gutendex.com/books/",
                    "results": [
                        {
                            "id": 74,
                            "title": "The Adventures of Tom Sawyer, Complete",
                            "authors": [
                                {
                                    "name": "Twain, Mark",
                                    "birth_year": 1835,
                                    "death_year": 1910
                                }
                            ],
                            "summaries": [
                                "\\"The Adventures of Tom Sawyer, Complete\\" by Mark Twain is a novel published in 1876 about a mischievous boy growing up along the Mississippi River in the 1830s-1840s. Tom Sawyer and his friend Huckleberry Finn navigate childhood adventures that take increasingly dangerous turns when they witness a murder in a graveyard. Sworn to secrecy and living in fear, the boys must decide whether to speak the truth as an innocent man faces trial, while a vengeful killer remains free. (This is an automatically generated summary.)"
                            ],
                            "editors": [],
                            "translators": [],
                            "subjects": [
                                "Adventure stories",
                                "Bildungsromans",
                                "Boys -- Fiction",
                                "Child witnesses -- Fiction",
                                "Humorous stories",
                                "Male friendship -- Fiction",
                                "Mississippi River Valley -- Fiction",
                                "Missouri -- Fiction",
                                "Runaway children -- Fiction",
                                "Sawyer, Tom (Fictitious character) -- Fiction"
                            ],
                            "bookshelves": [
                                "Banned Books List from the American Library Association",
                                "Banned Books from Anne Haight's list",
                                "Category: Adventure",
                                "Category: American Literature",
                                "Category: Classics of Literature",
                                "Category: Novels"
                            ],
                            "languages": [
                                "en"
                            ],
                            "copyright": false,
                            "media_type": "Text",
                            "formats": {
                                "text/html": "https://www.gutenberg.org/ebooks/74.html.images",
                                "application/epub+zip": "https://www.gutenberg.org/ebooks/74.epub3.images",
                                "application/x-mobipocket-ebook": "https://www.gutenberg.org/ebooks/74.kf8.images",
                                "application/rdf+xml": "https://www.gutenberg.org/ebooks/74.rdf",
                                "image/jpeg": "https://www.gutenberg.org/cache/epub/74/pg74.cover.medium.jpg",
                                "application/octet-stream": "https://www.gutenberg.org/files/74/74-0.zip",
                                "text/plain; charset=utf-8": "https://www.gutenberg.org/ebooks/74.txt.utf-8",
                                "text/plain; charset=us-ascii": "https://www.gutenberg.org/files/74/74-0.txt"
                            },
                            "download_count": 31378
                        },
                        {
                            "id": 1080,
                            "title": "A Modest Proposal: For preventing the children of poor people in Ireland, from being a burden on their parents or country, and for making them beneficial to the publick",
                            "authors": [
                                {
                                    "name": "Swift, Jonathan",
                                    "birth_year": 1667,
                                    "death_year": 1745
                                }
                            ],
                            "summaries": [
                                "\\"A Modest Proposal\\" by Jonathan Swift is a satirical essay written and published in 1729. The work shockingly suggests that Ireland's poor could solve their economic troubles by selling their children as food to the wealthy. Through sustained irony and deadpan humor, Swift uses this outrageous premise to mock hostile attitudes toward the poor and expose the dehumanizing policies of British colonial rule. The essay remains celebrated for its dark wit and biting social commentary. (This is an automatically generated summary.)"
                            ],
                            "editors": [],
                            "translators": [],
                            "subjects": [
                                "Ireland -- Politics and government -- 18th century -- Humor",
                                "Political satire, English",
                                "Religious satire, English"
                            ],
                            "bookshelves": [
                                "Category: British Literature",
                                "Category: Classics of Literature",
                                "Category: Essays, Letters & Speeches"
                            ],
                            "languages": [
                                "en"
                            ],
                            "copyright": false,
                            "media_type": "Text",
                            "formats": {
                                "text/html": "https://www.gutenberg.org/ebooks/1080.html.images",
                                "application/epub+zip": "https://www.gutenberg.org/ebooks/1080.epub3.images",
                                "application/x-mobipocket-ebook": "https://www.gutenberg.org/ebooks/1080.kf8.images",
                                "application/rdf+xml": "https://www.gutenberg.org/ebooks/1080.rdf",
                                "image/jpeg": "https://www.gutenberg.org/cache/epub/1080/pg1080.cover.medium.jpg",
                                "application/octet-stream": "https://www.gutenberg.org/cache/epub/1080/pg1080-h.zip",
                                "text/plain; charset=utf-8": "https://www.gutenberg.org/ebooks/1080.txt.utf-8",
                                "text/plain; charset=us-ascii": "https://www.gutenberg.org/files/1080/1080-0.txt"
                            },
                            "download_count": 31260
                        },
                        {
                            "id": 2542,
                            "title": "A Doll's House : a play",
                            "authors": [
                                {
                                    "name": "Ibsen, Henrik",
                                    "birth_year": 1828,
                                    "death_year": 1906
                                }
                            ],
                            "summaries": [
                                "\\"A Doll's House : a play by Henrik Ibsen\\" is a three-act play written in 1879. Set in a Norwegian town, it follows Nora Helmer, a married woman navigating life in a male-dominated society where opportunities for self-fulfillment are scarce. When a figure from her past threatens to expose a secret financial transgression, Nora faces a crisis that challenges everything she knows about her marriage and identity. The play sparked outraged controversy upon its premiere and remains one of the most performed works in theater history. (This is an automatically generated summary.)"
                            ],
                            "editors": [],
                            "translators": [
                                {
                                    "name": "Sharp, R. Farquharson (Robert Farquharson)",
                                    "birth_year": 1864,
                                    "death_year": 1945
                                }
                            ],
                            "subjects": [
                                "Man-woman relationships -- Drama",
                                "Marriage -- Drama",
                                "Norwegian drama -- Translations into English",
                                "Wives -- Drama"
                            ],
                            "bookshelves": [
                                "Best Books Ever Listings",
                                "Category: Classics of Literature",
                                "Category: Plays/Films/Dramas"
                            ],
                            "languages": [
                                "en"
                            ],
                            "copyright": false,
                            "media_type": "Text",
                            "formats": {
                                "text/html": "https://www.gutenberg.org/ebooks/2542.html.images",
                                "application/epub+zip": "https://www.gutenberg.org/ebooks/2542.epub3.images",
                                "application/x-mobipocket-ebook": "https://www.gutenberg.org/ebooks/2542.kf8.images",
                                "application/rdf+xml": "https://www.gutenberg.org/ebooks/2542.rdf",
                                "image/jpeg": "https://www.gutenberg.org/cache/epub/2542/pg2542.cover.medium.jpg",
                                "application/octet-stream": "https://www.gutenberg.org/files/2542/2542-0.zip",
                                "text/plain; charset=utf-8": "https://www.gutenberg.org/ebooks/2542.txt.utf-8",
                                "text/plain; charset=us-ascii": "https://www.gutenberg.org/files/2542/2542-0.txt"
                            },
                            "download_count": 30565
                        }
                    ]
                }
                
                
                """;
        JSONObject jsonObject = JSONUtil.parseObj(json);
        JSONArray results = jsonObject.getJSONArray("results");
        List<GutendexBook> gutendexBookList = new ArrayList<>();
        for (Object result : results) {
            GutendexBook bean = JSONUtil.toBean(result.toString(), GutendexBook.class);
            gutendexBookList.add(bean);
        }
        Assert.notNull(gutendexBookList, "GutendexBook list is null");
        Assert.isTrue(gutendexBookList.size() == 3, "book sizes is not equal");
        Assert.isTrue(gutendexBookList.get(0).getId().equals(74), "book information is not equal");
    }

    @Test
    void testSendRequest() {
        String url = "https://gutendex.com/books/?page=3";
        Request request = new Request.Builder()
                .url(url)
                .get()
                .header("Content-Type", "application/json")
                .build();

        try (Response response = client.newCall(request).execute()) {
            if (!response.isSuccessful()) {
                System.err.println("request failed: " + response.code());
                if (response.body() != null) {
                    System.err.println("error msg: " + response.body().string());
                }
                return;
            }

            Assert.notNull(response.body(), "response body is null");
            if (response.body() != null) {
                String responseData = response.body().string();
                System.out.println("success, res: " + responseData);
            }
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    @Test
    void testSendRequestAndParseResponse() {
        String url = "https://gutendex.com/books/?page=3";
        Request request = new Request.Builder()
                .url(url)
                .get()
                .header("Content-Type", "application/json")
                .build();

        try (Response response = client.newCall(request).execute()) {
            if (!response.isSuccessful()) {
                System.err.println("request failed: " + response.code());
                if (response.body() != null) {
                    System.err.println("error msg: " + response.body().string());
                }
                return;
            }

            Assert.notNull(response.body(), "response body is null");
            if (response.body() != null) {
                String responseData = response.body().string();
                JSONObject jsonObject = JSONUtil.parseObj(responseData);
                JSONArray results = jsonObject.getJSONArray("results");
                List<GutendexBook> gutendexBookList = new ArrayList<>();
                for (Object result : results) {
                    GutendexBook bean = JSONUtil.toBean(result.toString(), GutendexBook.class);
                    gutendexBookList.add(bean);
                }
                Assert.notNull(gutendexBookList, "GutendexBook list is null");

            }
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    @Test
    void testBatchFetchAndSaveToFile() {
        int startPage = 1;
        int endPage = 5;

        Path outputPath = Paths.get("gutendex_responses.txt");

        System.out.println("Start batch file fetching，target file: " + outputPath.toAbsolutePath());

        try {
            Files.deleteIfExists(outputPath);
            Files.createFile(outputPath);

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
                        String contentToWrite = "=== PAGE " + page + " ===\n" + responseData + "\n\n";

                        Files.writeString(
                                outputPath,
                                contentToWrite,
                                StandardOpenOption.APPEND
                        );

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

}


@Data
class GutendexBook {
    Integer id;
    String title;
    JSONArray authors;
}