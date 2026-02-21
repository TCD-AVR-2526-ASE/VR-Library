package edu.tcd.library.common.mybatis.handler;

import org.apache.ibatis.type.JdbcType;
import org.apache.ibatis.type.TypeHandler;

import java.sql.CallableStatement;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.List;

public class BooleanToIntTypeHandler implements TypeHandler<Boolean> {
    @Override
    public void setParameter(PreparedStatement ps, int i, Boolean parameter, JdbcType jdbcType) throws SQLException {
        if (parameter == null) {
            ps.setInt(i, 0);
        } else {
            ps.setInt(i, parameter ? 1 : 0);
        }
    }

    @Override
    public Boolean getResult(ResultSet rs, String columnName) throws SQLException {
        int result = rs.getInt(columnName);
        return result != 0;
    }

    @Override
    public Boolean getResult(ResultSet rs, int columnIndex) throws SQLException {
        int result = rs.getInt(columnIndex);
        return result != 0;
    }

    @Override
    public Boolean getResult(CallableStatement cs, int columnIndex) throws SQLException {
        int result = cs.getInt(columnIndex);
        return result != 0;
    }
}
