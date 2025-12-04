package com.geoscene.topo.common.minio.service;

import cn.hutool.core.date.DateTime;
import cn.hutool.core.date.DateUtil;
import cn.hutool.core.io.FastByteArrayOutputStream;
import cn.hutool.core.util.StrUtil;
import com.geoscene.topo.common.minio.MinioClientExtend;
import com.geoscene.topo.common.minio.domain.ItemObject;
import io.minio.*;
import io.minio.http.Method;
import io.minio.messages.Bucket;
import io.minio.messages.Item;
import jakarta.annotation.Resource;
import lombok.extern.slf4j.Slf4j;
import org.apache.commons.compress.utils.Lists;
import org.springframework.web.multipart.MultipartFile;

import jakarta.servlet.ServletOutputStream;
import jakarta.servlet.http.HttpServletResponse;
import java.io.File;
import java.io.InputStream;
import java.net.URLEncoder;
import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.List;
import java.util.Objects;
import java.util.UUID;

import static cn.hutool.core.util.ZipUtil.getZipOutputStream;
import static cn.hutool.core.util.ZipUtil.zip;

@Slf4j
public class MinioService {

    @Resource
    private MinioClientExtend minioClient;

    /**
     * 查看存储bucket是否存在
     *
     * @return boolean
     */
    public Boolean bucketExists(String bucketName) {
        boolean found = false;
        try {
            found = minioClient.bucketExists(BucketExistsArgs.builder().bucket(bucketName).build());
        } catch (Exception e) {
            e.printStackTrace();
        }
        return found;
    }

    /**
     * 查看存储bucket是否存在
     *
     * @return boolean
     */
    public Boolean bucketExists(MinioClient client, String bucketName) {
        boolean found = false;
        try {
            found = client.bucketExists(BucketExistsArgs.builder().bucket(bucketName).build());
        } catch (Exception e) {
            e.printStackTrace();
        }
        return found;
    }

    /**
     * 创建存储bucket
     *
     * @return Boolean
     */
    public void createBucket(String bucketName) throws Exception {
        minioClient.makeBucket(MakeBucketArgs.builder()
                .bucket(bucketName)
                .build());
    }

    /**
     * 创建存储bucket
     *
     * @return Boolean
     */
    public void createBucket(MinioClient client, String bucketName) throws Exception {
        client.makeBucket(MakeBucketArgs.builder()
                .bucket(bucketName)
                .build());
    }

    /**
     * 删除存储bucket
     *
     * @return Boolean
     */
    public Boolean removeBucket(String bucketName) {
        try {
            minioClient.removeBucket(RemoveBucketArgs.builder()
                    .bucket(bucketName)
                    .build());
        } catch (Exception e) {
            e.printStackTrace();
            return false;
        }
        return true;
    }

    /**
     * 删除存储bucket
     *
     * @return Boolean
     */
    public Boolean removeBucket(MinioClient client, String bucketName) {
        try {
            client.removeBucket(RemoveBucketArgs.builder()
                    .bucket(bucketName)
                    .build());
        } catch (Exception e) {
            e.printStackTrace();
            return false;
        }
        return true;
    }

    /**
     * 获取全部bucket
     */
    public List<Bucket> getAllBuckets() {
        try {
            return minioClient.listBuckets();
        } catch (Exception e) {
            e.printStackTrace();
        }
        return null;
    }


    /**
     * 获取全部bucket
     */
    public List<Bucket> getAllBuckets(MinioClient client) {
        try {
            return client.listBuckets();
        } catch (Exception e) {
            e.printStackTrace();
        }
        return null;
    }

    /**
     * stream文件上传
     *
     * @param stream           输入流
     * @param bucketName       桶名称
     * @param originalFilename 原始文件名称
     * @param fileSize         文件大小
     * @return String
     */
    public String upload(InputStream stream, String bucketName, String originalFilename, Long fileSize) {
        return upload(minioClient, stream, bucketName, originalFilename, fileSize);
    }


