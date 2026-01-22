package com.geoscene.topo.common.core.utils;

import ch.ethz.ssh2.Connection;
import ch.ethz.ssh2.Session;
import ch.ethz.ssh2.StreamGobbler;
import cn.hutool.core.util.StrUtil;
import com.geoscene.topo.common.core.exceptions.AuthenticationFailException;
import com.sun.jna.Platform;
import com.sun.jna.Pointer;
import com.sun.jna.platform.win32.Kernel32;
import com.sun.jna.platform.win32.WinNT;
import lombok.extern.slf4j.Slf4j;
import org.apache.logging.log4j.util.Strings;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.lang.reflect.Field;
import java.nio.charset.StandardCharsets;

@Slf4j
public class ExecuteUtil {

    public static String executeLocal(String cmd) {
        return executeWithAuth("localhost", null, null, cmd);
    }

    public static String executeWithAuth(String ip, String user, String password, String cmd) {
        String result = "";
        Connection conn = new Connection(ip);
        Session session = null;
        try {
            conn.connect();
            if (!Strings.isBlank(user) && !Strings.isBlank(password)) {
                boolean flg = conn.authenticateWithPassword(user, password);//认证
                if (!flg) {
                    log.error("登录失败:{}", ip);
                    throw new AuthenticationFailException("登录失败！");
                }
            }
            session = conn.openSession();
            session.execCommand(cmd);
            result = processStdout(session.getStdout());
            //如果为得到标准输出为空，说明脚本执行出错了
            if (Strings.isBlank(result)) {
                log.info("得到标准输出为空,链接conn:" + conn + ",执行的命令：" + cmd);
                result = processStdout(session.getStderr());
            } else {
                log.info("执行命令成功,链接conn:" + conn + ",执行的命令：" + cmd);
            }
        } catch (IOException e) {
            log.error(e.getMessage());
        } finally {
            conn.close();
            if (session != null) {
                session.close();
            }
        }
        return result;
    }

    /**
     * 解析脚本执行返回的结果集
     *
     * @param in 输入流对象
     * @return 以纯文本的格式返回
     */
    private static String processStdout(InputStream in) {
        InputStream stdout = new StreamGobbler(in);
        StringBuilder buffer = new StringBuilder();
        try {
            BufferedReader br = new BufferedReader(new InputStreamReader(stdout, StandardCharsets.UTF_8));
            String line = null;
            while ((line = br.readLine()) != null) {
                buffer.append(line).append("\n");
            }
        } catch (IOException e) {
            log.error("解析脚本出错：" + e.getMessage());
            e.printStackTrace();
        }
        return buffer.toString();
    }

    public static boolean testIpPort(String ip, Integer port) {
        String cmd = String.format("timeout 10 ssh root@%s netstat -aon|grep %s", ip, port);
        String result = executeLocal(cmd);
        return StrUtil.isNotBlank(result);
    }

    /**
     * 关闭Linux进程
     *
     * @param pid 进程的PID
     */
    public static boolean killProcessByPid(String pid) {
        if (StrUtil.isEmpty(pid) || "-1".equals(pid)) {
            throw new RuntimeException("Pid ==" + pid);
        }
        Process process = null;
        BufferedReader reader = null;
        String command = "";
        boolean result = false;
        if (Platform.isWindows()) {
            command = "cmd.exe /c taskkill /PID " + pid + " /F /T ";
        } else if (Platform.isLinux() || Platform.isAIX()) {
            command = "kill -9 " + pid;
        }
        try {
            //杀掉进程
            process = Runtime.getRuntime().exec(command);
            reader = new BufferedReader(new InputStreamReader(process.getInputStream(), "utf-8"));
            String line = null;
            while ((line = reader.readLine()) != null) {
                log.info("kill PID return info -----> " + line);
            }
            result = true;
        } catch (Exception e) {
            log.info("杀进程出错：", e);
            result = false;
        } finally {
            if (process != null) {
                process.destroy();
            }
            if (reader != null) {
                try {
                    reader.close();
                } catch (IOException e) {

                }
            }
        }
        return result;
    }


    /**
     * 获取win进程id
     *
     * @param process win进程
     * @return
     * @throws Exception
     */
    public static int getWinProcessId(Process process) throws Exception {
        Field f = process.getClass().getDeclaredField("handle");
        f.setAccessible(true);
        long handle = f.getLong(process);

        Kernel32 kernel = Kernel32.INSTANCE;
        WinNT.HANDLE winntHandle = new WinNT.HANDLE();
        winntHandle.setPointer(Pointer.createConstant(handle));
        return kernel.GetProcessId(winntHandle);
    }
}
