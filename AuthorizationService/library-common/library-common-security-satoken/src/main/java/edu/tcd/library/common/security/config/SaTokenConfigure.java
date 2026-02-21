package edu.tcd.library.common.security.config;

import cn.dev33.satoken.interceptor.SaInterceptor;
import cn.dev33.satoken.router.SaRouter;
import cn.dev33.satoken.stp.StpUtil;
import org.springframework.context.annotation.Configuration;
import org.springframework.web.servlet.config.annotation.InterceptorRegistry;
import org.springframework.web.servlet.config.annotation.WebMvcConfigurer;

@Configuration
public class SaTokenConfigure implements WebMvcConfigurer {

    private final IgnoreUrlsConfig ignoreUrlsConfig;

    public SaTokenConfigure(IgnoreUrlsConfig ignoreUrlsConfig) {
        this.ignoreUrlsConfig = ignoreUrlsConfig;
    }

    // register Sa-Token interceptor
    @Override
    public void addInterceptors(InterceptorRegistry registry) {
        registry.addInterceptor(new SaInterceptor(handler -> {
            // match rules
            SaRouter
                    .match("/**")    // block all first
                    .notMatch(ignoreUrlsConfig.getUrls())        // ignore list
                    .check(r -> StpUtil.checkLogin());        // check action

            SaRouter.match("/ums/**", r -> StpUtil.checkRole("admin"));
        })).addPathPatterns("/**");
    }
}