    /**
     * stream文件上传
     *
     * @param clients          minio client
     * @param stream           输入流
     * @param bucketName       桶名称
     * @param originalFilename 原始文件名称
     * @param fileSize         文件大小
     * @return String
     */
    public String upload(MinioClientExtend clients, InputStream stream, String bucketName, String originalFilename,
                         Long fileSize) {
        String fileName = UUID.randomUUID() + originalFilename.substring(originalFilename.lastIndexOf("."));
        String objectName = DateUtil.format(DateTime.now(), "yyyy-MM-dd") + "/" + fileName;
        try {
            PutObjectArgs objectArgs = PutObjectArgs.builder().bucket(bucketName).object(objectName)
                    .stream(stream, fileSize, -1).build();
            //文件名称相同会覆盖
            clients.putObject(objectArgs);
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
        return objectName;
    }

    /**
     * 文件上传
     *
     * @param file 文件
     * @return String
     */
    public String upload(MultipartFile file, String bucketName) {
        String originalFilename = file.getOriginalFilename();
        if (StrUtil.isBlank(originalFilename)) {
            throw new RuntimeException();
        }
        String fileName = UUID.randomUUID() + originalFilename.substring(originalFilename.lastIndexOf("."));
        String objectName = DateUtil.format(DateTime.now(), "yyyy-MM-dd") + "/" + fileName;
        try {
            PutObjectArgs objectArgs = PutObjectArgs.builder().bucket(bucketName).object(objectName)
                    .stream(file.getInputStream(), file.getSize(), -1).contentType(file.getContentType()).build();
            //文件名称相同会覆盖
            minioClient.putObject(objectArgs);
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
        return objectName;
    }

    /**
     * 拷贝文件
     *
     * @param bucketName
     * @param objectName
     * @param srcBucketName
     * @param srcObjectName
     * @return
     */
    public Boolean copy(String srcBucketName, String srcObjectName, String bucketName, String objectName) {
        try {
            minioClient.copyObject(CopyObjectArgs.builder().bucket(bucketName).object(objectName).source(CopySource.builder().bucket(srcBucketName).object(srcObjectName).build()).build());
            return true;
        } catch (Exception e) {
            e.printStackTrace();
            return false;
        }
    }

    /**
     * 预览图片
     *
     * @param fileName
     * @return
     */
    public String preview(String fileName, String bucketName) {
        // 查看文件地址
        GetPresignedObjectUrlArgs build =
                GetPresignedObjectUrlArgs.builder().bucket(bucketName).object(fileName).method(Method.GET).build();
        try {
            return minioClient.getPresignedObjectUrl(build);
        } catch (Exception e) {
            e.printStackTrace();
        }
        return null;
    }

    /**
     * 文件下载
     *
     * @param path 文件路径
     * @param res  response
     * @return Boolean
     */
    public void download(String path, String bucketName, HttpServletResponse res) {
        GetObjectArgs objectArgs = GetObjectArgs.builder().bucket(bucketName)
                .object(path).build();
        try (GetObjectResponse response = minioClient.getObject(objectArgs)) {
            byte[] buf = new byte[1024];
            int len;
            try (FastByteArrayOutputStream os = new FastByteArrayOutputStream()) {
                while ((len = response.read(buf)) != -1) {
                    os.write(buf, 0, len);
                }
                os.flush();
                byte[] bytes = os.toByteArray();
                res.setCharacterEncoding("utf-8");
                String filename = path.substring(path.lastIndexOf("/"));
                // 设置强制下载不打开
                // res.setContentType("application/force-download");
                res.addHeader("Content-Disposition",
                        " attachment;filename=" + URLEncoder.encode(filename, "UTF-8"));
                res.setContentType("application/octet-stream");
                try (ServletOutputStream stream = res.getOutputStream()) {
                    stream.write(bytes);
                    stream.flush();
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    /**
     * 文件下载
     *
     * @param paths      文件路径
     * @param fileNames  文件名称列表
     * @param bucketName bucket名称
     * @param datasetId  数据集id
     * @param res        response
     */
    public void downloadZip(List<String> paths, List<String> fileNames, String bucketName, String datasetId,
                            HttpServletResponse res) {
        try {
            InputStream[] srcFiles = new InputStream[paths.size()];
            for (int i = 0; i < paths.size(); i++) {
                String path = paths.get(i);
                GetObjectArgs objectArgs = GetObjectArgs.builder().bucket(bucketName)
                        .object(path).build();
                InputStream inputStream = minioClient.getObject(objectArgs);
                if (inputStream == null) {
                    continue;
                }
                srcFiles[i] = inputStream;
            }
            res.setCharacterEncoding("UTF-8");
            res.setHeader("Content-Disposition", "attachment;filename=" + URLEncoder.encode(datasetId + ".zip", "UTF-8"));
            //多个文件压缩成压缩包返回
            zip(getZipOutputStream(res.getOutputStream(), StandardCharsets.UTF_8), fileNames.toArray(new String[]{}),
                    srcFiles);
        } catch (Exception ex) {
            log.error(ex.getMessage());
            throw new RuntimeException("文件集下载失败！");
        }

    }

    /**
     * 文件下载到本地
     *
     * @param paths
     * @param fileNames
     * @param bucketName
     * @param zipPath
     */
    public void downloadZipLocal(List<String> paths, List<String> fileNames, String bucketName, String zipPath) {
        try {
            InputStream[] srcFiles = new InputStream[paths.size()];
            for (int i = 0; i < paths.size(); i++) {
                String path = paths.get(i);
                GetObjectArgs objectArgs = GetObjectArgs.builder().bucket(bucketName)
                        .object(path).build();
                InputStream inputStream = minioClient.getObject(objectArgs);
                if (inputStream == null) {
                    continue;
                }
                srcFiles[i] = inputStream;
            }
            File zipFile = new File(zipPath);
            //多个文件压缩成压缩包返回
            zip(zipFile, fileNames.toArray(new String[]{}), srcFiles, Charset.forName("utf-8"));
        } catch (Exception ex) {
            log.error(ex.getMessage());
            throw new RuntimeException("文件集下载失败！");
        }

    }

    /**
     * 文件下载
     *
     * @param path
     * @param bucketName
     * @param filePath
     * @return
     */
    public String downloadLocal(String path, String bucketName, String filePath) {
        GetObjectArgs objectArgs = GetObjectArgs.builder().bucket(bucketName)
                .object(path).build();
        try (GetObjectResponse response = minioClient.getObject(objectArgs)) {
            byte[] buf = new byte[1024];
            int len;
            try (FastByteArrayOutputStream os = new FastByteArrayOutputStream()) {
                while ((len = response.read(buf)) != -1) {
                    os.write(buf, 0, len);
                }
                os.flush();
                byte[] bytes = os.toByteArray();
                Files.write(Paths.get(filePath), bytes);
                return filePath;
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
        return null;
    }

    /**
     * 查看文件对象
     *
     * @return 存储bucket内文件对象信息
     */
    public List<Item> listObjects(String bucketName) {
        Iterable<Result<Item>> results = minioClient.listObjects(
                ListObjectsArgs.builder().bucket(bucketName).build());
        List<Item> items = new ArrayList<>();
        try {
            for (Result<Item> result : results) {
                items.add(result.get());
            }
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
        return items;
    }

    public List<ItemObject> listRootObjects(String bucketName) {
        List<ItemObject> list = Lists.newArrayList();
        Iterable<Result<Item>> results = minioClient.listObjects(
                ListObjectsArgs.builder()
                        .bucket(bucketName)
                        .prefix("")
                        .recursive(false).build());
        try {
            for (Result<Item> itemResult : results) {
                Item item = itemResult.get();
                if (item.isDir()) {
                    ItemObject folder = ItemObject.builder()
                            .isFolder(Boolean.TRUE)
                            .name(item.objectName()).build();
                    list.add(folder);
                } else {
                    DateTimeFormatter dtf = DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm:ss");
                    //过滤空对象和空文件path
                    if (item.size() > 0) {
                        list.add(ItemObject.builder()
                                .isFolder(Boolean.FALSE)
                                .name(item.objectName())
                                .lastModified(
                                        Objects.isNull(item.lastModified()) ?
                                                null : dtf.format(item.lastModified().toLocalDateTime()))
                                .size(item.size())
                                .build());
                    }
                }
            }
        } catch (Exception e) {
            log.error("获取bucket下文件信息异常", e);
        }
        return list;
    }

    /**
     * 获取bucket下 所有文件夹和文件目录树
     *
     * @param bucketName bucket
     * @param path       objectName当前文件夹名称
     * @param parent     当前路径父节点
     * @return 树状结构
     */
    public List<ItemObject> listObjects(String bucketName, String path, ItemObject parent) {
        List<ItemObject> list = Lists.newArrayList();
        Iterable<Result<Item>> results = minioClient.listObjects(
                ListObjectsArgs.builder()
                        .bucket(bucketName)
                        .prefix(path)
                        .recursive(false).build());
        try {
            for (Result<Item> itemResult : results) {
                Item item = itemResult.get();
                if (item.isDir()) {
                    ItemObject folder = ItemObject.builder()
                            .isFolder(Boolean.TRUE)
                            .name(item.objectName()).build();
                    folder.setChildren(listObjects(bucketName, item.objectName(), folder));
                    list.add(folder);
                } else {
                    DateTimeFormatter dtf = DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm:ss");
                    //过滤空对象和空文件path
                    if (item.size() > 0) {
                        list.add(ItemObject.builder()
                                .isFolder(Boolean.FALSE)
                                .name(item.objectName())
                                .lastModified(
                                        Objects.isNull(item.lastModified()) ?
                                                null : dtf.format(item.lastModified().toLocalDateTime()))
                                .size(item.size())
                                .build());
                    }
                }
            }
        } catch (Exception e) {
            log.error("获取bucket下文件信息异常", e);
        }
        return list;
    }

    /**
     * 判断文件夹是否存在
     *
     * @param bucketName 存储桶
     * @param objectName 文件夹名称（去掉/）
     * @return true：存在
     */
    public boolean isFolderExist(String bucketName, String objectName) {
        boolean exist = false;
        try {
            Iterable<Result<Item>> results = minioClient.listObjects(
                    ListObjectsArgs.builder()
                            .bucket(bucketName)
                            .prefix(objectName)
                            .recursive(false).build());
            for (Result<Item> result : results) {
                Item item = result.get();
                if (objectName.equals(item.objectName())) {
                    exist = true;
                    break;
                }
            }
        } catch (Exception e) {
            exist = false;
        }
        return exist;
    }

    /**
     * 判断文件是否存在
     *
     * @param bucketName 桶名称
     * @param objectName 文件名称, 如果要带文件夹请用 / 分割, 例如 /help/index.html
     * @return true存在, 反之
     */
    public Boolean isFileExist(String bucketName, String objectName) {
        try {
            minioClient.statObject(
                    StatObjectArgs.builder().bucket(bucketName).object(objectName).build()
            );
        } catch (Exception e) {
            return false;
        }
        return true;
    }


    /**
     * 删除
     *
     * @param fileName
     * @return
     * @throws Exception
     */
    public boolean remove(String fileName, String bucketName) {
        try {
            minioClient.removeObject(RemoveObjectArgs.builder().bucket(bucketName).object(fileName).build());
        } catch (Exception e) {
            return false;
        }
        return true;
    }


    /**
     * 获取文件信息
     *
     * @param bucketName bucket名称
     * @param objectName 文件名称
     * @throws Exception
     */
    public StatObjectResponse getObjectInfo(String bucketName, String objectName) throws Exception {
        return minioClient.statObject(StatObjectArgs.builder()
                .bucket(bucketName)
                .object(objectName)
                .build());
    }

}
