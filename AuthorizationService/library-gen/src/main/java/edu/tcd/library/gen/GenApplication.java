package edu.tcd.library.gen;

import com.github.davidfantasy.mybatisplus.generatorui.GeneratorConfig;
import com.github.davidfantasy.mybatisplus.generatorui.MybatisPlusToolsApplication;
import com.github.davidfantasy.mybatisplus.generatorui.mbp.NameConverter;

public class GenApplication {

    public static void main(String[] args) {
        GeneratorConfig config = GeneratorConfig.builder().jdbcUrl("jdbc:postgresql://10.44.94.43:5432/topo")
                .userName("postgres")
                .password("123456")
                .driverClassName("org.postgresql.Driver")
                // Database schema; required for MSSQL, PGSQL, ORACLE, and DB2 databases
                .schemaName("public")
                // Database table prefix, removed when generating entity names (added in v2.0.3)
                .tablePrefix("")
                // To customize naming rules for entities, attributes, or generated files, define a NameConverter instance
                // and override the corresponding methods. Refer to the interface documentation for details:
                .nameConverter(new NameConverter() {
                    /**
                     * Custom naming rule for Service classes.
                     * entityName is the result after processing the table name via NameConverter.entityNameConvert.
                     * Customize implementation if specific requirements exist.
                     */
                    @Override
                    public String serviceNameConvert(String entityName) {
                        return entityName + "Service";
                    }

                    /**
                     * Custom naming rule for Controller classes
                     */
                    @Override
                    public String controllerNameConvert(String entityName) {
                        return entityName + "Controller";
                    }
                })
                // Base package name for all generated Java files; can also be configured in the UI later
                .basePackage("edu.tcd.library.gen.output")
                .port(8068)
                .build();
        MybatisPlusToolsApplication.run(config);
    }
}