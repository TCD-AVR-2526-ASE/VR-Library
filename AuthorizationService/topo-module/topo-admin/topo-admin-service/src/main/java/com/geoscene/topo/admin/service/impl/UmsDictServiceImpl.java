package com.geoscene.topo.admin.service.impl;

import cn.hutool.core.util.ObjectUtil;
import com.baomidou.mybatisplus.core.conditions.query.LambdaQueryWrapper;
import com.baomidou.mybatisplus.extension.service.impl.ServiceImpl;
import com.geoscene.topo.admin.entity.UmsDict;
import com.geoscene.topo.admin.entity.UmsDictNode;
import com.geoscene.topo.admin.mapper.UmsDictMapper;
import com.geoscene.topo.admin.service.UmsDictService;
import org.springframework.beans.BeanUtils;
import org.springframework.stereotype.Service;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Optional;
import java.util.stream.Collectors;

@Service
public class UmsDictServiceImpl extends ServiceImpl<UmsDictMapper, UmsDict> implements UmsDictService {

    @Override
    public Boolean saveDict(UmsDict dict) {
        LambdaQueryWrapper<UmsDict> queryWrapper = new LambdaQueryWrapper<>();
        queryWrapper.eq(UmsDict::getCode, dict.getCode());
        long iCount = this.baseMapper.selectCount(queryWrapper);
        if (iCount > 0) {
            throw new RuntimeException("字典code重复!");
        }
        int insert = this.baseMapper.insert(dict);
        return insert > 0;
    }

    @Override
    public List<UmsDict> listByCode(String code) {
        UmsDict umsDict = getItemByCode(code);
        LambdaQueryWrapper<UmsDict> dictNodeLambda = new LambdaQueryWrapper<>();
        dictNodeLambda.eq(UmsDict::getParentId, umsDict.getId());
        return this.baseMapper.selectList(dictNodeLambda);
    }

    @Override
    public UmsDict getItemByCode(String code) {
        LambdaQueryWrapper<UmsDict> lambda = new LambdaQueryWrapper<>();
        lambda.eq(UmsDict::getCode, code);
        UmsDict umsDict = this.baseMapper.selectOne(lambda);
        if (ObjectUtil.isNull(umsDict)) {
            throw new RuntimeException("字典不存在！");
        }
        return umsDict;
    }

    @Override
    public List<UmsDictNode> treeList() {
        List<UmsDict> dictList = this.baseMapper.selectList(null);
        return dictList.stream()
                .filter(dict -> dict.getParentId().equals(0L))
                .map(dict -> convertDictNode(dict, dictList)).collect(Collectors.toList());
    }

    @Override
    public List<UmsDictNode> treeList(String code) {
        List<UmsDict> dictList = this.baseMapper.selectList(null);
        return dictList.stream()
                .filter(dict -> dict.getCode().equals(code))
                .map(dict -> convertDictNode(dict, dictList)).collect(Collectors.toList());
    }

    /**
     * 将UmsDict转化为UmsDictNode并设置children属性
     */
    private UmsDictNode convertDictNode(UmsDict dict, List<UmsDict> dictList) {
        UmsDictNode node = new UmsDictNode();
        BeanUtils.copyProperties(dict, node);
        List<UmsDictNode> children = dictList.stream()
                .filter(subDict -> subDict.getParentId().equals(dict.getId()))
                .map(subDict -> convertDictNode(subDict, dictList)).collect(Collectors.toList());
        node.setChildren(children);
        return node;
    }

    @Override
    public List<UmsDict> listAllWithCode(String code) {
        List<UmsDict> dictList = this.baseMapper.selectList(null);
        List<UmsDict> umsDictList = new ArrayList<>();
        Optional<UmsDict> optional = dictList.stream()
                .filter(dict -> dict.getCode().equals(code)).findFirst();
        if (optional.isPresent()) {
            UmsDict dictRoot = optional.get();
            umsDictList.add(dictRoot);
            getChildDictNode(dictRoot, umsDictList, dictList);
        }
        return umsDictList;
    }

    private void getChildDictNode(UmsDict dictRoot, List<UmsDict> umsDictList, List<UmsDict> dictList) {
        dictList.stream()
                .filter(subDict -> subDict.getParentId().equals(dictRoot.getId()))
                .forEach(subDict -> {
                    umsDictList.add(subDict);
                    getChildDictNode(subDict, umsDictList, dictList);
                });
    }
}
