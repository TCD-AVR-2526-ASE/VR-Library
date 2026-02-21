package edu.tcd.library.common.mybatis.handler;

import org.apache.ibatis.type.JdbcType;
import org.apache.ibatis.type.MappedJdbcTypes;
import org.apache.ibatis.type.MappedTypes;
import org.apache.ibatis.type.TypeHandler;

import java.sql.CallableStatement;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;


@MappedJdbcTypes(JdbcType.VARCHAR)
@MappedTypes({List.class})
public class ListToStringTypeHandler implements TypeHandler<List<String>> {

    @Override
    public void setParameter(PreparedStatement ps, int i, List<String> parameter, JdbcType jdbcType) throws SQLException {
        if (parameter == null || parameter.isEmpty()) {
            ps.setString(i, "");
        } else {
            ps.setString(i, String.join(",", parameter));
        }
    }

    @Override
    public List<String> getResult(ResultSet rs, String columnName) throws SQLException {
        String str = rs.getString(columnName);
        if (str == null || str.isEmpty()) {
            return new ArrayList<>();
        } else {
            return Arrays.asList(str.split(","));
        }
    }

    @Override
    public List<String> getResult(ResultSet rs, int columnIndex) throws SQLException {
        String str = rs.getString(columnIndex);
        if (str == null || str.isEmpty()) {
            return new ArrayList<>();
        } else {
            return Arrays.asList(str.split(","));
        }
    }

    @Override
    public List<String> getResult(CallableStatement cs, int columnIndex) throws SQLException {
        String str = cs.getString(columnIndex);
        if (str == null || str.isEmpty()) {
            return new ArrayList<>();
        } else {
            return Arrays.asList(str.split(","));
        }
    }
}
