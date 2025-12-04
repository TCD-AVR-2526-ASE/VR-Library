package com.geoscene.topo.admin.service;

import com.baomidou.mybatisplus.extension.service.IService;
import com.geoscene.topo.admin.entity.UmsDict;
import com.geoscene.topo.admin.entity.UmsDictNode;

import java.util.List;

public interface UmsDictService extends IService<UmsDict> {

    /**
     * 保存字典信息
     *
     * @param dict 字典信息
     * @return
     */
    Boolean saveDict(UmsDict dict);

    /**
     * 根据字典编码获取字典值列表
     *
     * @param code 字典编码
     * @return
     */
    List<UmsDict> listByCode(String code);

    /**
     * 根据字典code获取字典信息
     *
     * @param code 字典code
     * @return
     */
    UmsDict getItemByCode(String code);

    /**
     * 获取字典树
     *
     * @return 字典树
     */
    List<UmsDictNode> treeList();

    /**
     * 获取字典子树
     *
     * @param code 当前节点code
     * @return 字典树
     */
    List<UmsDictNode> treeList(String code);

    /**
     * 根据父code获取字典子列表
     * <p>
     * 即获取父字典和其子字典以及其对应的子节点组成列表
     *
     * @param code 父code
     * @return 字典列表
     */
    List<UmsDict> listAllWithCode(String code);
}
